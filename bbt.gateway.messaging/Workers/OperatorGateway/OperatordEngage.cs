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
using Refit;
using SendSmsResponse = bbt.gateway.messaging.Api.dEngage.Model.Transactional.SendSmsResponse;

namespace bbt.gateway.messaging.Workers.OperatorGateway
{
    public class OperatordEngage : OperatorGatewayBase
    {
        private GetSmsFromsResponse _smsIds;
        private GetMailFromsResponse _mailIds;

        private int _authTryCount;
        private string _authToken;
        private readonly IdEngageClient _dEngageClient;
        public OperatordEngage(IdEngageClient dEngageClient, IConfiguration configuration,
            ITransactionManager transactionManager) : base(configuration,transactionManager)
        {
            _authTryCount = 0;
            _dEngageClient = dEngageClient;
            
        }

        private async Task<OperatorApiAuthResponse> Auth()
        {
            TransactionManager.SmsRequestInfo.Operator = Type;

            OperatorApiAuthResponse authResponse = new();
            if (string.IsNullOrEmpty(OperatorConfig.AuthToken) || OperatorConfig.TokenExpiredAt <= DateTime.Now.AddSeconds(-30))
            {
                var tokenCreatedAt = DateTime.Now;

                try
                {
                    var loginResponse = await _dEngageClient.Login(CreateAuthRequest());

                    authResponse.ResponseCode = "0";
                    OperatorConfig.AuthToken = loginResponse.access_token;
                    OperatorConfig.TokenCreatedAt = tokenCreatedAt;
                    OperatorConfig.TokenExpiredAt = DateTime.Now.AddSeconds(loginResponse.expires_in);
                    _authToken = OperatorConfig.AuthToken;
                    SaveOperator();
                }
                catch (ApiException ex)
                {
                    authResponse.ResponseCode = "99999";
                    authResponse.ResponseMessage = $"dEngage | Http Status Code : {ex.StatusCode} | Auth Failed";
                }                   
                
            }
            else
            {
                authResponse.ResponseCode = "0";
                _authToken = OperatorConfig.AuthToken;
            }

            return authResponse;
        }

        private async Task<OperatorApiAuthResponse> RefreshToken()
        {
            OperatorApiAuthResponse authResponse = new();

            var tokenCreatedAt = DateTime.Now;
            try
            {
                var loginResponse = await _dEngageClient.Login(CreateAuthRequest());

                authResponse.ResponseCode = "0";
                OperatorConfig.AuthToken = loginResponse.access_token;
                OperatorConfig.TokenCreatedAt = tokenCreatedAt;
                OperatorConfig.TokenExpiredAt = DateTime.Now.AddSeconds(loginResponse.expires_in);
                _authToken = OperatorConfig.AuthToken;
                SaveOperator();
            }
            catch (ApiException ex)
            {
                authResponse.ResponseCode = "99999";
                authResponse.ResponseMessage = $"dEngage | Http Status Code : {ex.StatusCode} | Auth Failed";
            }

            return authResponse;
        }

        public async Task<SmsStatusResponse> CheckSms(string queryId)
        {
            var authResponse = await Auth();
            if (authResponse.ResponseCode == "0")
            {
                try
                {
                    SmsStatusRequest smsStatusRequest = new()
                    {
                        trackingId = queryId,
                    };
                    var response = await _dEngageClient.GetSmsStatus(_authToken, smsStatusRequest);
                    return response;
                }
                catch (ApiException ex)
                {

                }
            }
            else
            { 
            
            }
            return null;
        }

