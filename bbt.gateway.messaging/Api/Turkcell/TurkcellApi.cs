using bbt.gateway.messaging.Api.Turkcell.Model;
using bbt.gateway.messaging.Models;
using bbt.gateway.messaging.Workers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace bbt.gateway.messaging.Api.Turkcell
{
    public class TurkcellApi:BaseApi
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<TurkcellApi> _logger;
        public TurkcellApi(IHttpClientFactory httpClientFactory
            ,ILogger<TurkcellApi> logger) {
            Type = OperatorType.Turkcell;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public async Task<TurkcellSmsResponse> SendSms(TurkcellSmsRequest turkcellSmsRequest) {

            try
            {
                HttpContent httpRequest = new StringContent(getSendSmsXml(turkcellSmsRequest), Encoding.UTF8, "text/xml");
                using var httpClient = _httpClientFactory.CreateClient();
                var httpResponse = await httpClient.PostAsync(OperatorConfig.SendService, httpRequest);
                var response = httpResponse.Content.ReadAsStringAsync().Result;
                response = response.Replace("&lt;", "<");
                response = response.Replace("&gt;", ">");
                
                if (httpResponse.IsSuccessStatusCode)
                {
                    var messageId = getBetween(response, "<MSGID>", "</MSGID>");
                    var responseMessage = getBetween(response, "<result xsi:type=\"xsd:string\">", "</result>");
                    if (string.IsNullOrEmpty(messageId))
                    {
                        return new TurkcellSmsResponse { ResultCode = responseMessage.Split(",")[1], ResultMessage = responseMessage, MsgId = "" , RequestBody = httpRequest.ReadAsStringAsync().Result, ResponseBody = response};
                    }
                    else
                    {
                        return new TurkcellSmsResponse { ResultCode = "0", ResultMessage = "", MsgId = messageId , RequestBody = httpRequest.ReadAsStringAsync().Result, ResponseBody = response };
                    }

                }
                else
                {
                    return new TurkcellSmsResponse { ResultCode = "-99999", ResultMessage = response, MsgId = "", RequestBody = httpRequest.ReadAsStringAsync().Result, ResponseBody = response };
                }
            }
            catch (System.Exception ex)
            {
                _logger.LogError("Turkcell Send Sms Failed | Exception : " + ex.ToString());
                return new TurkcellSmsResponse { ResultCode = "-99999", ResultMessage = ex.ToString(), MsgId = "" };
            }
            
        }

        public async Task<TurkcellAuthResponse> Auth(TurkcellAuthRequest turkcellAuthRequest)
        {
            try
            {
                HttpContent httpRequest = new StringContent(getAuthXml(turkcellAuthRequest), Encoding.UTF8, "text/xml");
                using var httpClient = _httpClientFactory.CreateClient();
                var httpResponse = await httpClient.PostAsync(OperatorConfig.AuthanticationService, httpRequest);
                var response = httpResponse.Content.ReadAsStringAsync().Result;
                if (httpResponse.IsSuccessStatusCode)
                {
                    var token = getBetween(response, "<result xsi:type=\"xsd:string\">", "</result>");
                    if (token.Contains("NOK"))
                    {
                        return new TurkcellAuthResponse { ResultCode = token.Split(",")[1], AuthToken = token };
                    }
                    else
                    {
                        return new TurkcellAuthResponse { ResultCode = "0", AuthToken = token };
                    }
                }
                else
                {
                    return new TurkcellAuthResponse { ResultCode = "-99999", AuthToken = "" };
                }
            }
            catch (System.Exception ex)
            {
                _logger.LogError("Turkcell Api Auth Failed | Exception : " + ex.ToString());
                return new TurkcellAuthResponse { ResultCode = "-99999", AuthToken = "" };
            }
            
                
        }

        public async Task<TurkcellSmsStatusResponse> CheckSmsStatus(TurkcellSmsStatusRequest turkcellSmsStatusRequest)
        {
            try
            {
                HttpContent httpRequest = new StringContent(getSmsStatusXml(turkcellSmsStatusRequest), Encoding.UTF8, "text/xml");
                using var httpClient = new HttpClient();
                var httpResponse = await httpClient.PostAsync(OperatorConfig.QueryService, httpRequest);
                var response = httpResponse.Content.ReadAsStringAsync().Result;
                response = response.Replace("&lt;", "<");
                response = response.Replace("&gt;", ">");
                if (httpResponse.IsSuccessStatusCode)
                {
                    var msgStat = getBetween(response, "<MSGSTAT>", "</MSGSTAT>");
                    var responseMessage = getBetween(response, "<result xsi:type=\"xsd:string\">", "</result>");
                    if (string.IsNullOrEmpty(msgStat))
                    {
                        var smsStatusResponse = new TurkcellSmsStatusResponse { ResultCode = responseMessage.Split(",")[1], ResultMessage = "" };
                        smsStatusResponse.SetFullResponse(responseMessage);
                        return smsStatusResponse;
                    }
                    else
                    {
                        var smsStatusResponse = new TurkcellSmsStatusResponse { ResultCode = msgStat, ResultMessage = "" };
                        smsStatusResponse.SetFullResponse(responseMessage);
                        return smsStatusResponse;
                    }

                }
                else
                {
                    var smsStatusResponse = new TurkcellSmsStatusResponse { ResultCode = "-99999", ResultMessage = "" };
                    smsStatusResponse.SetFullResponse(response);
                    return smsStatusResponse;
                }
            }
            catch (System.Exception ex)
            {
                _logger.LogError("Turkcell Check Sms Status Failed | Exception : " + ex.ToString());
                var smsStatusResponse = new TurkcellSmsStatusResponse { ResultCode = "-99999", ResultMessage = "Check Sms Status Failed" };
                smsStatusResponse.SetFullResponse(ex.ToString());
                return smsStatusResponse;
            }
        }

        private string getAuthXml(TurkcellAuthRequest turkcellAuthRequest)
        {
            string xml = $"<soapenv:Envelope xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:soapenv=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:reg=\"http://www.turkcell.com.tr/sms/webservices/register\">"
            + "<soapenv:Header/>"
            + "<soapenv:Body>"
            + "<reg:register soapenv:encodingStyle=\"http://schemas.xmlsoap.org/soap/encoding/\">"
            + "<string xsi:type=\"xsd:string\"><![CDATA[<?xml version=\"1.0\" encoding=\"UTF-8\"?>"
            + "<REGISTER>"
            + "<VERSION>1.0</VERSION>"
            + "<REG>"
            + "<USER>"+turkcellAuthRequest.User+"</USER>"
            + "<PASSWORD>"+turkcellAuthRequest.Password+"</PASSWORD>"
            + "</REG>"
            + "</REGISTER>]]>"
            + "</string>"
            + "</reg:register>"
            + "</soapenv:Body>"
            + "</soapenv:Envelope>";

            return xml;
        }

        private string getSendSmsXml(TurkcellSmsRequest turkcellSmsRequest)
        {
            string xml = "<soapenv:Envelope xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:soapenv=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:sen=\"http://www.turkcell.com.tr/sms/webservices/sendsms\">"
            + "<soapenv:Header/>"
            + "<soapenv:Body>"
            + "<sen:sendSMS soapenv:encodingStyle=\"http://schemas.xmlsoap.org/soap/encoding/\">"
            + "<string xsi:type=\"xsd:string\">"
            + "<![CDATA[<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>"
            + "<SENDSMS>"
            + "<VERSION>1.0</VERSION>"
            + "<SESSION_ID>"+turkcellSmsRequest.SessionId+"</SESSION_ID>"
            + "<MSG_CODE>{MSG_CODE}</MSG_CODE>"
            + "<VARIANT_ID>{VARIANT_ID}</VARIANT_ID>"
            + "<VP></VP>"
            + "<SRC_MSISDN></SRC_MSISDN>"
            + "<SENDER>"+turkcellSmsRequest.Header+"</SENDER>"
            + "<NOTIFICATION>T</NOTIFICATION>"
            + "<COMMERCIAL>N</COMMERCIAL>"
            + "<BRAND_CODE></BRAND_CODE>"
            + "<RECIPIENT_TYPE>BIREYSEL</RECIPIENT_TYPE>"
            + "<TM_LIST>"
            + "<TM>"
            + "<TRUSTED_DATE_LIST>"
            + "<TRUSTED_DATE>"+turkcellSmsRequest.TrustedDate+"</TRUSTED_DATE>"
            + "<TRUSTED_DATE_ALT>"+ turkcellSmsRequest.TrustedDate + "</TRUSTED_DATE_ALT>"
            + "</TRUSTED_DATE_LIST>"
            + "<DST_MSISDN_LIST>"
            + "<DST_MSISDN>"+ turkcellSmsRequest.PhoneNo+ "</DST_MSISDN>"
            + "</DST_MSISDN_LIST>"
            + "<CONTENT_LIST>"
            + "<CONTENT>"
            + "<CONTENT_TEXT>"+ turkcellSmsRequest.Content+ "</CONTENT_TEXT>"
            + "</CONTENT>"
            + "</CONTENT_LIST>"
            + "</TM>"
            + "</TM_LIST>"
            + "</SENDSMS>"
            + "]]>"
            + "</string>"
            + "</sen:sendSMS>"
            + "</soapenv:Body>"
            + "</soapenv:Envelope>";

            return xml;
        }

        private string getSmsStatusXml(TurkcellSmsStatusRequest turkcellSmsStatusRequest)
        {
            string xml = "<soapenv:Envelope xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:soapenv=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:get=\"http://www.turkcell.com.tr/sms/webservices/getstatus\">"
            + "<soapenv:Header/>"
            + "<soapenv:Body>"
            + "<get:getStatus soapenv:encodingStyle=\"http://schemas.xmlsoap.org/soap/encoding/\">"
            + "<string xsi:type=\"xsd:string\"><![CDATA[<GETSTATUS>"
            + "<VERSION>1.0</VERSION>"
            + "<SESSION_ID>"+turkcellSmsStatusRequest.AuthToken+"</SESSION_ID>"
            + "<MSGID_LIST>"
            + "<MSGID>"+turkcellSmsStatusRequest.MsgId+"</MSGID>"
            + "</MSGID_LIST>"
            + "</GETSTATUS>"
            + "]]>"
            + "</string>"
            + "</get:getStatus>"
            + "</soapenv:Body>"
            + "</soapenv:Envelope>";

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
