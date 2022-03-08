namespace bbt.gateway.messaging.Api.dEngage.Authentication
{
    public class AuthTokenStore : IAuthTokenStore
    {
        private readonly IdEngageClient _dEngageClient;
        private readonly IRepositoryManager _repositoryManager;
        public AuthTokenStore(IdEngageClient dEngageClient,IRepositoryManager repositoryManager)
        {
            _dEngageClient = dEngageClient;
            _repositoryManager = repositoryManager;
        }

        public string GetToken()
        {
            
        }
    }
}