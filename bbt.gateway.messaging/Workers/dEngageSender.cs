using bbt.gateway.common.Models;
using bbt.gateway.common.Repositories;
using bbt.gateway.messaging.Workers.OperatorGateway;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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

        PhoneConfiguration phoneConfiguration;

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

        public async Task<SendSmsResponseStatus> SendSms(SendMessageSmsRequest sendMessageSmsRequest)
        {

            SendSmsResponseStatus sendSmsResponseStatus = SendSmsResponseStatus.ClientError;
            

            return sendSmsResponseStatus;
        }

        public async Task<SendSmsResponseStatus> SendTemplatedSms(SendTemplatedSmsRequest sendTemplatedSmsRequest)
        {

            SendSmsResponseStatus sendSmsResponseStatus = SendSmsResponseStatus.ClientError;


            return sendSmsResponseStatus;
        }

        public async Task<SendEmailResponse> SendMail(SendMessageEmailRequest sendMessageEmailRequest)
        {

            SendEmailResponse sendEmailResponse = new();


            return sendEmailResponse;
        }

        public async Task<SendEmailResponse> SendTemplatedMail(SendTemplatedEmailRequest sendTemplatedEmailRequest)
        {
                
            var response = await _operatordEngage.SendMail(sendTemplatedEmailRequest.Email, null, null, sendTemplatedEmailRequest.Template, sendTemplatedEmailRequest.TeamplateParams);
            return new SendEmailResponse();
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



    }
}
