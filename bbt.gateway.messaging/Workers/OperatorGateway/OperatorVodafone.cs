using bbt.gateway.messaging.Api.Vodafone;
using bbt.gateway.messaging.Api.Vodafone.Model;
using bbt.gateway.messaging.Models;
using bbt.gateway.messaging.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;

namespace bbt.gateway.messaging.Workers.OperatorGateway
{
    public class OperatorVodafone : OperatorGatewayBase, IOperatorGateway
    {
        private string _authToken;
        private readonly VodafoneApi _vodafoneApi;
        public OperatorVodafone(VodafoneApi vodafoneApi)
        {
            _vodafoneApi = vodafoneApi;
            Type = OperatorType.Vodafone;
        }

        private async Task<bool> Auth()
        {
            bool isAuthSuccess = false;
            if (OperatorConfig.TokenExpiredAt <= System.DateTime.Now.AddMinutes(-1))
            {
                var tokenCreatedAt = System.DateTime.Now;
                var tokenExpiredAt = System.DateTime.Now.AddMinutes(59);
                var authResponse = await _vodafoneApi.Auth(CreateAuthRequest());
                if (authResponse.ResultCode == "0")
                {
                    isAuthSuccess = true;
                    OperatorConfig.AuthToken = authResponse.AuthToken;
                    OperatorConfig.TokenCreatedAt = tokenCreatedAt;
                    OperatorConfig.TokenExpiredAt = tokenExpiredAt;
                    _authToken = OperatorConfig.AuthToken;
                    SaveOperator();
                }
            }
            else
            {
                isAuthSuccess = true;
                _authToken = OperatorConfig.AuthToken;
            }

            return isAuthSuccess;
        }

        private async Task<bool> RefreshToken()
        {
            var tokenCreatedAt = System.DateTime.Now;
            var tokenExpiredAt = System.DateTime.Now.AddMinutes(59);
            var authResponse = await _vodafoneApi.Auth(CreateAuthRequest());
            if (authResponse.ResultCode == "0")
            {
                OperatorConfig.AuthToken = authResponse.AuthToken;
                OperatorConfig.TokenCreatedAt = tokenCreatedAt;
                OperatorConfig.TokenExpiredAt = tokenExpiredAt;
                _authToken = OperatorConfig.AuthToken;
                SaveOperator();
            }
            return authResponse.ResultCode == "0";
        }

        private void ExtendToken()
        {
            if (DateTime.Now < OperatorConfig.TokenCreatedAt.AddHours(24))
            {
                OperatorConfig.TokenExpiredAt = DateTime.Now.AddMinutes(60);
                SaveOperator();
            }
        }

        public async Task<bool> SendOtp(Phone phone, string content, ConcurrentBag<OtpResponseLog> responses, Header header, bool useControlDays)
        {
            var isAuthSuccess = await Auth();
            if (isAuthSuccess)
            {
                var vodafoneResponse = await _vodafoneApi.SendSms(CreateSmsRequest(phone, content, header, useControlDays));
                if (vodafoneResponse.ResultCode.Trim().Equals("1008") ||
                    vodafoneResponse.ResultCode.Trim().Equals("1011") ||
                    vodafoneResponse.ResultCode.Trim().Equals("1016"))
                {
                    if (await RefreshToken())
                        vodafoneResponse = await _vodafoneApi.SendSms(CreateSmsRequest(phone, content, header, false));
                }
                System.Diagnostics.Debug.WriteLine("Vodafone otp is send");

                var response = vodafoneResponse.BuildOperatorApiResponse();

                responses.Add(response);

                if (response.ResponseCode == SendSmsResponseStatus.Success)
                    Task.Run(() => TrackMessageStatus(response));
                ExtendToken();
                
            }
            else
            {
                var response = new OtpResponseLog
                {
                    Operator = OperatorType.Vodafone,
                    Topic = "Vodafone otp sending",
                    TrackingStatus = SmsTrackingStatus.SystemError
                };
                response.ResponseCode = SendSmsResponseStatus.ClientError;
                response.ResponseMessage = "Vodafone Auth Failed";
            }
            return true;
        }