        public async Task<MailResponseLog> SendMail(string to, string? from, string? subject, string? html, string? templateId, string? templateParams)
        {
            var mailResponseLog = new MailResponseLog() { 
                Topic = "dEngage Mail Sending",
            };
           
            var authResponse = await Auth();
            if (authResponse.ResponseCode == "0")
            {
                if (html != null)
                {
                    try
                    {
                        var res = await _dEngageClient.GetMailFroms(_authToken);
                        _mailIds = res;
                    }
                    catch (ApiException ex)
                    {
                        mailResponseLog.ResponseCode = "99999";
                        mailResponseLog.ResponseMessage = $"dEngage | Http Status Code : {(int)ex.StatusCode} | Cannot Retrieve Sms Froms";
                        return mailResponseLog;
                    }
                    if (_mailIds.data.emailFroms.Where(m => m.fromAddress == from).FirstOrDefault() == null) 
                    {
                        mailResponseLog.ResponseCode = "99999";
                        mailResponseLog.ResponseMessage = "Mail From is Not Found";
                    }
                }
               
                try
                {
                    var req = CreateMailRequest(to, from, subject, html, templateId, templateParams);
                    try
                    {
                        var sendMailResponse = await _dEngageClient.SendMail(req, _authToken);
                        mailResponseLog.ResponseCode = sendMailResponse.code.ToString();
                        mailResponseLog.ResponseMessage = sendMailResponse.message;
                        mailResponseLog.StatusQueryId = sendMailResponse.data.to.trackingId;

                    }
                    catch (ApiException ex)
                    {
                        if (ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                        {
                            authResponse = await RefreshToken();
                            if (authResponse.ResponseCode == "0")
                            {
                                _authTryCount++;
                                if (_authTryCount < 3)
                                {
                                    return await SendMail(to,from,subject,html,templateId,templateParams);
                                }
                                else
                                {
                                    mailResponseLog.ResponseCode = "99999";
                                    mailResponseLog.ResponseMessage = "dEngage Auth Failed For 3 Times";
                                    return mailResponseLog;
                                }
                            }
                            else
                            {
                                mailResponseLog.ResponseCode = authResponse.ResponseCode;
                                mailResponseLog.ResponseMessage = authResponse.ResponseMessage;
                                return mailResponseLog;
                            }
                        }
                        var error = await ex.GetContentAsAsync<SendMailResponse>();
                        mailResponseLog.ResponseCode = error.code.ToString();
                        mailResponseLog.ResponseMessage = error.message;
                    }                    


                    return mailResponseLog;
                }
                catch (Exception ex)
                {
                    mailResponseLog.ResponseCode = "-99999";
                    mailResponseLog.ResponseMessage = ex.ToString();

                    //logging
                    return mailResponseLog;
                }

            }
            else
            {
                mailResponseLog.ResponseCode = authResponse.ResponseCode;
                mailResponseLog.ResponseMessage = authResponse.ResponseMessage;

                return mailResponseLog;
            }

        }
        public async Task<SmsResponseLog> SendSms(Phone phone,SmsTypes smsType, string? content,string? templateId,string? templateParams)
        {
            var smsLog = new SmsResponseLog()
            {
                Operator = Type,
                Content = String.IsNullOrEmpty(content) ? "" : content.ClearMaskingFields(),
                CreatedAt = DateTime.Now,
            };

            var authResponse = await Auth();
            if (authResponse.ResponseCode == "0")
            {
                if (content != null)
                {
                    try
                    {
                        var res = await _dEngageClient.GetSmsFroms(_authToken);
                        _smsIds = res;
                    }
                    catch (ApiException ex)
                    {
                        smsLog.OperatorResponseCode = 99999;
                        smsLog.OperatorResponseMessage = $"dEngage | Http Status Code : {(int)ex.StatusCode} | Cannot Retrieve Sms Froms";
                        return smsLog;
                    }
                }
                try
                {
                    var req = CreateSmsRequest(phone, smsType, content, templateId, templateParams);
                    try
                    {
                        var sendSmsResponse = await _dEngageClient.SendSms(req, _authToken);
                        smsLog.OperatorResponseCode = sendSmsResponse.code;
                        smsLog.OperatorResponseMessage = sendSmsResponse.message;
                        smsLog.StatusQueryId = sendSmsResponse.data.to.trackingId;

                    }
                    catch (ApiException ex)
                    {
                        if (ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                        {
                            authResponse = await RefreshToken();
                            if (authResponse.ResponseCode == "0")
                            {
                                _authTryCount++;
                                if (_authTryCount < 3)
                                {
                                    return await SendSms(phone, smsType, content, templateId, templateParams);
                                }
                                else
                                {
                                    smsLog.OperatorResponseCode = 99999;
                                    smsLog.OperatorResponseMessage = "dEngage Auth Failed For 3 Times";
                                    return smsLog;
                                }
                            }
                            else
                            {
                                smsLog.OperatorResponseCode = Convert.ToInt32(authResponse.ResponseCode);
                                smsLog.OperatorResponseMessage = authResponse.ResponseMessage;
                                return smsLog;
                            }
                        }
                        var error = await ex.GetContentAsAsync<bbt.gateway.messaging.Api.dEngage.Model.Transactional.SendSmsResponse>();
                        smsLog.OperatorResponseCode = error.code;
                        smsLog.OperatorResponseMessage = error.message;
                    }
                    
                    return smsLog;
                }
                catch (Exception ex)
                {
                    //logging
                    smsLog.OperatorResponseCode = -99999;
                    smsLog.OperatorResponseMessage = ex.ToString();
                    return smsLog;
                }                 
                
            }
            else
            {
                smsLog.OperatorResponseCode = Convert.ToInt32(authResponse.ResponseCode);
                smsLog.OperatorResponseMessage = authResponse.ResponseMessage;              

                return smsLog;
            }
        }

        private SendMailRequest CreateMailRequest(string to,string from = null,string subject = null, string html = null, string templateId = null, string templateParams = null)
        {
            SendMailRequest sendMailRequest = new();
            sendMailRequest.send.to = to;
            if (!string.IsNullOrEmpty(templateId))
            {
                sendMailRequest.content.templateId = templateId;
                if (!string.IsNullOrEmpty(templateParams))
                {
                    sendMailRequest.current = templateParams.ClearMaskingFields();
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(html))
                {
                    sendMailRequest.content.fromNameId = _mailIds.data.emailFroms.Where(m => m.fromAddress == from).FirstOrDefault().id;
                    sendMailRequest.content.html = html.ClearMaskingFields();
                    sendMailRequest.content.subject = subject.ClearMaskingFields();
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
                    sendSmsRequest.current = templateParams.ClearMaskingFields();
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(content))
                {
                    sendSmsRequest.content.smsFromId = _smsIds.data.smsFroms.Where(i => i.partnerName == Constant.smsTypes[smsType]).FirstOrDefault().id;
                    sendSmsRequest.content.message = content.ClearMaskingFields();
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
