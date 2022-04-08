using bbt.gateway.messaging.Api;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using bbt.gateway.messaging.Api.dEngage;
using bbt.gateway.messaging.Api.dEngage.Model.Login;
using bbt.gateway.messaging.Api.dEngage.Model.Settings;
using bbt.gateway.messaging.Api.dEngage.Model.Transactional;
using bbt.gateway.common.Models;
using System.Collections.Generic;
using Refit;
using Newtonsoft.Json;

namespace bbt.gateway.messaging.Workers.OperatorGateway
{
    public class OperatordEngage : OperatorGatewayBase
    {
        private GetSmsFromsResponse _smsIds;
        private GetMailFromsResponse _mailIds;

        private string _authToken;
        private readonly IdEngageClient _dEngageClient;
        public OperatordEngage(IdEngageClient dEngageClient, IConfiguration configuration,
            ITransactionManager transactionManager) : base(configuration,transactionManager)
        {
            _dEngageClient = dEngageClient;
            if (transactionManager.BusinessLine == "X")
                Type = OperatorType.dEngageOn;
            else
                Type = OperatorType.dEngageBurgan;
        }

        private async Task<OperatorApiAuthResponse> Auth()
        {
            OperatorApiAuthResponse authResponse = new();
            if (string.IsNullOrEmpty(OperatorConfig.AuthToken) || OperatorConfig.TokenExpiredAt <= System.DateTime.Now.AddMinutes(-1))
            {
                var tokenCreatedAt = DateTime.Now;
                var tokenExpiredAt = DateTime.Now.AddMinutes(59);
                var loginResponse = await _dEngageClient.Login(CreateAuthRequest());
                if (!string.IsNullOrEmpty(loginResponse.access_token))
                {
                    authResponse.ResponseCode = "0";
                    OperatorConfig.AuthToken = loginResponse.access_token;
                    OperatorConfig.TokenCreatedAt = tokenCreatedAt;
                    OperatorConfig.TokenExpiredAt = tokenExpiredAt;
                    _authToken = OperatorConfig.AuthToken;
                    SaveOperator();
                }
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
            var tokenCreatedAt = DateTime.Now;
            var tokenExpiredAt = DateTime.Now.AddMinutes(59);
            var loginResponse = await _dEngageClient.Login(CreateAuthRequest());
            if (!string.IsNullOrEmpty(loginResponse.access_token))
            {
                OperatorConfig.AuthToken = loginResponse.access_token;
                OperatorConfig.TokenCreatedAt = tokenCreatedAt;
                OperatorConfig.TokenExpiredAt = tokenExpiredAt;
                _authToken = OperatorConfig.AuthToken;
                SaveOperator();
                return true;
            }
            
            return false;
        }

        public async Task<SmsLog> SendMail(string to, string? subject, string? html, string? templateId, string? templateParams)
        {
            var authResponse = await Auth();
            if (authResponse.ResponseCode == "0")
            {
                _mailIds = await _dEngageClient.GetMailFroms(_authToken);
                try
                {
                    var req = CreateMailRequest(to, subject, html, templateId, templateParams);
                    var sendMailResponse = await _dEngageClient.SendMail(req, _authToken);

                    if (sendMailResponse.code == 1251)
                    {
                        if (await RefreshToken())
                            sendMailResponse = await _dEngageClient.SendMail(req, _authToken);
                    }
                    var response = new SmsLog()
                    {
                        Content = html,
                        OperatorResponseCode = sendMailResponse.code,
                        OperatorResponseMessage = sendMailResponse.message,
                        Operator = Type,
                        CreatedAt = DateTime.Now
                    };


                    return response;
                }
                catch (ApiException ex)
                {
                    //logging
                    return null;
                }

            }
            else
            {
                var response = new SmsLog()
                {
                    Content = html,
                    OperatorResponseCode = -99999,
                    OperatorResponseMessage = "dEngage Authentication Failed",
                    Operator = Type,
                    CreatedAt = DateTime.Now
                };

                return response;
            }

        }
        public async Task<SmsLog> SendSms(Phone phone,SmsTypes smsType, string? content,string? templateId,string? templateParams)
        {
            var authResponse = await Auth();
            if (authResponse.ResponseCode == "0")
            {
                _smsIds = await _dEngageClient.GetSmsFroms(_authToken);
                try
                {
                    var req = CreateSmsRequest(phone, smsType, content, templateId, templateParams);
                    var sendSmsResponse = await _dEngageClient.SendSms(req, _authToken);
                   
                    if (sendSmsResponse.code == 1251)
                    {
                        if (await RefreshToken())
                            sendSmsResponse = await _dEngageClient.SendSms(req, _authToken);
                    }
                    var response = new SmsLog()
                    {
                        Content = content,
                        OperatorResponseCode = sendSmsResponse.code,
                        OperatorResponseMessage = sendSmsResponse.message,
                        Operator = Type,
                        CreatedAt = DateTime.Now
                    };


                    return response;
                }
                catch (ApiException ex)
                {
                    //logging
                    return null;
                }                 
                
            }
            else
            {
                var response = new SmsLog()
                {
                    Content = content,
                    OperatorResponseCode = -99999,
                    OperatorResponseMessage = "dEngage Authentication Failed",
                    Operator = Type,
                    CreatedAt = DateTime.Now
                };

                return response;
            }
        }

        private SendMailRequest CreateMailRequest(string to,string subject = null, string html = null, string templateId = null, string templateParams = null)
        {
            SendMailRequest sendMailRequest = new();
            sendMailRequest.send.to = to;
            if (!string.IsNullOrEmpty(templateId))
            {
                sendMailRequest.content.templateId = templateId;
                if (!string.IsNullOrEmpty(templateParams))
                {
                    sendMailRequest.current = templateParams;
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(html))
                {
                    sendMailRequest.content.fromNameId = _mailIds.data.emailFroms.FirstOrDefault().id;
                    sendMailRequest.content.html = html;
                    sendMailRequest.content.subject = subject;
                }
                else
                {
                    //Critical Error
                }

            }
            return sendMailRequest;
        }

        private Api.dEngage.Model.Transactional.SendSmsRequest CreateSmsRequest(Phone phone, SmsTypes smsType, string content = null,string templateId = null,string templateParams = null)
        {
            Api.dEngage.Model.Transactional.SendSmsRequest sendSmsRequest = new();
            sendSmsRequest.send.to = phone.Concatenate();
            if (!string.IsNullOrEmpty(templateId))
            {
                sendSmsRequest.content.templateId = templateId;
                if (!string.IsNullOrEmpty(templateParams))
                {
                    sendSmsRequest.current = templateParams;
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(content))
                {
                    sendSmsRequest.content.smsFromId = _smsIds.data.smsFroms.Where(i => i.partnerName == Constant.smsTypes[smsType]).FirstOrDefault().id;
                    sendSmsRequest.content.message = content;
                }
                else
                { 
                    //Critical Error
                }

            }
            return sendSmsRequest;
        }

        private LoginRequest CreateAuthRequest()
        {
            return new LoginRequest() { 
                userkey = OperatorConfig.User,
                password = OperatorConfig.Password
            };
        }
    }
}