        public async Task<OtpResponseLog> SendOtp(Phone phone, string content, Header header, bool useControlDays)
        {
            var isAuthSuccess = await Auth();
            if (isAuthSuccess)
            {
                var vodafoneResponse = await _vodafoneApi.SendSms(CreateSmsRequest(phone, content, header, useControlDays));
                if (vodafoneResponse.ResultCode.Trim().Equals("1008") ||
                    vodafoneResponse.ResultCode.Trim().Equals("1011") ||
                    vodafoneResponse.ResultCode.Trim().Equals("1016"))
                {
                    if (await RefreshToken())
                        vodafoneResponse = await _vodafoneApi.SendSms(CreateSmsRequest(phone, content, header, useControlDays));
                }
                System.Diagnostics.Debug.WriteLine("Vodafone otp is send");

                var response = vodafoneResponse.BuildOperatorApiResponse();

                if (response.ResponseCode == SendSmsResponseStatus.Success)
                    Task.Run(() => TrackMessageStatus(response));
                ExtendToken();

                return response;
            }
            else
            {
                var response = new OtpResponseLog
                {
                    Operator = OperatorType.Vodafone,
                    Topic = "Vodafone otp sending",
                    TrackingStatus = SmsTrackingStatus.SystemError
                };
                response.ResponseCode = SendSmsResponseStatus.ClientError;
                response.ResponseMessage = "Vodafone Auth Failed";

                return response;
            }
        }

        public override async Task<OtpTrackingLog> CheckMessageStatus(OtpResponseLog response)
        {
            var isAuthSuccess = await Auth();
            var vodafoneResponse = await _vodafoneApi.CheckSmsStatus(CreateSmsStatusRequest(response.StatusQueryId));
            return vodafoneResponse.BuildOperatorApiTrackingResponse(response);
        }

        private VodafoneSmsRequest CreateSmsRequest(Phone phone, string content, Header header, bool useControlDays)
        {
            double controlHour = (OperatorConfig.ControlDaysForOtp * 24);
            if (useControlDays)
            {
                var phoneConfiguration = GetPhoneConfiguration(phone);
                if (phoneConfiguration.BlacklistEntries != null &&
                    phoneConfiguration.BlacklistEntries.Count > 0)
                {
                    var blackListEntry = phoneConfiguration.BlacklistEntries
                    .Where(b => b.Status == BlacklistStatus.Resolved).OrderByDescending(b => b.CreatedAt)
                    .FirstOrDefault();

                    if (blackListEntry != null)
                    {
                        if (blackListEntry.ResolvedAt != null)
                        {
                            double resolvedDateTotalHour = (DateTime.Now - blackListEntry.ResolvedAt.Value).TotalHours;
                            controlHour = resolvedDateTotalHour > controlHour ? controlHour : resolvedDateTotalHour;
                        }
                    }
                }

            }

            return new VodafoneSmsRequest()
            {
                AuthToken = _authToken,
                User = OperatorConfig.User,
                ExpiryPeriod = "60",
                Header = "BURGAN BANK",
                Message = content,
                PhoneNo = phone.CountryCode.ToString() + phone.Prefix.ToString() + phone.Number.ToString(),
                ControlHour = controlHour.ToString()
            };
        }

        private VodafoneSmsStatusRequest CreateSmsStatusRequest(string messageId)
        {
            return new VodafoneSmsStatusRequest()
            {
                AuthToken = _authToken,
                User = OperatorConfig.User,
                MessageId = messageId
            };
        }

        private VodafoneAuthRequest CreateAuthRequest()
        {
            return new VodafoneAuthRequest() { 
                User = OperatorConfig.User,
                Password = OperatorConfig.Password
            };
        }
    }
}
