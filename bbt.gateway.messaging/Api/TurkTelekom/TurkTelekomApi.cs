using bbt.gateway.messaging.Api.TurkTelekom.Model;
using bbt.gateway.messaging.Workers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace bbt.gateway.messaging.Api.TurkTelekom
{
    public class TurkTelekomApi:BaseApi
    {
        private readonly ILogger<TurkTelekomApi> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        public TurkTelekomApi(ILogger<TurkTelekomApi> logger,
            IHttpClientFactory httpClientFactory,OperatorManager operatorManager) {
            _logger = logger;
            Type = Models.OperatorType.TurkTelekom;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<TurkTelekomSmsResponse> SendSms(TurkTelekomSmsRequest turkTelekomSmsRequest) 
        {

            
            try
            {
                var requestBody = turkTelekomSmsRequest.SerializeXml();
                var httpRequest = new StringContent(requestBody, Encoding.UTF8, "text/xml");
                using var httpClient = _httpClientFactory.CreateClient();
                var httpResponse = await httpClient.PostAsync(OperatorConfig.SendService, httpRequest);
                var response = httpResponse.Content.ReadAsStringAsync().Result;

                if (httpResponse.IsSuccessStatusCode)
                { 
                    var turkTelekomSmsResponse = response.DeserializeXml<TurkTelekomSmsResponse>();
                    turkTelekomSmsResponse.RequestBody = requestBody;
                    turkTelekomSmsResponse.ResponseBody = response;
                    return turkTelekomSmsResponse;
                }
                else
                {
                    var turkTelekomSmsResponse = response.DeserializeXml<TurkTelekomSmsResponse>();
                    turkTelekomSmsResponse.RequestBody = requestBody;
                    turkTelekomSmsResponse.ResponseBody = response;
                    return turkTelekomSmsResponse;
                }

                
            }
            catch (System.Exception ex)
            {
                _logger.LogError("TurkTelekom Send Sms Failed | Exception : " + ex.ToString());
                var response = new TurkTelekomSmsResponse();
                response.ResponseSms = new();
                response.ResponseSms.MessageId = "";
                response.ResponseSms.ResponseCode = "-99999";
                response.ResponseSms.ResponseMessage = ex.ToString();
                return response;
            }

        }

        public async Task<TurkTelekomSmsStatusResponse> CheckSmsStatus(TurkTelekomSmsStatusRequest turkTelekomSmsStatusRequest)
        {           

            try
            {
                var requestBody = turkTelekomSmsStatusRequest.SerializeXml();
                var httpRequest = new StringContent(requestBody, Encoding.UTF8, "text/xml");
                using var httpClient = new HttpClient();
                var httpResponse = await httpClient.PostAsync(OperatorConfig.QueryService, httpRequest);
                var response = httpResponse.Content.ReadAsStringAsync().Result;

                if (httpResponse.IsSuccessStatusCode)
                {
                    var turkTelekomSmsStatusResponse = response.DeserializeXml<TurkTelekomSmsStatusResponse>();
                    turkTelekomSmsStatusResponse.SetFullResponse(response);
                    return turkTelekomSmsStatusResponse;
                }
                else
                {
                    var turkTelekomSmsStatusResponse = response.DeserializeXml<TurkTelekomSmsStatusResponse>();
                    turkTelekomSmsStatusResponse.SetFullResponse(response);
                    return turkTelekomSmsStatusResponse;
                }

                
            }
            catch (System.Exception ex)
            {
                _logger.LogError("TurkTelekom Sms Status Failed | Exception : " + ex.ToString());
                var response = new TurkTelekomSmsStatusResponse();
                response.ResponseSmsStatus = new();
                response.ResponseSmsStatus.Status = "-99999";
                response.ResponseSmsStatus.StatusDesc = ex.ToString();
                return response;
            }
        }
    }
}
