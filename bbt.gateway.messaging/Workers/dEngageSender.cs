using bbt.gateway.common.Models;
using bbt.gateway.common.Repositories;
using bbt.gateway.messaging.Api.dEngage.Model.Contents;
using bbt.gateway.messaging.Exceptions;
using bbt.gateway.messaging.Workers.OperatorGateway;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bbt.gateway.messaging.Workers
{
    public class dEngageSender
    {
        private readonly HeaderManager _headerManager;
        private readonly IRepositoryManager _repositoryManager;
        private readonly ITransactionManager _transactionManager;
        private readonly IOperatordEngage _operatordEngage;
        private readonly IDistributedCache _distributedCache;


        public dEngageSender(HeaderManager headerManager,
            IRepositoryManager repositoryManager,
            ITransactionManager transactionManager,
            dEngageFactory dEngageFactory,
            IDistributedCache distributedCache)
        {
            _headerManager = headerManager;
            _repositoryManager = repositoryManager;
            _transactionManager = transactionManager;
            _operatordEngage = dEngageFactory(_transactionManager.UseFakeSmtp);
            _distributedCache = distributedCache;

            
        }

        private async Task<List<ContentInfo>> SetMailContents()
        {
            var response = await _operatordEngage.GetMailContents();
            if (response != null)
            {
                await _distributedCache.SetAsync(_operatordEngage.Type.ToString()+"_MailContents",Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(response.data.result)),
                    new DistributedCacheEntryOptions() { 
                        AbsoluteExpiration = DateTimeOffset.UtcNow.AddMinutes(30)
                    });
            }
            return response.data.result;
        }

        private async Task<List<PushContentInfo>> SetPushContents()
        {
            var response = await _operatordEngage.GetPushContents();
            if (response != null)
            {
                await _distributedCache.SetAsync(_operatordEngage.Type.ToString() + "_PushContents", Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(response.data.result)),
                    new DistributedCacheEntryOptions()
                    {
                        AbsoluteExpiration = DateTimeOffset.UtcNow.AddMinutes(30)
                    });
            }
            return response.data.result;
        }

        private async Task<List<SmsContentInfo>> SetSmsContents()
        {
            var response = await _operatordEngage.GetSmsContents();
            if (response != null)
            {
                await _distributedCache.SetAsync(_operatordEngage.Type.ToString() + "_SmsContents", Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(response.data.result)),
                    new DistributedCacheEntryOptions()
                    {
                        AbsoluteExpiration = DateTimeOffset.UtcNow.AddMinutes(30)
                    });
            }
            return response.data.result;
        }

        public async Task<CheckSmsStatusResponse> CheckSms(CheckSmsStatusRequest checkSmsStatusRequest)
        {
            CheckSmsStatusResponse checkSmsStatusResponse = new();
            var txnInfo = _repositoryManager.Transactions.GetWithId(checkSmsStatusRequest.TxnId);
            var responseLog = txnInfo?.SmsRequestLog?.ResponseLogs?.Where(r => r.OperatorResponseCode == 0).SingleOrDefault();
            if (responseLog != null)
            {
                _operatordEngage.Type = responseLog.Operator;

                var response = await _operatordEngage.CheckSms(responseLog.StatusQueryId);
                checkSmsStatusResponse.code = response.code;
                checkSmsStatusResponse.message = response.message;
                if (response.data.result.Count > 0)
                {
                    if (response.data.result[0].event_type == "DL")
                        checkSmsStatusResponse.status = SmsStatus.Delivered;
                    else
                        checkSmsStatusResponse.status = SmsStatus.NotDelivered;
                }
                else
                {
                    checkSmsStatusResponse.status = SmsStatus.NotDelivered;
                }

                return checkSmsStatusResponse;
            }
            else
            {
                checkSmsStatusResponse.code = -99999;
                checkSmsStatusResponse.message = "Transaction kaydı bulunamadı";
                return checkSmsStatusResponse;
            }

            
        }

        public async Task<SendSmsResponse> SendSms(SendMessageSmsRequest sendMessageSmsRequest)
        {
            SendSmsResponse sendSmsResponse = new SendSmsResponse()
            {
                TxnId = _transactionManager.TxnId,
            };

            var header =  _headerManager.Get(sendMessageSmsRequest.ContentType, sendMessageSmsRequest.HeaderInfo);
            if (_transactionManager.CustomerRequestInfo.BusinessLine == "X")
                _operatordEngage.Type = OperatorType.dEngageOn;
            else
                _operatordEngage.Type = OperatorType.dEngageBurgan;

            var smsRequest = new SmsRequestLog()
            {
                Operator = _operatordEngage.Type,
                Phone = sendMessageSmsRequest.Phone,
                content = header.SmsPrefix+" "+sendMessageSmsRequest.Content.MaskFields()+" "+header.SmsSuffix,
                TemplateId = "",
                TemplateParams = "",
                SmsType = sendMessageSmsRequest.SmsType,
                CreatedBy = sendMessageSmsRequest.Process
            };
            smsRequest.PhoneConfiguration = _transactionManager.SmsRequestInfo.PhoneConfiguration;

            _repositoryManager.SmsRequestLogs.Add(smsRequest);

            _transactionManager.Transaction.SmsRequestLog = smsRequest;

            var response = await _operatordEngage.SendSms(sendMessageSmsRequest.Phone, sendMessageSmsRequest.SmsType, header.BuildContentForSms(sendMessageSmsRequest.Content), null, null);
            
            smsRequest.ResponseLogs.Add(response);
            
            sendSmsResponse.Status = response.GetdEngageStatus();

            return sendSmsResponse;
        }

        public async Task<SendSmsResponse> SendTemplatedSms(SendTemplatedSmsRequest sendTemplatedSmsRequest)
        {
            SendSmsResponse sendSmsResponse = new SendSmsResponse()
            {
                TxnId = _transactionManager.TxnId,
            };

            if (sendTemplatedSmsRequest.HeaderInfo?.Sender != null)
            {
                if(sendTemplatedSmsRequest.HeaderInfo.Sender != SenderType.AutoDetect)
                _transactionManager.CustomerRequestInfo.BusinessLine = sendTemplatedSmsRequest.HeaderInfo.Sender == SenderType.On ? "X" : "B";
            }

            if (_transactionManager.CustomerRequestInfo.BusinessLine == "X")
                _operatordEngage.Type = OperatorType.dEngageOn;
            else
                _operatordEngage.Type = OperatorType.dEngageBurgan;

            var contentListByteArray = await _distributedCache.GetAsync(_operatordEngage.Type.ToString() + "_SmsContents");
            List<SmsContentInfo> contentList = null;
            if (contentListByteArray == null)
            {
                contentList = await SetSmsContents();
            }
            else
            {
                contentList = JsonConvert.DeserializeObject<List<SmsContentInfo>>(
                        Encoding.UTF8.GetString(contentListByteArray)
                    );
            }

            var templateInfo = contentList.Where(c => c.contentName.Trim() == sendTemplatedSmsRequest.Template.Trim()).FirstOrDefault();
            if (templateInfo == null)
            {
                await SetSmsContents();
                templateInfo = contentList.Where(c => c.contentName.Trim() == sendTemplatedSmsRequest.Template.Trim()).FirstOrDefault();
                if (templateInfo == null)
                {
                    throw new WorkflowException("Template Not Found", System.Net.HttpStatusCode.NotFound);
                }
            }

            var smsRequest = new SmsRequestLog()
            {
                Operator = _operatordEngage.Type,
                Phone = sendTemplatedSmsRequest.Phone,
                content = "",
                TemplateId = templateInfo.publicId,
                TemplateParams = sendTemplatedSmsRequest.TemplateParams?.MaskFields(),
                CreatedBy = sendTemplatedSmsRequest.Process
            };

            _repositoryManager.SmsRequestLogs.Add(smsRequest);
            smsRequest.PhoneConfiguration = _transactionManager.SmsRequestInfo.PhoneConfiguration;
            _transactionManager.Transaction.SmsRequestLog = smsRequest;

            var response = await _operatordEngage.SendSms(sendTemplatedSmsRequest.Phone, SmsTypes.Fast,null,templateInfo.publicId, sendTemplatedSmsRequest.TemplateParams);

            smsRequest.ResponseLogs.Add(response);
                        
            
            sendSmsResponse.Status = response.GetdEngageStatus();

            return sendSmsResponse;
        }

        public async Task<SendEmailResponse> SendMail(SendMessageEmailRequest sendMessageEmailRequest)
        {
            SendEmailResponse sendEmailResponse = new SendEmailResponse()
            {
                TxnId = _transactionManager.TxnId,
            };

            var mailRequest = new MailRequestLog()
            {
                Operator = _operatordEngage.Type,
                content = sendMessageEmailRequest.Content.MaskFields(),
                subject = sendMessageEmailRequest.Subject.MaskFields(),
                TemplateId = "",
                TemplateParams = "",
                FromMail = sendMessageEmailRequest.From,
                CreatedBy = sendMessageEmailRequest.Process
            };

            mailRequest.MailConfiguration = _transactionManager.MailRequestInfo.MailConfiguration;

            _repositoryManager.MailRequestLogs.Add(mailRequest);

            _transactionManager.Transaction.MailRequestLog = mailRequest;

            var response = await _operatordEngage.SendMail(sendMessageEmailRequest.Email, sendMessageEmailRequest.From, sendMessageEmailRequest.Subject, sendMessageEmailRequest.Content, null, null, sendMessageEmailRequest.Attachments);

            mailRequest.ResponseLogs.Add(response);
            
            sendEmailResponse.Status = response.GetdEngageStatus();

            return sendEmailResponse;
        }

        public async Task<SendEmailResponse> SendTemplatedMail(SendTemplatedEmailRequest sendTemplatedEmailRequest)
        {
            SendEmailResponse sendEmailResponse = new SendEmailResponse() {
                TxnId = _transactionManager.TxnId,
            };

            if (sendTemplatedEmailRequest.HeaderInfo?.Sender != null)
            {
                if (sendTemplatedEmailRequest.HeaderInfo.Sender != SenderType.AutoDetect)
                    _transactionManager.CustomerRequestInfo.BusinessLine = sendTemplatedEmailRequest.HeaderInfo.Sender == SenderType.On ? "X" : "B";
            }
            if (_transactionManager.CustomerRequestInfo.BusinessLine == "X")
                _operatordEngage.Type = OperatorType.dEngageOn;
            else
                _operatordEngage.Type = OperatorType.dEngageBurgan;

            var contentListByteArray = await _distributedCache.GetAsync(_operatordEngage.Type.ToString() + "_MailContents");
            List<ContentInfo> contentList = null;
            if (contentListByteArray == null)
            {
                contentList = await SetMailContents();
            }
            else
            {
                contentList = JsonConvert.DeserializeObject<List<ContentInfo>>(
                        Encoding.UTF8.GetString(contentListByteArray)
                    );
            }

            var templateInfo = contentList.Where(c => c.contentName.Trim() == sendTemplatedEmailRequest.Template.Trim()).FirstOrDefault();
            if (templateInfo == null)
            {
                await SetMailContents();
                templateInfo = contentList.Where(c => c.contentName.Trim() == sendTemplatedEmailRequest.Template.Trim()).FirstOrDefault();
                if (templateInfo == null)
                {
                    throw new WorkflowException("Template Not Found", System.Net.HttpStatusCode.NotFound);
                }
            }

            var mailRequest = new MailRequestLog() {
                Operator = _operatordEngage.Type,
                content = "",
                subject = "",
                TemplateId = templateInfo.publicId,
                TemplateParams = sendTemplatedEmailRequest.TemplateParams?.MaskFields(),
                CreatedBy = sendTemplatedEmailRequest.Process
            };

            mailRequest.MailConfiguration = _transactionManager.MailRequestInfo.MailConfiguration;

            _repositoryManager.MailRequestLogs.Add(mailRequest);

            _transactionManager.Transaction.MailRequestLog = mailRequest;

            var response = await _operatordEngage.SendMail(sendTemplatedEmailRequest.Email, null, null, null,templateInfo.publicId, sendTemplatedEmailRequest.TemplateParams,sendTemplatedEmailRequest.Attachments);

            mailRequest.ResponseLogs.Add(response);
            
            sendEmailResponse.Status = response.GetdEngageStatus();

            return sendEmailResponse;
        }

        public async Task<SendPushNotificationResponse> SendPushNotification(SendMessagePushNotificationRequest sendMessagePushNotificationRequest)
        {

            SendPushNotificationResponse sendPushNotificationResponse = new();


            return sendPushNotificationResponse;
        }

        public async Task<SendPushNotificationResponse> SendTemplatedPushNotification(SendTemplatedPushNotificationRequest sendTemplatedPushNotificationRequest)
        {

            SendPushNotificationResponse sendPushNotificationResponse = new() { 
                TxnId = _transactionManager.TxnId,
            };

            var contentListByteArray = await _distributedCache.GetAsync(_operatordEngage.Type.ToString() + "_PushContents");
            List<PushContentInfo> contentList = null;
            if (contentListByteArray == null)
            {
                contentList = await SetPushContents();
            }
            else
            {
                contentList = JsonConvert.DeserializeObject<List<PushContentInfo>>(
                        Encoding.UTF8.GetString(contentListByteArray)
                    );
            }

            var templateInfo = contentList.Where(c => c.name.Trim() == sendTemplatedPushNotificationRequest.Template.Trim()).FirstOrDefault();
            if (templateInfo == null)
            {
                await SetPushContents();
                templateInfo = contentList.Where(c => c.name.Trim() == sendTemplatedPushNotificationRequest.Template.Trim()).FirstOrDefault();
                if (templateInfo == null)
                {
                    throw new WorkflowException("Template Not Found", System.Net.HttpStatusCode.NotFound);
                }
            }

            var pushRequest = new PushNotificationRequestLog()
            {
                Operator = _operatordEngage.Type,
                TemplateId = templateInfo.id,
                TemplateParams = sendTemplatedPushNotificationRequest.TemplateParams?.MaskFields(),
                ContactId = sendTemplatedPushNotificationRequest.ContactId,
                CustomParameters = sendTemplatedPushNotificationRequest.CustomParameters?.MaskFields(),
                CreatedBy = sendTemplatedPushNotificationRequest.Process
            };

            _repositoryManager.PushNotificationRequestLogs.Add(pushRequest);
            _transactionManager.Transaction.PushNotificationRequestLog = pushRequest;

            var response = await _operatordEngage.SendPush(sendTemplatedPushNotificationRequest.ContactId, templateInfo.id, sendTemplatedPushNotificationRequest.TemplateParams, sendTemplatedPushNotificationRequest.CustomParameters);

            pushRequest.ResponseLogs.Add(response);

            
            sendPushNotificationResponse.Status = response.GetdEngageStatus();

            return sendPushNotificationResponse;
        }

    }
}
