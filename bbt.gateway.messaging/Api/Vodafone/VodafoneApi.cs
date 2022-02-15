using bbt.gateway.messaging.Api.Vodafone.Model;
using bbt.gateway.messaging.Models;
using bbt.gateway.messaging.Workers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace bbt.gateway.messaging.Api.Vodafone
{
    public class VodafoneApi:BaseApi
    {
        private readonly ILogger<VodafoneApi> _logger;
        private readonly IHttpClientFactory _httpClientFactory;

        public VodafoneApi(IHttpClientFactory httpClientFactory,
            ILogger<VodafoneApi> logger) 
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            Type = OperatorType.Vodafone;
        }

        public async Task<VodafoneSmsResponse> SendSms(VodafoneSmsRequest vodafoneSmsRequest)
        {
            try
            {
                var httpRequest = new StringContent(getSendSmsXml(vodafoneSmsRequest), Encoding.UTF8, "application/soap+xml");
                using var httpClient = _httpClientFactory.CreateClient();
                var httpResponse = await httpClient.PostAsync(OperatorConfig.SendService, httpRequest);
                var response = httpResponse.Content.ReadAsStringAsync().Result;

                if (httpResponse.IsSuccessStatusCode)
                {
                    var deliveryResponse = getBetween(response, "<deliveryResponse>", "</deliveryResponse>");
                    var errorCode = getBetween(deliveryResponse, "<errorCode>", "</errorCode>");
                    var messageId = getBetween(deliveryResponse, "<messageId>", "</messageId>");
                    if (string.IsNullOrEmpty(deliveryResponse))
                    {
                        var faultCodeText = getBetween(response, "<soapenv:Code>", "</soapenv:Code>");
                        var faultReasonText = getBetween(response, "<soapenv:Reason>", "</soapenv:Reason>");
                        var faultCode = string.IsNullOrEmpty(faultCodeText) ? "" : getBetween(faultCodeText, "<soapenv:Value>", "</soapenv:Value>");

                        return new VodafoneSmsResponse { ResultCode = faultCode.Split(":")[1], ResultMessage = faultReasonText, MessageId = "" };
                    }
                    else
                    {

                        return new VodafoneSmsResponse { ResultCode = errorCode, ResultMessage = "", MessageId = messageId, 
                            RequestBody = httpRequest.ReadAsStringAsync().Result, ResponseBody = response };
                    }
                }
                else
                {
                    var faultCodeText = getBetween(response, "<soapenv:Code>", "</soapenv:Code>");
                    var faultReasonText = getBetween(response, "<soapenv:Reason>", "</soapenv:Reason>");
                    var faultCode = string.IsNullOrEmpty(faultCodeText) ? "" : getBetween(faultCodeText, "<soapenv:Value>", "</soapenv:Value>");

                    return new VodafoneSmsResponse { ResultCode = string.IsNullOrEmpty(faultCode) ? "-99999" : faultCode.Split(":")[1],
                        ResultMessage = response, MessageId = "", RequestBody = httpRequest.ReadAsStringAsync().Result,
                        ResponseBody = response
                    };
                }
            }
            catch (System.Exception ex)
            {
                _logger.LogError("Vodafone Send Sms Failed | Exception : " + ex.ToString());
                return new VodafoneSmsResponse { ResultCode = "-99999", ResultMessage = ex.ToString(), MessageId = "" };
            }

            
        }

        public async Task<VodafoneSmsStatusResponse> CheckSmsStatus(VodafoneSmsStatusRequest vodafoneSmsStatusRequest)
        {
            string response = "";
            try
            {
                var xmlBody = new StringContent(getSmsStatusXml(vodafoneSmsStatusRequest), Encoding.UTF8, "application/soap+xml");
                using var httpClient = new HttpClient();
                var httpResponse = await httpClient.PostAsync(OperatorConfig.QueryService, xmlBody);
                response = httpResponse.Content.ReadAsStringAsync().Result;

                if (httpResponse.IsSuccessStatusCode)
                {
                    var status = getBetween(response, "<status>", "</status>");
                    if (string.IsNullOrEmpty(status))
                    {
                        var faultCodeText = getBetween(response, "<errorCode>", "</errorCode>");
                        var faultReasonText = getBetween(response, "<description>", "</description>");

                        var smsStatusResponse =  new VodafoneSmsStatusResponse { ResponseCode = faultCodeText, ResponseMessage = faultReasonText };
                        smsStatusResponse.SetFullResponse(response);
                        return smsStatusResponse;
                    }
                    else
                    {

                        var smsStatusResponse = new VodafoneSmsStatusResponse { ResponseCode = status, ResponseMessage = "" };
                        smsStatusResponse.SetFullResponse(response);
                        return smsStatusResponse;
                    }
                }
                else
                {
                    var faultCodeText = getBetween(response, "<soapenv:Code>", "</soapenv:Code>");
                    var faultReasonText = getBetween(response, "<soapenv:Reason>", "</soapenv:Reason>");
                    var faultCode = string.IsNullOrEmpty(faultCodeText) ? "" : getBetween(faultCodeText, "<soapenv:Value>", "</soapenv:Value>");

                    var smsStatusResponse =  new VodafoneSmsStatusResponse
                    {
                        ResponseCode = string.IsNullOrEmpty(faultCode) ? "-99999" : faultCode.Split(":")[1],
                        ResponseMessage = response
                    };
                    smsStatusResponse.SetFullResponse(response);
                    return smsStatusResponse;
                }
            }
            catch (System.Exception ex)
            {
                _logger.LogError("Vodafone Sms Status Failed | Exception : " + ex.ToString());
                var smsStatusResponse = new VodafoneSmsStatusResponse { ResponseCode = "-99999", ResponseMessage = ex.ToString() };
                smsStatusResponse.SetFullResponse(ex.ToString());
                return smsStatusResponse;
            }
        }

        public async Task<VodafoneAuthResponse> Auth(VodafoneAuthRequest vodafoneAuthRequest)
        {
            try
            {

                var xmlBody = new StringContent(getAuthXml(vodafoneAuthRequest), Encoding.UTF8, "application/soap+xml");
                using var httpClient = _httpClientFactory.CreateClient();
                var httpResponse = await httpClient.PostAsync(OperatorConfig.AuthanticationService, xmlBody);
                var response = httpResponse.Content.ReadAsStringAsync().Result;

                if (httpResponse.IsSuccessStatusCode)
                {
                    var token = getBetween(response, "<sessionId>", "</sessionId>");
                    var faultCodeText = getBetween(response, "<soapenv:Code>", "</soapenv:Code>");
                    var faultReasonText = getBetween(response, "<soapenv:Reason>", "</soapenv:Reason>");
                    var faultCode = string.IsNullOrEmpty(faultCodeText) ? "" : getBetween(faultCodeText,"<soapenv:Value>", "</soapenv:Value>");
                    if (string.IsNullOrEmpty(token))
                    {
                        return new VodafoneAuthResponse { ResultCode = faultCode.Split(":")[1], AuthToken = faultReasonText };
                    }
                    else
                    {
                        return new VodafoneAuthResponse { ResultCode = "0", AuthToken = token };
                    }
                }
                else
                {
                    return new VodafoneAuthResponse { ResultCode = "-99999", AuthToken = "" };
                }
            }
            catch (System.Exception ex)
            {

                _logger.LogError("Vodafone Auth Failed | Exception : " + ex.ToString());
                return new VodafoneAuthResponse { ResultCode = "-99999", AuthToken = "" };
            }
           
        }

        private string getAuthXml(VodafoneAuthRequest vodafoneAuthRequest)
        {
            string xml = "<soap:Envelope xmlns:soap=\"http://www.w3.org/2003/05/soap-envelope\" xmlns:aut=\"http://authentication.services.system.sdf.oksijen.com\">"
            + "<soap:Header>"
            + "<Authentication:usernamePassword xmlns:Authentication=\"Authentication\" Authentication:user=\""+vodafoneAuthRequest.User+"\" Authentication:password=\""+vodafoneAuthRequest.Password+"\"/>    </soap:Header>"
            + "<soap:Body>"
            + "<aut:authenticate/>"
            + "</soap:Body>"
            + "</soap:Envelope>";

            return xml;
        }

        private string getSendSmsXml(VodafoneSmsRequest vodafoneSmsRequest)
        {
            string xml = "<soap:Envelope xmlns:soap=\"http://www.w3.org/2003/05/soap-envelope\" xmlns:mes=\"http://messaging.packet.services.system.sdf.oksijen.com\">"
            + "<soap:Header>"
            + "<Authentication:usernamePassword Authentication:user=\""+vodafoneSmsRequest.User+"\""
            + " Authentication:sessionid=\""+vodafoneSmsRequest.AuthToken+"\" Authentication:serviceid=\"OTP\" xmlns:Authentication=\"Authentication\"/>"
            + "</soap:Header>"
            + "<soap:Body>"
            + "<mes:sendSMSPacket>"
            + "<smsPacket>"
            + "<sms>"
            + "<destinationList>"
            + "<destination>"
            + "<subscriberId>"+vodafoneSmsRequest.PhoneNo+"</subscriberId>"
            + "</destination>"
            + "</destinationList>"
            + "<message>"+vodafoneSmsRequest.Message+"</message>"
            + "<smsParameters>"
            + "<sender>"+vodafoneSmsRequest.Header+"</sender>"
            + "<shortCode xsi:nil=\"true\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"/>"
            + "<sourceMsisdn xsi:nil=\"true\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"/>                  <startDate xsi:nil=\"true\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"/>               </smsParameters>"
            + "<unifier xsi:nil=\"true\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"/>            </sms>"
            + "</smsPacket>"
            + "<traceId xsi:nil=\"true\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"/>"
            + "<correlator xsi:nil=\"true\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"/>"
            + "<notificationURL xsi:nil=\"true\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"/>         <serviceAttributes>"
            + "<attribute>"
            + "<key>SKIP_ALL_CONTROLS</key>"
            + "<value>false</value>"
            + "</attribute>"
            + "<attribute>"
            + "<key>CHECK_MNP_STATUS</key>"
            + "<value>true</value>"
            + "</attribute>"
            + "<attribute>"
            + "<key>CHECK_SIMCARD_STATUS</key>"
            + "<value>true</value>"
            + "</attribute>"
            + "<attribute>"
            + "<key>NOTIFICATION_SMS_REQUESTED</key>"
            + "<value>true</value>"
            + "</attribute>"
            + "<attribute>"
            + "<key>SENDABLE_TO_OFFNET</key>"
            + "<value>false</value>"
            + "</attribute>"
            + "<attribute>"
            + "<key>IS_ENCRYPTED</key>"
            + "<value>false</value>"
            + "</attribute>"
            + "<attribute>"
            + "<key>SIMCARD_CHANGE_PERIOD</key>"
            + "<value>"+vodafoneSmsRequest.ControlHour+"</value>"
            + "</attribute>"
            + "<attribute>"
            + "<key>OTP_EXPIRY_PERIOD</key>"
            + "<value>"+vodafoneSmsRequest.ExpiryPeriod+"</value>"
            + "</attribute>"
            + "<attribute>"
            + "<key>MNP_PERIOD</key>"
            + "<value>"+vodafoneSmsRequest.ControlHour+"</value>"
            + "</attribute>"
            + "</serviceAttributes>"
            + "</mes:sendSMSPacket>"
            + "</soap:Body>"
            + "</soap:Envelope>";

            return xml;
        }

        private string getSmsStatusXml(VodafoneSmsStatusRequest vodafoneSmsStatusRequest)
        {
            string xml = "<soap:Envelope xmlns:soap=\"http://www.w3.org/2003/05/soap-envelope\" xmlns:mes=\"http://messaging.packet.services.system.sdf.oksijen.com\">"
            + "<soap:Header>"
            + "<Authentication:usernamePassword Authentication:user=\""+ vodafoneSmsStatusRequest.User+ "\""
            + " Authentication:sessionid=\""+vodafoneSmsStatusRequest.AuthToken+"\" Authentication:serviceid=\"OTP\" xmlns:Authentication=\"Authentication\"/>"
            + "</soap:Header>"
            + "<soap:Body>"
            + "<mes:queryPacketStatus>"
            + "<packetId>" + vodafoneSmsStatusRequest.MessageId.Substring(0, vodafoneSmsStatusRequest.MessageId.Length-13) + "</packetId>"
            + "<messageId>" + vodafoneSmsStatusRequest.MessageId + "</messageId>"
            + "<transactionIdList>"
            + "<transactionId>" + vodafoneSmsStatusRequest.MessageId + "</transactionId>"
            + "</transactionIdList>"
            + "</mes:queryPacketStatus>"
            + "</soap:Body>"
            + "</soap:Envelope>";

            return xml;
        }
        private string getBetween(string strSource, string strStart, string strEnd)
        {
            if (strSource.Contains(strStart) && strSource.Contains(strEnd))
            {
                int Start, End;
                Start = strSource.IndexOf(strStart, 0) + strStart.Length;
                End = strSource.IndexOf(strEnd, Start);
                return strSource.Substring(Start, End - Start);
            }

            return "";
        }
    }
}
