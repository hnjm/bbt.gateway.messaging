﻿using bbt.gateway.messaging.Api.Vodafone.Model;
using bbt.gateway.common.Models;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace bbt.gateway.messaging.Api.Vodafone
{
    public class VodafoneApi:BaseApi
    {
        private readonly ILogger<VodafoneApi> _logger;
        private readonly HttpClient _httpClient;

        public VodafoneApi(ILogger<VodafoneApi> logger) 
        {
            _logger = logger;
            var httpClientHandler = new HttpClientHandler();
            httpClientHandler.UseProxy = false;
            _httpClient = new(httpClientHandler);
            Type = OperatorType.Vodafone;
        }

        public async Task<OperatorApiResponse> SendSms(VodafoneSmsRequest vodafoneSmsRequest)
        {
            OperatorApiResponse vodafoneSmsResponse = new(){ OperatorType = this.Type };
            string response = "";
            var requests = getSendSmsXml(vodafoneSmsRequest);
            try
            {
                var httpRequest = new StringContent(requests.Item1, Encoding.UTF8, "application/soap+xml");
                var httpResponse = await _httpClient.PostAsync(OperatorConfig.SendService, httpRequest);
                response = httpResponse.Content.ReadAsStringAsync().Result;
                
                if (httpResponse.IsSuccessStatusCode)
                {
                    var parsedXml = response.DeserializeXml<Model.SendSms.SuccessXml.Envelope>();
                    vodafoneSmsResponse.ResponseCode = parsedXml.Body.sendSMSPacketResponse.@return.deliveryResponseList.deliveryResponse.deliveryInfoList.deliveryInfo.errorCode.ToString();
                    vodafoneSmsResponse.ResponseMessage = "";
                    vodafoneSmsResponse.MessageId = parsedXml.Body.sendSMSPacketResponse.@return.deliveryResponseList.deliveryResponse.messageId;
                    vodafoneSmsResponse.ResponseBody = response;
                    vodafoneSmsResponse.RequestBody = requests.Item2;
                }
                else
                {
                    var parsedXml = response.DeserializeXml<Model.SendSms.ErrorXml.Envelope>();
                    vodafoneSmsResponse.ResponseCode = parsedXml.Body.Fault.Code.Value.Split(":")[1];
                    vodafoneSmsResponse.ResponseMessage = parsedXml.Body.Fault.Reason.Text.Value;
                    vodafoneSmsResponse.MessageId = "";
                    vodafoneSmsResponse.ResponseBody = response;
                    vodafoneSmsResponse.RequestBody = requests.Item2;
                    
                }
            }
            catch (System.Exception ex)
            {
                _logger.LogError("Vodafone Send Sms Failed | Exception : " + ex.ToString());
                vodafoneSmsResponse.ResponseCode = "-99999";
                vodafoneSmsResponse.ResponseBody = ex.ToString();
                vodafoneSmsResponse.MessageId = "";
                vodafoneSmsResponse.ResponseBody = response;
                vodafoneSmsResponse.RequestBody = requests.Item2;
            }

            return vodafoneSmsResponse;
        }

        public async Task<OperatorApiTrackingResponse> CheckSmsStatus(VodafoneSmsStatusRequest vodafoneSmsStatusRequest)
        {
            OperatorApiTrackingResponse vodafoneSmsStatusResponse = new(){ OperatorType = this.Type };
            string response = "";
            try
            {
                var xmlBody = new StringContent(getSmsStatusXml(vodafoneSmsStatusRequest), Encoding.UTF8, "application/soap+xml");
                var httpResponse = await _httpClient.PostAsync(OperatorConfig.QueryService, xmlBody);
                response = httpResponse.Content.ReadAsStringAsync().Result;

                if (httpResponse.IsSuccessStatusCode)
                {
                    var parsedXml = response.DeserializeXml<Model.SmsStatus.SuccessXml.Envelope>();

                    if (parsedXml.Body.queryPacketStatusResponse.@return.result == 1)
                    {
                        vodafoneSmsStatusResponse.ResponseCode = parsedXml.Body.queryPacketStatusResponse.@return.deliveryStatusList.deliveryStatus.status.ToString();
                        vodafoneSmsStatusResponse.ResponseMessage = "";
                        vodafoneSmsStatusResponse.ResponseBody = response;
                    }
                    else
                    {
                        vodafoneSmsStatusResponse.ResponseCode = parsedXml.Body.queryPacketStatusResponse.@return.errorCode.ToString();
                        vodafoneSmsStatusResponse.ResponseMessage = parsedXml.Body.queryPacketStatusResponse.@return.description.ToString();
                        vodafoneSmsStatusResponse.ResponseBody = response;
                    }
                }
                else
                {
                    var parsedXml = response.DeserializeXml<Model.SmsStatus.ErrorXml.Envelope>();
                    vodafoneSmsStatusResponse.ResponseCode = parsedXml.Body.Fault.Code.Value.Split(":")[1];
                    vodafoneSmsStatusResponse.ResponseMessage = parsedXml.Body.Fault.Reason.Text.Value;
                    vodafoneSmsStatusResponse.ResponseBody = response;

                }
            }
            catch (System.Exception ex)
            {
                _logger.LogError("Vodafone Sms Status Failed | Exception : " + ex.ToString());
                vodafoneSmsStatusResponse.ResponseCode = "-99999";
                vodafoneSmsStatusResponse.ResponseMessage = ex.ToString();
                vodafoneSmsStatusResponse.ResponseBody = response;

            }

            return vodafoneSmsStatusResponse;
        }

        public async Task<OperatorApiAuthResponse> Auth(VodafoneAuthRequest vodafoneAuthRequest)
        {
            OperatorApiAuthResponse vodafoneAuthResponse = new();
            try
            {
                var xmlBody = new StringContent(getAuthXml(vodafoneAuthRequest), Encoding.UTF8, "application/soap+xml");
                var httpResponse = await _httpClient.PostAsync(OperatorConfig.AuthanticationService, xmlBody);
                var response = httpResponse.Content.ReadAsStringAsync().Result;
                
                if (httpResponse.IsSuccessStatusCode)
                {
                    var parsedResponse = response.DeserializeXml<Model.Auth.SuccessXml.Envelope>();
                    vodafoneAuthResponse.ResponseCode = "0";
                    vodafoneAuthResponse.ResponseMessage = "";
                    vodafoneAuthResponse.AuthToken = parsedResponse.Body.authenticateResponse.@return.sessionId;
                }
                else
                {
                    var parsedResponse = response.DeserializeXml<Model.Auth.ErrorXml.Envelope>();
                    vodafoneAuthResponse.ResponseCode = "-99999";
                    vodafoneAuthResponse.ResponseMessage = parsedResponse.Body.Fault.Reason.Text.Value;
                    vodafoneAuthResponse.AuthToken = "";
                }
            }
            catch (System.Exception ex)
            {

                _logger.LogError("Vodafone Auth Failed | Exception : " + ex.ToString());
                vodafoneAuthResponse.ResponseCode = "-99999";
                vodafoneAuthResponse.ResponseMessage = ex.ToString();
                vodafoneAuthResponse.AuthToken = "";
            }

            return vodafoneAuthResponse;
           
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

        private (string,string) getSendSmsXml(VodafoneSmsRequest vodafoneSmsRequest)
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

            string maskedXml = "<soap:Envelope xmlns:soap=\"http://www.w3.org/2003/05/soap-envelope\" xmlns:mes=\"http://messaging.packet.services.system.sdf.oksijen.com\">"
            + "<soap:Header>"
            + "<Authentication:usernamePassword Authentication:user=\"" + vodafoneSmsRequest.User + "\""
            + " Authentication:sessionid=\"" + vodafoneSmsRequest.AuthToken + "\" Authentication:serviceid=\"OTP\" xmlns:Authentication=\"Authentication\"/>"
            + "</soap:Header>"
            + "<soap:Body>"
            + "<mes:sendSMSPacket>"
            + "<smsPacket>"
            + "<sms>"
            + "<destinationList>"
            + "<destination>"
            + "<subscriberId>" + vodafoneSmsRequest.PhoneNo + "</subscriberId>"
            + "</destination>"
            + "</destinationList>"
            + "<message>" + vodafoneSmsRequest.Message.MaskOtpContent() + "</message>"
            + "<smsParameters>"
            + "<sender>" + vodafoneSmsRequest.Header + "</sender>"
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
            + "<value>" + vodafoneSmsRequest.ControlHour + "</value>"
            + "</attribute>"
            + "<attribute>"
            + "<key>OTP_EXPIRY_PERIOD</key>"
            + "<value>" + vodafoneSmsRequest.ExpiryPeriod + "</value>"
            + "</attribute>"
            + "<attribute>"
            + "<key>MNP_PERIOD</key>"
            + "<value>" + vodafoneSmsRequest.ControlHour + "</value>"
            + "</attribute>"
            + "</serviceAttributes>"
            + "</mes:sendSMSPacket>"
            + "</soap:Body>"
            + "</soap:Envelope>";

            return (xml,maskedXml);
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
        
    }
}
