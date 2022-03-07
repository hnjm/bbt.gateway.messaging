using bbt.gateway.messaging.Api.Turkcell.Model;
using bbt.gateway.common.Models;
using bbt.gateway.messaging.Api.Turkcell;
using bbt.gateway.common;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

namespace bbt.gateway.messaging.Workers.OperatorGateway
{
    public class OperatorTurkcell : OperatorGatewayBase, IOperatorGateway
    {
        private readonly TurkcellApi _turkcellApi;
        private string _authToken;
        public OperatorTurkcell(TurkcellApi turkcellApi,IConfiguration configuration) : base(configuration)
        {
            _turkcellApi = turkcellApi;
            Type = OperatorType.Turkcell;
            _turkcellApi.SetOperatorType(OperatorConfig);
        }

        private async Task<bool> Auth()
        {
            bool isAuthSuccess = false;
            if (OperatorConfig.TokenExpiredAt <= System.DateTime.Now.AddMinutes(-1))
            {
                var tokenCreatedAt = System.DateTime.Now.SetKindUtc();
                var tokenExpiredAt = System.DateTime.Now.AddMinutes(20).SetKindUtc();
                var authResponse = await _turkcellApi.Auth(CreateAuthRequest());
                if(authResponse.ResponseCode == "0")
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
            var tokenCreatedAt = System.DateTime.Now.SetKindUtc();
            var tokenExpiredAt = System.DateTime.Now.AddMinutes(20).SetKindUtc();
            var authResponse = await _turkcellApi.Auth(CreateAuthRequest());
            if (authResponse.ResponseCode == "0")
            {
                OperatorConfig.AuthToken = authResponse.AuthToken;
                OperatorConfig.TokenCreatedAt = tokenCreatedAt;
                OperatorConfig.TokenExpiredAt = tokenExpiredAt;
                _authToken = OperatorConfig.AuthToken;
                SaveOperator();
            }
            return authResponse.ResponseCode == "0";
        }

        private void ExtendToken()
        {
            OperatorConfig.TokenExpiredAt = DateTime.Now.AddMinutes(20).SetKindUtc();
            SaveOperator();
        }

        public async Task<bool> SendOtp(Phone phone, string content, ConcurrentBag<OtpResponseLog> responses, Header header, bool useControlDays)
        {
            var isAuthSuccess = await Auth();

            if (isAuthSuccess)
            {
                var turkcellResponse = await _turkcellApi.SendSms(CreateSmsRequest(phone, content, header, false));
                if (turkcellResponse.ResponseCode.Trim().Equals("-2"))
                {
                    if(await RefreshToken())
                        turkcellResponse = await _turkcellApi.SendSms(CreateSmsRequest(phone, content, header, false));
                }
                System.Diagnostics.Debug.WriteLine("Turkcell otp is send");

                var response = turkcellResponse.BuildOperatorApiResponse();
                responses.Add(response);

                ExtendToken();
            }
            else
            {
                var response = new OtpResponseLog
                {
                    Operator = OperatorType.Turkcell,
                    Topic = "Turkcell otp sending",
                    TrackingStatus = SmsTrackingStatus.SystemError
                };
                response.ResponseCode = SendSmsResponseStatus.ClientError;
                response.ResponseMessage = "Turkcell Auth Failed";
                responses.Add(response);
            }

            return true;
        }

        public async Task<OtpResponseLog> SendOtp(Phone phone, string content, Header header, bool useControlDays)
        {
            var isAuthSuccess = await Auth();

            if (isAuthSuccess)
            {
                var turkcellResponse = await _turkcellApi.SendSms(CreateSmsRequest(phone, content, header, useControlDays));
                if (turkcellResponse.ResponseCode.Trim().Equals("-2"))
                {
                    if (await RefreshToken())
                        turkcellResponse = await _turkcellApi.SendSms(CreateSmsRequest(phone, content, header, useControlDays));
                }
                System.Diagnostics.Debug.WriteLine("Turkcell otp is send");

                var response = turkcellResponse.BuildOperatorApiResponse();

                ExtendToken();

                return response;
            }
            else 
            {
                var response = new OtpResponseLog
                {
                    Operator = OperatorType.Turkcell,
                    Topic = "Turkcell otp sending",
                    TrackingStatus = SmsTrackingStatus.SystemError
                };
                response.ResponseCode = SendSmsResponseStatus.ClientError;
                response.ResponseMessage = "Turkcell Auth Failed";

                return response;
            }
                        
        }

        public async Task<OtpTrackingLog> CheckMessageStatus(CheckSmsRequest checkSmsRequest)
        {
            var isAuthSuccess = await Auth();
            var turkcellResponse = await _turkcellApi.CheckSmsStatus(CreateSmsStatusRequest(checkSmsRequest.StatusQueryId));
            return turkcellResponse.BuildOperatorApiTrackingResponse(checkSmsRequest);
        }

        private TurkcellSmsRequest CreateSmsRequest(Phone phone, string content, Header header, bool useControlDays)
        {
            DateTime trustedDate = DateTime.Now.AddDays(-1 * OperatorConfig.ControlDaysForOtp);
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
                            DateTime resolvedDate = blackListEntry.ResolvedAt.Value;
                            trustedDate = trustedDate > resolvedDate ? trustedDate : resolvedDate;
                        }
                    }
                }
            }

            var request = new TurkcellSmsRequest();
            request.Header = Constant.OperatorSenders[header.SmsSender][OperatorType.Turkcell];
            request.PhoneNo = "00" + phone.CountryCode + phone.Prefix + phone.Number;
            request.SessionId = _authToken;
            request.Content = content;
            request.TrustedDate = trustedDate.ToString("ddMMyyHHmmss");
            return request;
        }

        private TurkcellSmsStatusRequest CreateSmsStatusRequest(string msgId)
        {
            var request = new TurkcellSmsStatusRequest();
            request.AuthToken = _authToken;
            request.MsgId = msgId;
            return request;
        }


        private TurkcellAuthRequest CreateAuthRequest()
        {
            var request = new TurkcellAuthRequest();
            request.User = OperatorConfig.User;
            request.Password= OperatorConfig.Password;
            return request;
        }
    }
}
