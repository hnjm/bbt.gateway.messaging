﻿using bbt.gateway.messaging.Api.Turkcell.Model;
using bbt.gateway.common.Models;
using bbt.gateway.messaging.Api.Turkcell;
using bbt.gateway.common;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using bbt.gateway.messaging.Api;

namespace bbt.gateway.messaging.Workers.OperatorGateway
{
    public class OperatorTurkcell : OperatorGatewayBase, IOperatorGateway
    {
        private readonly ITurkcellApi _turkcellApi;
        private string _authToken;
        public OperatorTurkcell(TurkcellApiFactory turkcellApiFactory,IConfiguration configuration,
            ITransactionManager transactionManager) : base(configuration,transactionManager)
        {
            _turkcellApi = turkcellApiFactory(TransactionManager.UseFakeSmtp);
            Type = OperatorType.Turkcell;
            _turkcellApi.SetOperatorType(OperatorConfig);
        }

        private async Task<OperatorApiAuthResponse> Auth()
        {
            OperatorApiAuthResponse response = new();
            if (string.IsNullOrEmpty(OperatorConfig.AuthToken) || OperatorConfig.TokenExpiredAt <= System.DateTime.Now.AddMinutes(-1))
            {
                var tokenCreatedAt = DateTime.Now;
                var tokenExpiredAt = DateTime.Now.AddMinutes(20);
                response = await _turkcellApi.Auth(CreateAuthRequest());
                if (response.ResponseCode == "0")
                {
                    OperatorConfig.AuthToken = response.AuthToken;
                    OperatorConfig.TokenCreatedAt = tokenCreatedAt;
                    OperatorConfig.TokenExpiredAt = tokenExpiredAt;
                    _authToken = OperatorConfig.AuthToken;

                    await SaveOperator();
                }
                else
                {
                    TransactionManager.LogCritical($"Turkcell Auth Failed | Response Code : {response.ResponseCode} : Response Message : {response.ResponseMessage}");
                }
            }
            else
            {
                response.ResponseCode = "0";
                _authToken = OperatorConfig.AuthToken;
            }

            return response;
        }

        private async Task<bool> RefreshToken()
        {
            var tokenCreatedAt = System.DateTime.Now;
            var tokenExpiredAt = System.DateTime.Now.AddMinutes(20);
            var authResponse = await _turkcellApi.Auth(CreateAuthRequest());
            if (authResponse.ResponseCode == "0")
            {
                OperatorConfig.AuthToken = authResponse.AuthToken;
                OperatorConfig.TokenCreatedAt = tokenCreatedAt;
                OperatorConfig.TokenExpiredAt = tokenExpiredAt;
                _authToken = OperatorConfig.AuthToken;
                await SaveOperator();
            }
            else
            {
                TransactionManager.LogCritical($"Turkcell Auth Failed | Response Code : {authResponse.ResponseCode} : Response Message : {authResponse.ResponseMessage}");
            }
            return authResponse.ResponseCode == "0";
        }

        private async Task ExtendToken()
        {
            OperatorConfig.TokenExpiredAt = DateTime.Now.AddMinutes(20);
            await SaveOperator();
        }

        public async Task<bool> SendOtp(Phone phone, string content, ConcurrentBag<OtpResponseLog> responses, Header header, bool useControlDays)
        {
            var authResponse = await Auth();

            if (authResponse.ResponseCode == "0")
            {
                var turkcellResponse = await _turkcellApi.SendSms(await CreateSmsRequest(phone, content, header, false));
                if (turkcellResponse.ResponseCode.Trim().Equals("-2"))
                {
                    if(await RefreshToken())
                        turkcellResponse = await _turkcellApi.SendSms(await CreateSmsRequest(phone, content, header, false));
                }

                var response = turkcellResponse.BuildOperatorApiResponse();
                responses.Add(response);

                await ExtendToken();
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
                var turkcellResponse = await _turkcellApi.SendSms(await CreateSmsRequest(phone, content, header, useControlDays));
                if (turkcellResponse.ResponseCode.Trim().Equals("-2"))
                {
                    if (await RefreshToken())
                        turkcellResponse = await _turkcellApi.SendSms(await CreateSmsRequest(phone, content, header, useControlDays));
                }

                var response = turkcellResponse.BuildOperatorApiResponse();

                await ExtendToken();

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
                response.ResponseMessage = authResponse.ResponseMessage;

                return response;
            }
                        
        }

        public async Task<OtpTrackingLog> CheckMessageStatus(CheckSmsRequest checkSmsRequest)
        {
            var authResponse = await Auth();
            if (authResponse.ResponseCode == "0")
            {
                var turkcellResponse = await _turkcellApi.CheckSmsStatus(CreateSmsStatusRequest(checkSmsRequest.StatusQueryId));
                return turkcellResponse.BuildOperatorApiTrackingResponse(checkSmsRequest);
            }
            else
            {
                return null;
            }
        }

        private async Task<TurkcellSmsRequest> CreateSmsRequest(Phone phone, string content, Header header, bool useControlDays)
        {
            DateTime trustedDate = DateTime.Now.AddDays(-1 * OperatorConfig.ControlDaysForOtp);
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
                            DateTime resolvedDate = blackListEntry.ResolvedAt.Value;
                            trustedDate = trustedDate > resolvedDate ? trustedDate : resolvedDate;
                        }
                    }
                    else
                    {
                        trustedDate = trustedDate > TransactionManager.OldBlacklistVerifiedAt ? trustedDate : TransactionManager.OldBlacklistVerifiedAt;
                    }
                }
            }

            var request = new TurkcellSmsRequest();
            request.IsAbroad = phone.CountryCode != 90;
            request.MsgCode = this.Configuration.GetSection("Operators:Turkcell:MsgCode").Get<string>();
            request.VariantId =
                request.IsAbroad ?
                this.Configuration.GetSection("Operators:Turkcell:UVariantId").Get<string>():
                this.Configuration.GetSection("Operators:Turkcell:VariantId").Get<string>();
            request.Header =
                request.IsAbroad ?
                this.Configuration.GetSection("Operators:Turkcell:SrcMsIsdn").Get<string>() :
                Constant.OperatorSenders[header.SmsSender][OperatorType.Turkcell];
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
