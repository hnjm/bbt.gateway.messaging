using bbt.gateway.common.Models;
using bbt.gateway.common.Repositories;
using bbt.gateway.messaging.Workers.OperatorGateway;
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

        public async Task<SendSmsResponse> SendSms(SendMessageSmsRequest sendMessageSmsRequest)
        {
            SendSmsResponse sendSmsResponse = new SendSmsResponse()
            {
                TxnId = _transactionManager.TxnId,
            };

            var header = await _headerManager.Get(_transactionManager.SmsRequestInfo.PhoneConfiguration, sendMessageSmsRequest.ContentType, sendMessageSmsRequest.HeaderInfo);

            var contentWithHeader = header.SmsPrefix + " " + sendMessageSmsRequest.Content + " " + header.SmsSuffix;

            var smsRequest = new SmsRequestLog()
            {
                Phone = sendMessageSmsRequest.Phone,
                content = header.SmsPrefix+" "+sendMessageSmsRequest.Content.MaskFields()+" "+header.SmsSuffix,
                TemplateId = "",
                TemplateParams = "",
                SmsType = sendMessageSmsRequest.SmsType,
                CreatedBy = sendMessageSmsRequest.Process
            };

            var response = await _operatordEngage.SendSms(sendMessageSmsRequest.Phone, sendMessageSmsRequest.SmsType, contentWithHeader, null, null);

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

            var smsRequest = new SmsRequestLog()
            {
                Phone = sendTemplatedSmsRequest.Phone,
                content = "",
                TemplateId = sendTemplatedSmsRequest.Template,
                TemplateParams = sendTemplatedSmsRequest.TemplateParams.MaskFields(),
                SmsType = sendTemplatedSmsRequest.SmsType,
                CreatedBy = sendTemplatedSmsRequest.Process
            };


            var response = await _operatordEngage.SendSms(sendTemplatedSmsRequest.Phone, sendTemplatedSmsRequest.SmsType,null, sendTemplatedSmsRequest.Template, sendTemplatedSmsRequest.TemplateParams);

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


            var mailRequest = new MailRequestLog()
            {
                content = sendMessageEmailRequest.Content.MaskFields(),
                subject = sendMessageEmailRequest.Content.MaskFields(),
                TemplateId = "",
                TemplateParams = "",
                FromMail = sendMessageEmailRequest.From,
                CreatedBy = sendMessageEmailRequest.Process
            };


            var response = await _operatordEngage.SendMail(sendMessageEmailRequest.Email, sendMessageEmailRequest.From, sendMessageEmailRequest.Subject, sendMessageEmailRequest.Content, null, null);

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


            var mailRequest = new MailRequestLog() {
                content = "",
                subject = "",
                TemplateId = sendTemplatedEmailRequest.Template,
                TemplateParams = sendTemplatedEmailRequest.TemplateParams.MaskFields(),
                CreatedBy = sendTemplatedEmailRequest.Process
            };

         
            var response = await _operatordEngage.SendMail(sendTemplatedEmailRequest.Email, null, null, null, sendTemplatedEmailRequest.Template, sendTemplatedEmailRequest.TemplateParams);

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

            SendPushNotificationResponse sendPushNotificationResponse = new();


            return sendPushNotificationResponse;
        }

        //private MailConfiguration CreateMailConfiguration()
        //{
        //    var mailConfiguration = new MailConfiguration()
        //    {
        //        CustomerNo = _transactionManager.CustomerNo,
        //        Email = _sendTemplatedEmailRequest.Email,
        //    };

        //    mailConfiguration.Logs = new List<MailConfigurationLog>() {new MailConfigurationLog() {
        //        Action = "Initialize",
        //        Type = "Add",
        //        CreatedBy = _sendTemplatedEmailRequest.Process,
        //        Mail = mailConfiguration
        //    }};

        //    return mailConfiguration;
        //}



    }
}
