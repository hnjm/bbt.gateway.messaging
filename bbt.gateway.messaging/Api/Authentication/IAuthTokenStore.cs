using Refit;

namespace bbt.gateway.messaging.Api.dEngage.Authentication
{
    public interface IAuthTokenStore
    {
        public string GetToken();
    }
}