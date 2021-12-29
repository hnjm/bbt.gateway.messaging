using bbt.gateway.messaging.Api.Vodafone.Model;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Threading.Tasks;

namespace bbt.gateway.messaging.Api.Vodafone
{
    public class VodafoneApi
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        public VodafoneApi(IConfiguration configuration) {
            _configuration = configuration;
            _httpClient = new HttpClient();
        }

        public async Task<VodafoneSmsResponse> SendSms(VodafoneSmsRequest vodafoneSmsRequest) {
            await Task.CompletedTask;
            return new VodafoneSmsResponse();
        }
    }
}
