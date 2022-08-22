using bbt.gateway.common.Models;
using bbt.gateway.messaging.Api.Codec.Model;
using bbt.gateway.messaging.Api.dEngage.Model.Transactional;
using CodecFastApi;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace bbt.gateway.messaging.Workers.OperatorGateway
{
    public class OperatorCodec : OperatorGatewayBase
    {

        private SoapSoapClient _codecClient;
        public OperatorCodec(IConfiguration configuration,
            ITransactionManager transactionManager) : base(configuration, transactionManager)
        {
            _codecClient = new SoapSoapClient(SoapSoapClient.EndpointConfiguration.SoapSoap12);
        }



        public async Task<SmsStatusResponse> CheckSms(string queryId)
        {
            await Task.CompletedTask;
            return new SmsStatusResponse();
        }


        public async Task<SmsResponseLog> SendSms(Phone phone, string content)
        {
            var smsLog = new SmsResponseLog()
            {
                Operator = Type,
                Content = String.IsNullOrEmpty(content) ? "" : content.ClearMaskingFields(),
                CreatedAt = DateTime.Now,
            };

            try
            {
                var response = await _codecClient.SendSmsAsync(OperatorConfig.User, OperatorConfig.Password, GetSender(),
                    phone.Concatenate(), content, string.Empty, false, Configuration["Api:Codec:HeaderCode"], 3, string.Empty, string.Empty,
                    "BIREYSEL", "BILGILENDIRME");

                var parsedResponse = JsonConvert.DeserializeObject<CodecSmsResponse>(response);

                smsLog.OperatorResponseCode = parsedResponse.ResultSet.Code;
                smsLog.OperatorResponseMessage = parsedResponse.ResultSet.Description;
                smsLog.StatusQueryId = parsedResponse.ResultList.FirstOrDefault()?.SmsRefId ?? String.Empty;
                smsLog.Status = String.Empty;
            }
            catch (Exception ex)
            {
                TransactionManager.LogError($"Critical Error Occured at Codec Services | ErrorCode:499 | Exception : {ex.ToString()}");
                smsLog.OperatorResponseCode = -99999;
                smsLog.OperatorResponseMessage = ex.ToString();
            }

            return smsLog;
        }

        private string GetSender()
        {
            return TransactionManager.CustomerRequestInfo.BusinessLine == "X" ? "On." : "BURGAN";
        }
    }
}
