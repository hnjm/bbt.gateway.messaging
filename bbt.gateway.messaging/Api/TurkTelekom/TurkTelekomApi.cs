using bbt.gateway.messaging.Api.TurkTelekom.Model;
using bbt.gateway.common.Models;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace bbt.gateway.messaging.Api.TurkTelekom
{
    public class TurkTelekomApi:BaseApi
    {
        private readonly ILogger<TurkTelekomApi> _logger;
        private readonly HttpClient _httpClient;
        public TurkTelekomApi(ILogger<TurkTelekomApi> logger) {
            _logger = logger;
            Type = OperatorType.TurkTelekom;

            var httpClientHandler = new HttpClientHandler();
            httpClientHandler.UseProxy = false;
            _httpClient = new(httpClientHandler);
        }

        public async Task<OperatorApiResponse> SendSms(TurkTelekomSmsRequest turkTelekomSmsRequest) 
        {
            OperatorApiResponse operatorApiResponse = new() { OperatorType = this.Type };
            string response = "";
            try
            {
                var requestBody = turkTelekomSmsRequest.SerializeXml();
                var httpRequest = new StringContent(requestBody, Encoding.UTF8, "text/xml");
                var httpResponse = await _httpClient.PostAsync(OperatorConfig.SendService, httpRequest);
                response = httpResponse.Content.ReadAsStringAsync().Result;

                turkTelekomSmsRequest.Message = turkTelekomSmsRequest.Message.MaskOtpContent();

                if (httpResponse.IsSuccessStatusCode)
                { 
                    var turkTelekomSmsResponse = response.DeserializeXml<TurkTelekomSmsResponse>();
                    operatorApiResponse.ResponseCode = turkTelekomSmsResponse.ResponseSms.ResponseCode;
                    operatorApiResponse.ResponseMessage = turkTelekomSmsResponse.ResponseSms.ResponseMessage;
                    operatorApiResponse.MessageId = turkTelekomSmsResponse.ResponseSms.MessageId;
                    operatorApiResponse.RequestBody = turkTelekomSmsRequest.SerializeXml();
                    operatorApiResponse.ResponseBody = response;
                }
                else
                {
                    operatorApiResponse.ResponseCode = "-99999";
                    operatorApiResponse.ResponseMessage = "Http Status Code : 500";
                    operatorApiResponse.MessageId = "";
                    operatorApiResponse.RequestBody = turkTelekomSmsRequest.SerializeXml();
                    operatorApiResponse.ResponseBody = response;
                }

                
            }
            catch (System.Exception ex)
            {
                _logger.LogError("TurkTelekom Send Sms Failed | Exception : " + ex.ToString());
                operatorApiResponse.ResponseCode = "-99999";
                operatorApiResponse.ResponseMessage = ex.ToString();
                operatorApiResponse.MessageId = "";
                operatorApiResponse.RequestBody = turkTelekomSmsRequest.SerializeXml();
                operatorApiResponse.ResponseBody = response;
            }

            return operatorApiResponse;

        }

        public async Task<OperatorApiTrackingResponse> CheckSmsStatus(TurkTelekomSmsStatusRequest turkTelekomSmsStatusRequest)
        {
            OperatorApiTrackingResponse operatorApiTrackingResponse = new() { OperatorType = this.Type };
            string response = "";
            try
            {
                var requestBody = turkTelekomSmsStatusRequest.SerializeXml();
                var httpRequest = new StringContent(requestBody, Encoding.UTF8, "text/xml");
                var httpResponse = await _httpClient.PostAsync(OperatorConfig.QueryService, httpRequest);
                response = httpResponse.Content.ReadAsStringAsync().Result;

                if (httpResponse.IsSuccessStatusCode)
                {
                    var turkTelekomSmsStatusResponse = response.DeserializeXml<TurkTelekomSmsStatusResponse>();
                    operatorApiTrackingResponse.ResponseCode = turkTelekomSmsStatusResponse.ResponseCode;
                    operatorApiTrackingResponse.ResponseMessage = turkTelekomSmsStatusResponse.ResponseMessage;
                    turkTelekomSmsStatusResponse.ResponseBody = response;
                }
                else
                {
                    operatorApiTrackingResponse.ResponseCode = "-99999";
                    operatorApiTrackingResponse.ResponseMessage = "Http Status Code : 500";
                    operatorApiTrackingResponse.ResponseBody = response;
                }

                
            }
            catch (System.Exception ex)
            {
                _logger.LogError("TurkTelekom Sms Status Failed | Exception : " + ex.ToString());
                operatorApiTrackingResponse.ResponseCode = "-99999";
                operatorApiTrackingResponse.ResponseMessage = ex.ToString();
                operatorApiTrackingResponse.ResponseBody = response;
            }

            return operatorApiTrackingResponse;
        }
    }
}
