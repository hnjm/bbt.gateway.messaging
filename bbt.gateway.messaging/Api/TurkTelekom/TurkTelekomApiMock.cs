using bbt.gateway.messaging.Api.TurkTelekom.Model;
using bbt.gateway.common.Models;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using bbt.gateway.messaging.Workers;
using System;

namespace bbt.gateway.messaging.Api.TurkTelekom
{
    public class TurkTelekomApiMock:BaseApi,ITurkTelekomApi
    {
        public TurkTelekomApiMock(ITransactionManager transactionManager):base(transactionManager) {
            Type = OperatorType.TurkTelekom;
        }

        public async Task<OperatorApiResponse> SendSms(TurkTelekomSmsRequest turkTelekomSmsRequest) 
        {
            await Task.CompletedTask;
            OperatorApiResponse operatorApiResponse = new() { OperatorType = this.Type };
            
            operatorApiResponse.ResponseCode = "0";
            operatorApiResponse.ResponseMessage = "";
            operatorApiResponse.MessageId = Guid.NewGuid().ToString();
            operatorApiResponse.RequestBody = turkTelekomSmsRequest.SerializeXml();
            operatorApiResponse.ResponseBody = "<Mock>Successfull</Mock>";

            return operatorApiResponse;
        }

        public async Task<OperatorApiTrackingResponse> CheckSmsStatus(TurkTelekomSmsStatusRequest turkTelekomSmsStatusRequest)
        {
            await Task.CompletedTask;
            OperatorApiTrackingResponse operatorApiTrackingResponse = new() { OperatorType = this.Type };
           
            var requestBody = turkTelekomSmsStatusRequest.SerializeXml();
            var httpRequest = new StringContent(requestBody, Encoding.UTF8, "text/xml");
                
            operatorApiTrackingResponse.ResponseCode = "0";
            operatorApiTrackingResponse.ResponseMessage = "";
            operatorApiTrackingResponse.ResponseBody = "<Mock>Successfull</Mock>";
                

            return operatorApiTrackingResponse;
        }
    }
}
