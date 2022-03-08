using bbt.gateway.messaging.Api.dEngage.Model;
using Refit;

namespace bbt.gateway.messaging.Api.dEngage
{
    public interface IdEngageClient
    {
        [Headers("Authorization")]
        [Post("Login")]
        Task<LoginResponse> Login(LoginRequest loginRequest);

        [Post("transactional/sms")]
        Task<TransactionalSmsResponse> SendTranscationalSms(TransactionalSmsRequest request);
    }
}