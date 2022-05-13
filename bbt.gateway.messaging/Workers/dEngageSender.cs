using bbt.gateway.common.Models;
using bbt.gateway.common.Repositories;
using bbt.gateway.messaging.Workers.OperatorGateway;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace bbt.gateway.messaging.Workers
{
    public class dEngageSender
    {
        private readonly HeaderManager _headerManager;
        private readonly IRepositoryManager _repositoryManager;
        private readonly ITransactionManager _transactionManager;
        private readonly OperatordEngage _operatordEngage;

        public dEngageSender(HeaderManager headerManager,
            IRepositoryManager repositoryManager,
            ITransactionManager transactionManager,
            OperatordEngage operatordEngage)
        {
            _headerManager = headerManager;
            _repositoryManager = repositoryManager;
            _transactionManager = transactionManager;
            _operatordEngage = operatordEngage;
        }

        public async Task<CheckSmsStatusResponse> CheckSms(CheckSmsStatusRequest checkSmsStatusRequest)
        {
            CheckSmsStatusResponse checkSmsStatusResponse = new();
            var txnInfo = _repositoryManager.Transactions.GetWithId(checkSmsStatusRequest.TxnId);
            var responseLog = txnInfo?.SmsRequestLog?.ResponseLogs?.Where(r => r.OperatorResponseCode == 0).SingleOrDefault();
            if (responseLog != null)
            {
                if (responseLog.Operator == OperatorType.dEngageOn)
                    _operatordEngage.Type = OperatorType.dEngageOn;
                else
                    _operatordEngage.Type = OperatorType.dEngageBurgan;

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

            if (_transactionManager.CustomerRequestInfo.BusinessLine == "X")
                _operatordEngage.Type = OperatorType.dEngageOn;
            else
                _operatordEngage.Type = OperatorType.dEngageBurgan;

            var header =  _headerManager.Get(_transactionManager.SmsRequestInfo.PhoneConfiguration, sendMessageSmsRequest.ContentType, sendMessageSmsRequest.HeaderInfo);

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

            var response = await _operatordEngage.SendSms(sendMessageSmsRequest.Phone, sendMessageSmsRequest.SmsType, header.BuildContentForSms(sendMessageSmsRequest.Content), null, null);
            
            smsRequest.ResponseLogs.Add(response);
            smsRequest.PhoneConfiguration = _transactionManager.SmsRequestInfo.PhoneConfiguration;

            _repositoryManager.SmsRequestLogs.Add(smsRequest);

            _repositoryManager.SaveChanges();
            _transactionManager.SmsRequestLog = smsRequest;
            sendSmsResponse.Status = response.GetdEngageStatus();

            return sendSmsResponse;
        }

        public async Task<SendSmsResponse> SendTemplatedSms(SendTemplatedSmsRequest sendTemplatedSmsRequest)
        {
            SendSmsResponse sendSmsResponse = new SendSmsResponse()
            {
                TxnId = _transactionManager.TxnId,
            };

            if (_transactionManager.CustomerRequestInfo.BusinessLine == "X")
                _operatordEngage.Type = OperatorType.dEngageOn;
            else
                _operatordEngage.Type = OperatorType.dEngageBurgan;

            var smsRequest = new SmsRequestLog()
            {
                Operator = _operatordEngage.Type,
                Phone = sendTemplatedSmsRequest.Phone,
                content = "",
                TemplateId = sendTemplatedSmsRequest.Template,
                TemplateParams = sendTemplatedSmsRequest.TemplateParams.MaskFields(),
                CreatedBy = sendTemplatedSmsRequest.Process
            };


            var response = await _operatordEngage.SendSms(sendTemplatedSmsRequest.Phone, SmsTypes.Fast,null, sendTemplatedSmsRequest.Template, sendTemplatedSmsRequest.TemplateParams);

            smsRequest.ResponseLogs.Add(response);
            smsRequest.PhoneConfiguration = _transactionManager.SmsRequestInfo.PhoneConfiguration;

            _repositoryManager.SmsRequestLogs.Add(smsRequest);

            _repositoryManager.SaveChanges();
            _transactionManager.SmsRequestLog = smsRequest;
            sendSmsResponse.Status = response.GetdEngageStatus();

            return sendSmsResponse;
        }

        public async Task<SendEmailResponse> SendMail(SendMessageEmailRequest sendMessageEmailRequest)
        {
            SendEmailResponse sendEmailResponse = new SendEmailResponse()
            {
                TxnId = _transactionManager.TxnId,
            };

            if (_transactionManager.CustomerRequestInfo.BusinessLine == "X")
                _operatordEngage.Type = OperatorType.dEngageOn;
            else
                _operatordEngage.Type = OperatorType.dEngageBurgan;

            var mailRequest = new MailRequestLog()
            {
                Operator = _operatordEngage.Type,
                content = sendMessageEmailRequest.Content.MaskFields(),
                subject = sendMessageEmailRequest.Content.MaskFields(),
                TemplateId = "",
                TemplateParams = "",
                FromMail = sendMessageEmailRequest.From,
                CreatedBy = sendMessageEmailRequest.Process
            };


            var response = await _operatordEngage.SendMail(sendMessageEmailRequest.Email, sendMessageEmailRequest.From, sendMessageEmailRequest.Subject, sendMessageEmailRequest.Content, null, null, sendMessageEmailRequest.Attachments);

            mailRequest.ResponseLogs.Add(response);
            mailRequest.MailConfiguration = _transactionManager.MailRequestInfo.MailConfiguration;

            _repositoryManager.MailRequestLogs.Add(mailRequest);

            _repositoryManager.SaveChanges();
            _transactionManager.MailRequestLog = mailRequest;
            sendEmailResponse.Status = response.GetdEngageStatus();

            return sendEmailResponse;
        }

        public async Task<SendEmailResponse> SendTemplatedMail(SendTemplatedEmailRequest sendTemplatedEmailRequest)
        {
            SendEmailResponse sendEmailResponse = new SendEmailResponse() {
                TxnId = _transactionManager.TxnId,
            };

            if (_transactionManager.CustomerRequestInfo.BusinessLine == "X")
                _operatordEngage.Type = OperatorType.dEngageOn;
            else
                _operatordEngage.Type = OperatorType.dEngageBurgan;

            var mailRequest = new MailRequestLog() {
                Operator = _operatordEngage.Type,
                content = "",
                subject = "",
                TemplateId = sendTemplatedEmailRequest.Template,
                TemplateParams = sendTemplatedEmailRequest.TemplateParams.MaskFields(),
                CreatedBy = sendTemplatedEmailRequest.Process
            };

         
            var response = await _operatordEngage.SendMail(sendTemplatedEmailRequest.Email, null, null, null, sendTemplatedEmailRequest.Template, sendTemplatedEmailRequest.TemplateParams,sendTemplatedEmailRequest.Attachments);

            mailRequest.ResponseLogs.Add(response);
            mailRequest.MailConfiguration = _transactionManager.MailRequestInfo.MailConfiguration;

            _repositoryManager.MailRequestLogs.Add(mailRequest);

            _repositoryManager.SaveChanges();
            _transactionManager.MailRequestLog = mailRequest;
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

            if (_transactionManager.CustomerRequestInfo.BusinessLine == "X")
                _operatordEngage.Type = OperatorType.dEngageOn;
            else
                _operatordEngage.Type = OperatorType.dEngageBurgan;

            var pushRequest = new PushNotificationRequestLog()
            {
                Operator = _operatordEngage.Type,
                TemplateId = sendTemplatedPushNotificationRequest.Template,
                TemplateParams = sendTemplatedPushNotificationRequest.TemplateParams?.MaskFields(),
                ContactId = sendTemplatedPushNotificationRequest.ContactId,
                CustomParameters = sendTemplatedPushNotificationRequest.CustomParameters?.MaskFields(),
                CreatedBy = sendTemplatedPushNotificationRequest.Process
            };


            var response = await _operatordEngage.SendPush(sendTemplatedPushNotificationRequest.ContactId, sendTemplatedPushNotificationRequest.Template, sendTemplatedPushNotificationRequest.TemplateParams, sendTemplatedPushNotificationRequest.CustomParameters);

            pushRequest.ResponseLogs.Add(response);

            _repositoryManager.PushNotificationRequestLogs.Add(pushRequest);

            _repositoryManager.SaveChanges();
            _transactionManager.PushNotificationRequestLog = pushRequest;
            sendPushNotificationResponse.Status = response.GetdEngageStatus();

            return sendPushNotificationResponse;
        }

    }
}
