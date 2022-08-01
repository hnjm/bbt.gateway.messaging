﻿using bbt.gateway.messaging.Api.Vodafone.Model;
using bbt.gateway.common.Models;
using bbt.gateway.messaging.Api.Vodafone;
using bbt.gateway.messaging.Api;
using bbt.gateway.common;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

namespace bbt.gateway.messaging.Workers.OperatorGateway
{
    public class OperatorVodafone : OperatorGatewayBase, IOperatorGateway
    {
        private string _authToken;
        private readonly IVodafoneApi _vodafoneApi;
        public OperatorVodafone(VodafoneApiFactory vodafoneApiFactory, IConfiguration configuration,
            ITransactionManager transactionManager) : base(configuration,transactionManager)
        {
            _vodafoneApi = vodafoneApiFactory(TransactionManager.UseFakeSmtp);
            Type = OperatorType.Vodafone;
            _vodafoneApi.SetOperatorType(OperatorConfig);
        }

        private async Task<OperatorApiAuthResponse> Auth()
        {
            OperatorApiAuthResponse authResponse = new();
            if (string.IsNullOrEmpty(OperatorConfig.AuthToken) || OperatorConfig.TokenExpiredAt <= System.DateTime.Now.AddMinutes(-1))
            {

                var tokenCreatedAt = System.DateTime.Now;
                var tokenExpiredAt = System.DateTime.Now.AddMinutes(59);
                authResponse = await _vodafoneApi.Auth(CreateAuthRequest());
                if (authResponse.ResponseCode == "0")
                {
                    OperatorConfig.AuthToken = authResponse.AuthToken;
                    OperatorConfig.TokenCreatedAt = tokenCreatedAt;
                    OperatorConfig.TokenExpiredAt = tokenExpiredAt;
                    _authToken = OperatorConfig.AuthToken;
                    await SaveOperator();
                }
                //Logging
            }
            else
            {
                authResponse.ResponseCode = "0";
                _authToken = OperatorConfig.AuthToken;
            }

            return authResponse;
        }

        private async Task<bool> RefreshToken()
        {
            var tokenCreatedAt = System.DateTime.Now;
            var tokenExpiredAt = System.DateTime.Now.AddMinutes(59);
            var authResponse = await _vodafoneApi.Auth(CreateAuthRequest());
            if (authResponse.ResponseCode == "0")
            {
                OperatorConfig.AuthToken = authResponse.AuthToken;
                OperatorConfig.TokenCreatedAt = tokenCreatedAt;
                OperatorConfig.TokenExpiredAt = tokenExpiredAt;
                _authToken = OperatorConfig.AuthToken;
                await SaveOperator();
            }
            //Login
            return authResponse.ResponseCode == "0";
        }

        private async Task ExtendToken()
        {
            if (DateTime.Now < OperatorConfig.TokenCreatedAt.AddHours(24))
            {
                OperatorConfig.TokenExpiredAt = DateTime.Now.AddMinutes(60);
                await SaveOperator();
            }
        }

        public async Task<bool> SendOtp(Phone phone, string content, ConcurrentBag<OtpResponseLog> responses, Header header, bool useControlDays)
        {
            var authResponse = await Auth();
            if (authResponse.ResponseCode == "0")
            {
                var vodafoneResponse = await _vodafoneApi.SendSms(await CreateSmsRequest(phone, content, header, useControlDays));
                if (vodafoneResponse.ResponseCode.Trim().Equals("1008") ||
                    vodafoneResponse.ResponseCode.Trim().Equals("1011") ||
                    vodafoneResponse.ResponseCode.Trim().Equals("1016"))
                {
                    if (await RefreshToken())
                        vodafoneResponse = await _vodafoneApi.SendSms(await CreateSmsRequest(phone, content, header, false));
                }

                var response = vodafoneResponse.BuildOperatorApiResponse();
                responses.Add(response);
                await ExtendToken();
                
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
                response.ResponseMessage = authResponse.ResponseMessage;
                responses.Add(response);
            }
            
            return true;
        }

        public async Task<OtpResponseLog> SendOtp(Phone phone, string content, Header header, bool useControlDays)
        {
            var authResponse = await Auth();
            if (authResponse.ResponseCode == "0")
            {
                var vodafoneResponse = await _vodafoneApi.SendSms(await CreateSmsRequest(phone, content, header, useControlDays));
                if (vodafoneResponse.ResponseCode.Trim().Equals("1008") ||
                    vodafoneResponse.ResponseCode.Trim().Equals("1011") ||
                    vodafoneResponse.ResponseCode.Trim().Equals("1016"))
                {
                    if (await RefreshToken())
                        vodafoneResponse = await _vodafoneApi.SendSms(await CreateSmsRequest(phone, content, header, useControlDays));
                }

                var response = vodafoneResponse.BuildOperatorApiResponse();

                await ExtendToken();

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
                response.ResponseMessage = authResponse.ResponseMessage;

                return response;
            }
        }

        public async Task<OtpTrackingLog> CheckMessageStatus(CheckSmsRequest checkSmsRequest)
        {
            var authResponse = await Auth();
            if (authResponse.ResponseCode == "0")
            {
                var vodafoneResponse = await _vodafoneApi.CheckSmsStatus(CreateSmsStatusRequest(checkSmsRequest.StatusQueryId));
                return vodafoneResponse.BuildOperatorApiTrackingResponse(checkSmsRequest);
            }
            else
            {
                return null;
            }
        }

        private async Task<VodafoneSmsRequest> CreateSmsRequest(Phone phone, string content, Header header, bool useControlDays)
        {
            double controlHour = (OperatorConfig.ControlDaysForOtp * 24);
            if (useControlDays)
            {
                var phoneConfiguration = await GetPhoneConfiguration(phone);
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
                    else
                    {
                        double oldResolvedDateTotalHour = (DateTime.Now - TransactionManager.OldBlacklistVerifiedAt).TotalHours;
                        controlHour = oldResolvedDateTotalHour > controlHour ? controlHour : oldResolvedDateTotalHour;
                    }
                }

            }

            return new VodafoneSmsRequest()
            {
                AuthToken = _authToken,
                User = OperatorConfig.User,
                ExpiryPeriod = "60",
                Header = Constant.OperatorSenders[header.SmsSender][OperatorType.Vodafone],
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
