using bbt.gateway.common.Models;
using bbt.gateway.messaging.Api;
using bbt.gateway.messaging.Api.dEngage.Model.Contents;
using bbt.gateway.messaging.Api.dEngage.Model.Transactional;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace bbt.gateway.messaging.Workers.OperatorGateway
{
    public interface IOperatordEngage:IOperatorGatewayBase
    {
        public Task<MailResponseLog> SendMail(string to, string? from, string? subject, string? html, string? templateId, string? templateParams, List<common.Models.Attachment> attachments,string? cc,string? bcc);
        public Task<SmsResponseLog> SendSms(Phone phone, SmsTypes smsType, string? content, string? templateId, string? templateParams);
        public Task<PushNotificationResponseLog> SendPush(string contactId, string template, string templateParams, string customParameters);
        public Task<SmsStatusResponse> CheckSms(string queryId);
        public Task<MailContentsResponse> GetMailContents(int limit,string offset);
        public Task<SmsContentsResponse> GetSmsContents(int limit, string offset);
        public Task<PushContentsResponse> GetPushContents(int limit, string offset);

    }
}
