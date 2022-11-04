using bbt.gateway.common.Extensions;
using bbt.gateway.common.Models;
using bbt.gateway.common.Repositories;
using bbt.gateway.messaging.Api.dEngage.Model.Contents;
using bbt.gateway.messaging.Exceptions;
using bbt.gateway.messaging.Workers.OperatorGateway;
using Dapr.Client;
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

        private const string DEFAULT_TEMPLATE_NAME = "Default";
        private const string BURGAN_MAIL_SUFFIX = "@m.burgan.com.tr";
        private const string ON_MAIL_SUFFIX = "@m.on.com.tr";

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

        public async Task<List<ContentInfo>> SetMailContents(OperatorType type)
        {
            List<ContentInfo> mailContents = new List<ContentInfo>();

            int limit = 1000;
            int offsetMultiplexer = 0;
            _operatordEngage.Type = type;
            while (true)
            {
                var response = await _operatordEngage.GetMailContents(limit, (limit * offsetMultiplexer).ToString());
                mailContents.AddRange(response.data.result);
                if (response.data.queryForNextPage)
                {
                    offsetMultiplexer++;
                }
                else
                {
                    break;
                }
            }

            if (mailContents.Count > 0)
            {
                    await _distributedCache.SetAsync(_operatordEngage.Type.ToString() + "_MailContents", Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(mailContents)),
                    new DistributedCacheEntryOptions()
                    {
                        AbsoluteExpiration = DateTimeOffset.UtcNow.AddMonths(1)
                    });
            }

            return mailContents;
        }

        public async Task<List<PushContentInfo>> SetPushContents(OperatorType type)
        {
            List<PushContentInfo> pushContents = new List<PushContentInfo>();

            int limit = 1000;
            int offsetMultiplexer = 0;
            _operatordEngage.Type = type;
            while (true)
            {
                var response = await _operatordEngage.GetPushContents(limit, (limit * offsetMultiplexer).ToString());
                pushContents.AddRange(response.data.result);
                if (response.data.queryForNextPage)
                {
                    offsetMultiplexer++;
                }
                else
                {
                    break;
                }
            }

            if (pushContents.Count > 0)
            {
                await _distributedCache.SetAsync(_operatordEngage.Type.ToString() + "_PushContents", Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(pushContents)),
                    new DistributedCacheEntryOptions()
                    {
                        AbsoluteExpiration = DateTimeOffset.UtcNow.AddMonths(1)
                    });
            }

            return pushContents;
        }

        public async Task<List<SmsContentInfo>> SetSmsContents(OperatorType type)
        {
            List<SmsContentInfo> smsContents = new List<SmsContentInfo>();

            int limit = 1000;
            int offsetMultiplexer = 0;
            _operatordEngage.Type = type;
            while (true)
            {
                var response = await _operatordEngage.GetSmsContents(limit, (limit * offsetMultiplexer).ToString());
                smsContents.AddRange(response.data.result);
                if (response.data.queryForNextPage)
                {
                    offsetMultiplexer++;
                }
                else
                {
                    break;
                }
            }

            if (smsContents.Count > 0)
            {
                await _distributedCache.SetAsync(_operatordEngage.Type.ToString() + "_SmsContents", Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(smsContents)),
                    new DistributedCacheEntryOptions()
                    {
                        AbsoluteExpiration = DateTimeOffset.UtcNow.AddMonths(1)
                    });
            }
            return smsContents;
        }

        public async Task<CheckSmsStatusResponse> CheckSms(CheckSmsStatusRequest checkSmsStatusRequest)
        {
            CheckSmsStatusResponse checkSmsStatusResponse = new();
            var txnInfo = await _repositoryManager.Transactions.GetWithIdAsync(checkSmsStatusRequest.TxnId);
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

        public async Task<SmsTrackingLog> CheckSms(common.Models.v2.CheckFastSmsRequest checkFastSmsRequest)
        {
            _operatordEngage.Type = checkFastSmsRequest.Operator;
            var response = await _operatordEngage.CheckSms(checkFastSmsRequest.StatusQueryId);

            if (response != null)
            {
                if (response.code == 0)
                {
                    if (response.data.result != null)
                    {
                        if (response.data.result.Count() > 0)
                        {
                            return response.BuilddEngageTrackingResponse(checkFastSmsRequest);
                        }
                        else
                        {
                            return new SmsTrackingLog()
                            {
                                Id = Guid.NewGuid(),
                                LogId = checkFastSmsRequest.SmsRequestLogId,
                                Status = SmsTrackingStatus.Pending,
                                Detail = "",
                                StatusReason = $"İletim raporu henüz hazır değil",
                            };
                        }
                    }
                    else
                    {
                        return new SmsTrackingLog()
                        {
                            Id = Guid.NewGuid(),
                            LogId = checkFastSmsRequest.SmsRequestLogId,
                            Status = SmsTrackingStatus.Pending,
                            Detail = "",
                            StatusReason = $"İletim raporu henüz hazır değil",
                        };
                    }
                    
                }
                else
                {
                    return new SmsTrackingLog()
                    {
                        Id = Guid.NewGuid(),
                        LogId = checkFastSmsRequest.SmsRequestLogId,
                        Status = SmsTrackingStatus.SystemError,
                        Detail = "",
                        StatusReason = $"dEngage operatöründen bilgi alınamadı. Response Code : {response.code} | Response Message : {response.message}",
                    };
                }
            }
            else
            {
                return new SmsTrackingLog()
                {
                    Id = Guid.NewGuid(),
                    LogId = checkFastSmsRequest.SmsRequestLogId,
                    Status = SmsTrackingStatus.SystemError,
                    Detail = "",
                    StatusReason = "dEngage operatöründen bilgi alınamadı.",

                };
            }

        }

        public async Task<SendSmsResponse> SendSms(SendMessageSmsRequest sendMessageSmsRequest)
        {
            SendSmsResponse sendSmsResponse = new SendSmsResponse()
            {
                TxnId = _transactionManager.TxnId,
            };

            var header = await _headerManager.Get(sendMessageSmsRequest.ContentType, sendMessageSmsRequest.HeaderInfo);

            if (_transactionManager.CustomerRequestInfo.BusinessLine == "X")
                _operatordEngage.Type = OperatorType.dEngageOn;
            else
                _operatordEngage.Type = OperatorType.dEngageBurgan;

            var smsRequest = new SmsRequestLog()
            {
                Operator = _operatordEngage.Type,
                Phone = sendMessageSmsRequest.Phone,
                content = header.SmsPrefix + " " + sendMessageSmsRequest.Content.MaskFields() + " " + header.SmsSuffix,
                TemplateId = "",
                TemplateParams = "",
                SmsType = sendMessageSmsRequest.SmsType,
                CreatedBy = sendMessageSmsRequest.Process
            };
            smsRequest.PhoneConfiguration = _transactionManager.SmsRequestInfo.PhoneConfiguration;

            await _repositoryManager.SmsRequestLogs.AddAsync(smsRequest);

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
                if (sendTemplatedSmsRequest.HeaderInfo.Sender != SenderType.AutoDetect)
                    _transactionManager.CustomerRequestInfo.BusinessLine = sendTemplatedSmsRequest.HeaderInfo.Sender == SenderType.On ? "X" : "B";
            }

            if (_transactionManager.CustomerRequestInfo.BusinessLine == "X")
                _operatordEngage.Type = OperatorType.dEngageOn;
            else
                _operatordEngage.Type = OperatorType.dEngageBurgan;

            var contentListByteArray = await _distributedCache.GetAsync(_operatordEngage.Type.ToString() + "_SmsContents");
            List<SmsContentInfo> contentList = JsonConvert.DeserializeObject<List<SmsContentInfo>>(
                        Encoding.UTF8.GetString(contentListByteArray)
                    );

            var templateInfo = contentList.Where(c => c.contentName.Trim() == $"{sendTemplatedSmsRequest.Template.Trim()}_{_transactionManager.CustomerRequestInfo.PreferedLanguage}").FirstOrDefault();
            if (templateInfo == null)
            {
                templateInfo = contentList.Where(c => c.contentName.Trim() == sendTemplatedSmsRequest.Template.Trim()).FirstOrDefault();
                if(templateInfo == null)
                    throw new WorkflowException("Template Not Found", System.Net.HttpStatusCode.NotFound);
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

            await _repositoryManager.SmsRequestLogs.AddAsync(smsRequest);
            smsRequest.PhoneConfiguration = _transactionManager.SmsRequestInfo.PhoneConfiguration;
            _transactionManager.Transaction.SmsRequestLog = smsRequest;

            var response = await _operatordEngage.SendSms(sendTemplatedSmsRequest.Phone, SmsTypes.Fast, null, templateInfo.publicId, sendTemplatedSmsRequest.TemplateParams);

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

            await _repositoryManager.MailRequestLogs.AddAsync(mailRequest);

            _transactionManager.Transaction.MailRequestLog = mailRequest;

            var response = await _operatordEngage.SendMail(sendMessageEmailRequest.Email, sendMessageEmailRequest.From, sendMessageEmailRequest.Subject, sendMessageEmailRequest.Content, null, null, sendMessageEmailRequest.Attachments, sendMessageEmailRequest.Cc, sendMessageEmailRequest.Bcc);

            mailRequest.ResponseLogs.Add(response);

            sendEmailResponse.Status = response.GetdEngageStatus();

            return sendEmailResponse;
        }

        public async Task<SendEmailResponse> SendTemplatedMail(SendTemplatedEmailRequest sendTemplatedEmailRequest)
        {
            SendEmailResponse sendEmailResponse = new SendEmailResponse()
            {
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
            List<ContentInfo> contentList = JsonConvert.DeserializeObject<List<ContentInfo>>(
                        Encoding.UTF8.GetString(contentListByteArray)
                    );

            var templateInfo = contentList.Where(c => c.contentName.Trim() == $"{sendTemplatedEmailRequest.Template.Trim()}_{_transactionManager.CustomerRequestInfo.PreferedLanguage}").FirstOrDefault();
            if (templateInfo == null)
            {
                templateInfo = contentList.Where(c => c.contentName.Trim() == sendTemplatedEmailRequest.Template.Trim()).FirstOrDefault();
                if(templateInfo == null)
                    throw new WorkflowException("Template Not Found", System.Net.HttpStatusCode.NotFound);
            }

            var mailRequest = new MailRequestLog()
            {
                Operator = _operatordEngage.Type,
                content = "",
                subject = "",
                TemplateId = templateInfo.publicId,
                TemplateParams = sendTemplatedEmailRequest.TemplateParams?.MaskFields(),
                CreatedBy = sendTemplatedEmailRequest.Process
            };

            mailRequest.MailConfiguration = _transactionManager.MailRequestInfo.MailConfiguration;

            await _repositoryManager.MailRequestLogs.AddAsync(mailRequest);

            _transactionManager.Transaction.MailRequestLog = mailRequest;

            var response = await _operatordEngage.SendMail(sendTemplatedEmailRequest.Email, null, null, null, templateInfo.publicId, sendTemplatedEmailRequest.TemplateParams, sendTemplatedEmailRequest.Attachments, sendTemplatedEmailRequest.Cc, sendTemplatedEmailRequest.Bcc);

            mailRequest.ResponseLogs.Add(response);

            sendEmailResponse.Status = response.GetdEngageStatus();

            return sendEmailResponse;
        }

        public async Task<SendPushNotificationResponse> SendPushNotification(SendMessagePushNotificationRequest sendMessagePushNotificationRequest)
        {
            await Task.CompletedTask;
            SendPushNotificationResponse sendPushNotificationResponse = new();


            return sendPushNotificationResponse;
        }

        public async Task<SendPushNotificationResponse> SendTemplatedPushNotification(SendTemplatedPushNotificationRequest sendTemplatedPushNotificationRequest)
        {

            SendPushNotificationResponse sendPushNotificationResponse = new()
            {
                TxnId = _transactionManager.TxnId,
            };

            if (sendTemplatedPushNotificationRequest.HeaderInfo?.Sender != null)
            {
                if (sendTemplatedPushNotificationRequest.HeaderInfo.Sender != SenderType.AutoDetect)
                    _transactionManager.CustomerRequestInfo.BusinessLine = sendTemplatedPushNotificationRequest.HeaderInfo.Sender == SenderType.On ? "X" : "B";
            }
            if (_transactionManager.CustomerRequestInfo.BusinessLine == "X")
                _operatordEngage.Type = OperatorType.dEngageOn;
            else
                _operatordEngage.Type = OperatorType.dEngageBurgan;

            var contentListByteArray = await _distributedCache.GetAsync(_operatordEngage.Type.ToString() + "_PushContents");
            List<PushContentInfo> contentList = JsonConvert.DeserializeObject<List<PushContentInfo>>(
                        Encoding.UTF8.GetString(contentListByteArray)
                    );

            var templateInfo = contentList.Where(c => c.name.Trim() == $"{sendTemplatedPushNotificationRequest.Template.Trim()}_{_transactionManager.CustomerRequestInfo.PreferedLanguage}").FirstOrDefault();
            if (templateInfo == null)
            {
                templateInfo = contentList.Where(c => c.name.Trim() == sendTemplatedPushNotificationRequest.Template.Trim()).FirstOrDefault();
                if(templateInfo == null)
                    throw new WorkflowException("Template Not Found", System.Net.HttpStatusCode.NotFound);
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

            await _repositoryManager.PushNotificationRequestLogs.AddAsync(pushRequest);
            _transactionManager.Transaction.PushNotificationRequestLog = pushRequest;

            var response = await _operatordEngage.SendPush(sendTemplatedPushNotificationRequest.ContactId, templateInfo.id, sendTemplatedPushNotificationRequest.TemplateParams, sendTemplatedPushNotificationRequest.CustomParameters);

            pushRequest.ResponseLogs.Add(response);

            sendPushNotificationResponse.Status = response.GetdEngageStatus();

            return sendPushNotificationResponse;
        }

        public async Task<common.Models.v2.TemplatedSmsResponse> SendTemplatedSmsV2(common.Models.v2.TemplatedSmsRequest templatedSmsRequest)
        {
            common.Models.v2.TemplatedSmsResponse sendSmsResponse = new common.Models.v2.TemplatedSmsResponse()
            {
                TxnId = _transactionManager.TxnId,
            };

            if (templatedSmsRequest.Sender != common.Models.v2.SenderType.AutoDetect)
                _transactionManager.CustomerRequestInfo.BusinessLine = templatedSmsRequest.Sender == common.Models.v2.SenderType.On ? "X" : "B";


            if (_transactionManager.CustomerRequestInfo.BusinessLine == "X")
                _operatordEngage.Type = OperatorType.dEngageOn;
            else
                _operatordEngage.Type = OperatorType.dEngageBurgan;

            var contentList = await GetContentList<SmsContentInfo>(_operatordEngage.Type.ToString() + "_SmsContents");

            var templateInfo = GetContentInfo(contentList, templatedSmsRequest.Template);

            var smsRequest = new SmsRequestLog()
            {
                Operator = _operatordEngage.Type,
                Phone = new() { CountryCode = templatedSmsRequest.Phone.CountryCode, Prefix = templatedSmsRequest.Phone.Prefix, Number = templatedSmsRequest.Phone.Number },
                content = "",
                TemplateId = templateInfo.publicId,
                TemplateParams = templatedSmsRequest.TemplateParams?.MaskFields(),
                CreatedBy = templatedSmsRequest.Process.MapTo<Process>()
            };

            await _repositoryManager.SmsRequestLogs.AddAsync(smsRequest);
            smsRequest.PhoneConfiguration = _transactionManager.SmsRequestInfo.PhoneConfiguration;
            _transactionManager.Transaction.SmsRequestLog = smsRequest;

            var response = await _operatordEngage.SendSms(templatedSmsRequest.Phone.MapTo<Phone>(), SmsTypes.Fast, null, templateInfo.publicId, templatedSmsRequest.TemplateParams);

            smsRequest.ResponseLogs.Add(response);


            sendSmsResponse.Status = (common.Models.v2.dEngageResponseCodes)response.GetdEngageStatus();

            return sendSmsResponse;
        }

        public async Task<common.Models.v2.TemplatedSmsResponse> SendSmsV2(common.Models.v2.SmsRequest sendSmsRequest)
        {
            common.Models.v2.TemplatedSmsResponse sendSmsResponse = new common.Models.v2.TemplatedSmsResponse()
            {
                TxnId = _transactionManager.TxnId,
            };

            var header = _headerManager.Get(sendSmsRequest.SmsType);

            if (sendSmsRequest.Sender != common.Models.v2.SenderType.AutoDetect)
                _transactionManager.CustomerRequestInfo.BusinessLine = sendSmsRequest.Sender == common.Models.v2.SenderType.On ? "X" : "B";

            if (_transactionManager.CustomerRequestInfo.BusinessLine == "X")
                _operatordEngage.Type = OperatorType.dEngageOn;
            else
                _operatordEngage.Type = OperatorType.dEngageBurgan;


            var smsRequest = new SmsRequestLog()
            {
                Operator = _operatordEngage.Type,
                Phone = new() { CountryCode = sendSmsRequest.Phone.CountryCode, Prefix = sendSmsRequest.Phone.Prefix, Number = sendSmsRequest.Phone.Number },
                content = sendSmsRequest.Content.MaskFields(),
                TemplateId = "",
                TemplateParams = "",
                CreatedBy = sendSmsRequest.Process.MapTo<Process>()
            };

            await _repositoryManager.SmsRequestLogs.AddAsync(smsRequest);
            smsRequest.PhoneConfiguration = _transactionManager.SmsRequestInfo.PhoneConfiguration;
            _transactionManager.Transaction.SmsRequestLog = smsRequest;

            var response = await _operatordEngage.SendSms(sendSmsRequest.Phone.MapTo<Phone>(), (SmsTypes)sendSmsRequest.SmsType, header.BuildContentForSms(sendSmsRequest.Content), null, null);

            smsRequest.ResponseLogs.Add(response);


            sendSmsResponse.Status = (common.Models.v2.dEngageResponseCodes)response.GetdEngageStatus();

            return sendSmsResponse;
        }

        public async Task<common.Models.v2.MailResponse> SendMailV2(common.Models.v2.MailRequest mailRequestDto)
        {
            common.Models.v2.MailResponse sendEmailResponse = new common.Models.v2.MailResponse()
            {
                TxnId = _transactionManager.TxnId,
            };

            if (mailRequestDto.Sender != common.Models.v2.SenderType.AutoDetect)
                _transactionManager.CustomerRequestInfo.BusinessLine = mailRequestDto.Sender == common.Models.v2.SenderType.On ? "X" : "B";


            if (_transactionManager.CustomerRequestInfo.BusinessLine == "X")
            {
                _operatordEngage.Type = OperatorType.dEngageOn;
                mailRequestDto.From += ON_MAIL_SUFFIX;
            }
            else
            {
                _operatordEngage.Type = OperatorType.dEngageBurgan;
                mailRequestDto.From += BURGAN_MAIL_SUFFIX;
            }


            var mailRequest = new MailRequestLog()
            {
                Operator = _operatordEngage.Type,
                content = mailRequestDto.Content.MaskFields(),
                subject = mailRequestDto.Subject.MaskFields(),
                TemplateId = "",
                TemplateParams = "",
                FromMail = mailRequestDto.From,
                CreatedBy = mailRequestDto.Process.MapTo<Process>()
            };

            mailRequest.MailConfiguration = _transactionManager.MailRequestInfo.MailConfiguration;

            await _repositoryManager.MailRequestLogs.AddAsync(mailRequest);

            _transactionManager.Transaction.MailRequestLog = mailRequest;

            var response = await _operatordEngage.SendMail(mailRequestDto.Email, mailRequestDto.From, mailRequestDto.Subject, mailRequestDto.Content, null, null, mailRequestDto.Attachments.ListMapTo<common.Models.v2.Attachment, Attachment>(), mailRequestDto.Cc, mailRequestDto.Bcc);

            mailRequest.ResponseLogs.Add(response);

            sendEmailResponse.Status = (common.Models.v2.dEngageResponseCodes)response.GetdEngageStatus();

            return sendEmailResponse;
        }

        public async Task<common.Models.v2.TemplatedMailResponse> SendTemplatedMailV2(common.Models.v2.TemplatedMailRequest templatedMailRequest)
        {
            common.Models.v2.TemplatedMailResponse templatedEmailResponse = new()
            {
                TxnId = _transactionManager.TxnId,
            };


            if (templatedMailRequest.Sender != common.Models.v2.SenderType.AutoDetect)
                _transactionManager.CustomerRequestInfo.BusinessLine = templatedMailRequest.Sender == common.Models.v2.SenderType.On ? "X" : "B";

            if (_transactionManager.CustomerRequestInfo.BusinessLine == "X")
                _operatordEngage.Type = OperatorType.dEngageOn;
            else
                _operatordEngage.Type = OperatorType.dEngageBurgan;

            var contentList = await GetContentList<ContentInfo>(_operatordEngage.Type.ToString() + "_MailContents");

            var templateInfo = GetContentInfo(contentList,templatedMailRequest.Template);

            var mailRequest = new MailRequestLog()
            {
                Operator = _operatordEngage.Type,
                content = "",
                subject = "",
                TemplateId = templateInfo.publicId,
                TemplateParams = templatedMailRequest.TemplateParams?.MaskFields(),
                CreatedBy = templatedMailRequest.Process.MapTo<Process>()
            };

            mailRequest.MailConfiguration = _transactionManager.MailRequestInfo.MailConfiguration;

            await _repositoryManager.MailRequestLogs.AddAsync(mailRequest);

            _transactionManager.Transaction.MailRequestLog = mailRequest;

            var response = await _operatordEngage.SendMail(templatedMailRequest.Email, null, null, null, templateInfo.publicId, templatedMailRequest.TemplateParams, templatedMailRequest.Attachments.ListMapTo<common.Models.v2.Attachment, Attachment>(), templatedMailRequest.Cc, templatedMailRequest.Bcc);

            mailRequest.ResponseLogs.Add(response);

            templatedEmailResponse.Status = (common.Models.v2.dEngageResponseCodes)response.GetdEngageStatus();

            return templatedEmailResponse;
        }

        public async Task<common.Models.v2.TemplatedPushResponse> SendTemplatedPushNotificationV2(common.Models.v2.TemplatedPushRequest sendTemplatedPushNotificationRequest)
        {
            if (String.IsNullOrWhiteSpace(sendTemplatedPushNotificationRequest.CitizenshipNo))
            {
                sendTemplatedPushNotificationRequest.CitizenshipNo = _transactionManager.CustomerRequestInfo.Tckn;
            }

            common.Models.v2.TemplatedPushResponse sendPushNotificationResponse = new()
            {
                TxnId = _transactionManager.TxnId,
            };


            if (sendTemplatedPushNotificationRequest.Sender != common.Models.v2.SenderType.AutoDetect)
                _transactionManager.CustomerRequestInfo.BusinessLine = sendTemplatedPushNotificationRequest.Sender == common.Models.v2.SenderType.On ? "X" : "B";

            if (_transactionManager.CustomerRequestInfo.BusinessLine == "X")
                _operatordEngage.Type = OperatorType.dEngageOn;
            else
                _operatordEngage.Type = OperatorType.dEngageBurgan;

            
            var contentList = await GetContentList<PushContentInfo>(_operatordEngage.Type.ToString() + "_PushContents");

            var templateInfo = GetContentInfo(contentList, sendTemplatedPushNotificationRequest.Template);

            var pushRequest = new PushNotificationRequestLog()
            {
                Operator = _operatordEngage.Type,
                TemplateId = templateInfo.id,
                TemplateParams = sendTemplatedPushNotificationRequest.TemplateParams?.MaskFields(),
                ContactId = sendTemplatedPushNotificationRequest.CitizenshipNo,
                CustomParameters = sendTemplatedPushNotificationRequest.CustomParameters?.MaskFields(),
                CreatedBy = sendTemplatedPushNotificationRequest.Process.MapTo<Process>()
            };

            await _repositoryManager.PushNotificationRequestLogs.AddAsync(pushRequest);
            _transactionManager.Transaction.PushNotificationRequestLog = pushRequest;

            var response = await _operatordEngage.SendPush(sendTemplatedPushNotificationRequest.CitizenshipNo, templateInfo.id, sendTemplatedPushNotificationRequest.TemplateParams, sendTemplatedPushNotificationRequest.CustomParameters);

            pushRequest.ResponseLogs.Add(response);


            sendPushNotificationResponse.Status = (common.Models.v2.dEngageResponseCodes)response.GetdEngageStatus();

            return sendPushNotificationResponse;
        }

        public async Task<common.Models.v2.PushResponse> SendPushNotificationV2(common.Models.v2.PushRequest sendPushNotificationRequest)
        {

            if (String.IsNullOrWhiteSpace(sendPushNotificationRequest.CitizenshipNo))
            {
                sendPushNotificationRequest.CitizenshipNo = _transactionManager.CustomerRequestInfo.Tckn;
            }

            common.Models.v2.PushResponse sendPushNotificationResponse = new()
            {
                TxnId = _transactionManager.TxnId,
            };


            if (sendPushNotificationRequest.Sender != common.Models.v2.SenderType.AutoDetect)
                _transactionManager.CustomerRequestInfo.BusinessLine = sendPushNotificationRequest.Sender == common.Models.v2.SenderType.On ? "X" : "B";

            if (_transactionManager.CustomerRequestInfo.BusinessLine == "X")
                _operatordEngage.Type = OperatorType.dEngageOn;
            else
                _operatordEngage.Type = OperatorType.dEngageBurgan;

            var contentListByteArray = await _distributedCache.GetAsync(_operatordEngage.Type.ToString() + "_PushContents");
            List<PushContentInfo> contentList = JsonConvert.DeserializeObject<List<PushContentInfo>>(
                        Encoding.UTF8.GetString(contentListByteArray)
                    );

            var templateInfo = contentList.Where(c => c.name.Trim() == DEFAULT_TEMPLATE_NAME).FirstOrDefault();
            if (templateInfo == null)
            {
                throw new WorkflowException("Template Not Found", System.Net.HttpStatusCode.NotFound);
            }

            var templateParams = JsonConvert.SerializeObject(new
            {
                Content = sendPushNotificationRequest.Content
            });

            var pushRequest = new PushNotificationRequestLog()
            {
                Operator = _operatordEngage.Type,
                TemplateId = templateInfo.id,
                TemplateParams = templateParams.ToString().MaskFields(),
                ContactId = sendPushNotificationRequest.CitizenshipNo,
                CustomParameters = "",
                CreatedBy = sendPushNotificationRequest.Process.MapTo<Process>()
            };

            await _repositoryManager.PushNotificationRequestLogs.AddAsync(pushRequest);
            _transactionManager.Transaction.PushNotificationRequestLog = pushRequest;

            var response = await _operatordEngage.SendPush(sendPushNotificationRequest.CitizenshipNo, templateInfo.id, templateParams.ToString(), null);

            pushRequest.ResponseLogs.Add(response);


            sendPushNotificationResponse.Status = (common.Models.v2.dEngageResponseCodes)response.GetdEngageStatus();

            return sendPushNotificationResponse;
        }

        public string GetSmsType(common.Models.v2.SmsTypes smsType)
        {
            return smsType == common.Models.v2.SmsTypes.Bulk ? "Bulk" : "Fast";
        }

        public string GetTemplateNameWithPreferredLang(string template)
        {
            return $"{template.Trim()}_{_transactionManager.CustomerRequestInfo.PreferedLanguage}";
        }

        public string GetTemplateName(string template)
        {
            return template.Trim();
        }

        public T GetContentInfo<T>(List<T> contentList,string givenTemplate) where T : IContentReadeble
        {
            var templateInfo = contentList.Where(c => c.GetPath(givenTemplate.Trim().StartsWith("/")) == GetTemplateNameWithPreferredLang(givenTemplate)).FirstOrDefault();
            if (templateInfo == null)
            {
                templateInfo = contentList.Where(c => c.GetPath(givenTemplate.Trim().StartsWith("/")) == GetTemplateName(givenTemplate)).FirstOrDefault();
                if (templateInfo == null)
                    throw new WorkflowException("Template Not Found", System.Net.HttpStatusCode.NotFound);
            }

            return templateInfo;
        }

        public async Task<List<T>> GetContentList<T>(string templateListPath)
        {
            var contentListByteArray = await _distributedCache.GetAsync(templateListPath);
            return JsonConvert.DeserializeObject<List<T>>(
                        Encoding.UTF8.GetString(contentListByteArray)
                    );
        }
        
    }

}
