using bbt.gateway.messaging.Api.Turkcell.Model;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace bbt.gateway.messaging.Api.TurkTelekom
{
    public class TurkcellApi
    {
        private readonly IConfiguration _configuration;
        public TurkcellApi(IConfiguration configuration) {
            _configuration = configuration;
        }

        public async Task<TurkcellSmsResponse> SendSms(TurkcellSmsRequest turkcellSmsRequest) {
            await Task.CompletedTask;
            return new TurkcellSmsResponse();
        }
    }
}
