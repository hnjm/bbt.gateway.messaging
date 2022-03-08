using Refit;

namespace bbt.gateway.messaging.Api.dEngage.Authentication
{
    public class AuthHeaderHandler : DelegatingHandler
    {
        private readonly IAuthTokenStore _authTokenStore;

        public AuthHeaderHandler(IAuthTokenStore authTokenStore)
        {
            this._authTokenStore = authTokenStore;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
        var token = await authTokenStore.GetToken();

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
        }
    }
}