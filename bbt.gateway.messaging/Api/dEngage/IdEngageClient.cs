using Refit;
using System.Threading.Tasks;
using bbt.gateway.messaging.Api.dEngage.Model.Login;
using bbt.gateway.messaging.Api.dEngage.Model.Settings;
using bbt.gateway.messaging.Api.dEngage.Model.Transactional;
using System.Net.Http;
using bbt.gateway.messaging.Api.dEngage.Model.Contents;

namespace bbt.gateway.messaging.Api.dEngage
{
    public interface IdEngageClient
    {
        [Post("/login")]
        Task<LoginResponse> Login(LoginRequest loginRequest);

        [Get("/sms/froms/bulk")]
        Task<GetSmsFromsResponse> GetSmsFroms([Authorize("Bearer")] string token);
        [Get("/email/froms")]
        Task<GetMailFromsResponse> GetMailFroms([Authorize("Bearer")] string token);

        [Post("/transactional/sms")]
        Task<SendSmsResponse> SendSms(SendSmsRequest sendSmsRequest, [Authorize("Bearer")] string token);
        [Post("/transactional/email")]
        Task<SendSmsResponse> SendMail(SendMailRequest sendMailRequest, [Authorize("Bearer")] string token);
        [Post("/transactional/push")]
        Task<SendPushResponse> SendPush(SendPushRequest sendPushRequest, [Authorize("Bearer")] string token);

        [Get("/transactional/sms")]
        Task<SmsStatusResponse> GetSmsStatus([Authorize("Bearer")] string token,SmsStatusRequest smsStatusRequest);
        [Get("/transactional/email")]
        Task<GetSmsFromsResponse> GetMailStatus([Authorize("Bearer")] string token);
        [Get("/contents/email")]
        Task<MailContentsResponse> GetMailContents([Authorize("Bearer")] string token,int limit);
        [Get("/contents/sms")]
        Task<SmsContentsResponse> GetSmsContents([Authorize("Bearer")] string token,int limit);
        [Get("/contents/push")]
        Task<PushContentsResponse> GetPushContents([Authorize("Bearer")] string token,int limit);

        [Get("/contents/email/{id}")]
        Task<MailContentResponse> GetMailContent([Authorize("Bearer")] string token, [AliasAs("id")] string templateId);
        [Get("/contents/sms/{id}")]
        Task<SmsContentResponse> GetSmsContent([Authorize("Bearer")] string token, [AliasAs("id")] string templateId);
        [Get("/contents/push/{id}")]
        Task<PushContentResponse> GetPushContent([Authorize("Bearer")] string token, [AliasAs("id")] string templateId);
    }
}
