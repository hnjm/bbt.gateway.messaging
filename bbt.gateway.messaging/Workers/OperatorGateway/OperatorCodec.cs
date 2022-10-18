using bbt.gateway.common.Models;
using bbt.gateway.messaging.Api.Codec.Model;
using bbt.gateway.messaging.Api.dEngage.Model.Transactional;
using CodecFastApi;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
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



        public async Task<CodecSmsStatusResponse> CheckSms(string refId)
        {
            var serializeSettings = new JsonSerializerSettings();
            serializeSettings.Converters.Add(new IsoDateTimeConverter(){ DateTimeFormat = "ddMMyyHHmmss" });
            try
            {
                var res = await _codecClient.GetStatusAsync(OperatorConfig.User, OperatorConfig.Password, refId, 3, String.Empty);
                return JsonConvert.DeserializeObject<CodecSmsStatusResponse>(res,serializeSettings);
            }
            catch (Exception ex)
            {
                TransactionManager.LogError($"Couldn't get Codec Sms Status  | Exception : {ex}");
                return null;
            }
            
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
                var nvc = new List<KeyValuePair<string, string>>();
                nvc.Add(new KeyValuePair<string, string>("username", OperatorConfig.User));
                nvc.Add(new KeyValuePair<string, string>("password", OperatorConfig.Password));
                nvc.Add(new KeyValuePair<string, string>("sender", GetSender()));
                nvc.Add(new KeyValuePair<string, string>("phone", phone.Concatenate()));
                nvc.Add(new KeyValuePair<string, string>("messageContent", content));
                nvc.Add(new KeyValuePair<string, string>("msgSpecialId", String.Empty));
                nvc.Add(new KeyValuePair<string, string>("isOtn", "false"));
                nvc.Add(new KeyValuePair<string, string>("headerCode", Configuration["Api:Codec:HeaderCode"]));
                nvc.Add(new KeyValuePair<string, string>("responseType", "3"));
                nvc.Add(new KeyValuePair<string, string>("optionalParameters", String.Empty));
                nvc.Add(new KeyValuePair<string, string>("iysBrandCode", String.Empty));
                nvc.Add(new KeyValuePair<string, string>("iysRecipientType", "BIREYSEL"));
                nvc.Add(new KeyValuePair<string, string>("iysMessageType", "BILGILENDIRME"));


                var client = new HttpClient();
                var req = new HttpRequestMessage(HttpMethod.Post, "https://fastsms-api.codec.com.tr/Soap.asmx/SendSms") { Content = new FormUrlEncodedContent(nvc) };
                var response = await client.SendAsync(req);
                var res = await response.Content.ReadAsStringAsync();
                ////var response = await _codecClient.SendSmsAsync(OperatorConfig.User, OperatorConfig.Password, GetSender(),
                //    phone.Concatenate(), content, string.Empty, false, Configuration["Api:Codec:HeaderCode"], 3, string.Empty, string.Empty,
                //    "BIREYSEL", "BILGILENDIRME");
                res = res.GetWithRegexSingle("(<string[^>]*>)(.*?)(</string>)", 2);
                var parsedResponse = JsonConvert.DeserializeObject<CodecSmsResponse>(res);

                smsLog.OperatorResponseCode = parsedResponse.ResultSet.Code;
                smsLog.OperatorResponseMessage = parsedResponse.ResultSet.Description;
                smsLog.StatusQueryId = parsedResponse.ResultList.FirstOrDefault()?.SmsRefId ?? String.Empty;
                smsLog.Status = String.Empty;
            }
            catch (Exception ex)
            {
                TransactionManager.LogError($"Critical Error Occured at Codec Services | ErrorCode:499 | Exception : {ex}");
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
