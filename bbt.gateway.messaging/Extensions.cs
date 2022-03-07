using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using bbt.gateway.messaging.Api;
using bbt.gateway.common.Models;
using System.Text.RegularExpressions;

namespace bbt.gateway.messaging
{


    public static class Extensions
    {
       
        public static string Mask(this string str, string pattern, MatchEvaluator match)
        {
            return Regex.Replace(str, pattern, match);
        }

        public static string MaskOtpContent(this string content)
        {
            return content.Mask("[0-9]{6}", m =>
            {
                return $"{m.Value.Substring(0, 1)}{new string('*', 5)}";

            });
        }

        public static SendSmsResponseStatus UnifyResponse(this ICollection<OtpResponseLog> logs)
        {
            if (logs.Any(l => l.ResponseCode == SendSmsResponseStatus.Success))
                return SendSmsResponseStatus.Success;

            if (logs.Any(l => l.ResponseCode == SendSmsResponseStatus.OperatorChange))
                return SendSmsResponseStatus.OperatorChange;

            if (logs.Any(l => l.ResponseCode == SendSmsResponseStatus.SimChange))
                return SendSmsResponseStatus.SimChange;

            if (logs.All(l => l.ResponseCode == SendSmsResponseStatus.NotSubscriber))
                return SendSmsResponseStatus.NotSubscriber;

            return SendSmsResponseStatus.ClientError;
        }

        public static string BuildContentForSms(this Header header, string content)
        {
            string maskedContent = content.MaskOtpContent();
            return $"{header.SmsPrefix} {maskedContent} {header.SmsSuffix}";
        }

        public static string BuildContentForLog(this Header header, string content)
        {
            return $"[{header.SmsSender}]{header.SmsPrefix} {content} {header.SmsSuffix}";
        }

        public static OtpResponseLog BuildOperatorApiResponse(this OperatorApiResponse apiResponse)
        {
            var response = new OtpResponseLog
            {
                Operator = apiResponse.GetOperatorType(),
                Topic = $"{apiResponse.GetOperatorType()} otp sending",
                TrackingStatus = SmsTrackingStatus.Pending,
                RequestBody = apiResponse.GetRequestBody(),
                ResponseBody = apiResponse.GetResponseBody()
            };
            
            response.StatusQueryId = apiResponse.GetMessageId();
            if (Constant.OperatorErrorCodes.ContainsKey(apiResponse.GetOperatorType()))
            {
                var errorCodes = Constant.OperatorErrorCodes[apiResponse.GetOperatorType()];
                if (errorCodes.ContainsKey(apiResponse.GetResponseCode().Trim()))
                {
                    response.ResponseCode = errorCodes[apiResponse.GetResponseCode()].SmsResponseStatus;
                    if (string.IsNullOrEmpty(apiResponse.GetResponseMessage()))
                        response.ResponseMessage = errorCodes[apiResponse.GetResponseCode()].ReturnMessage;
                }
                else 
                {
                    response.ResponseCode = SendSmsResponseStatus.ClientError;
                    response.ResponseMessage = $"Given Error Code Not Exist In Dictionary | Operator Type : {apiResponse.GetOperatorType()} | Error Code : {apiResponse.GetResponseCode()}";
                }
            }
            else 
            {
                response.ResponseCode = SendSmsResponseStatus.ClientError;
                response.ResponseMessage =  $"Given Operator Type Not Exist In Dictionary | Operator Type : {apiResponse.GetOperatorType()}";
            }

            return response;
        }

        public static OtpTrackingLog BuildOperatorApiTrackingResponse(this OperatorApiTrackingResponse apiTrackingResponse,CheckSmsRequest checkSmsRequest)
        {
            var otpTrackingLog = new OtpTrackingLog();
            otpTrackingLog.LogId = checkSmsRequest.OtpRequestLogId;
            otpTrackingLog.Detail = apiTrackingResponse.GetFullResponse();
            if (Constant.OperatorTrackingErrorCodes.ContainsKey(apiTrackingResponse.GetOperatorType()))
            {
                var errorCodes = Constant.OperatorTrackingErrorCodes[apiTrackingResponse.GetOperatorType()];
                if (errorCodes.ContainsKey(apiTrackingResponse.GetResponseCode().Trim()))
                {
                    otpTrackingLog.Status = errorCodes[apiTrackingResponse.GetResponseCode()].SmsTrackingStatus;
                    if (string.IsNullOrEmpty(apiTrackingResponse.GetResponseMessage()))
                        otpTrackingLog.ResponseMessage = errorCodes[apiTrackingResponse.GetResponseCode()].ReturnMessage;
                }
                else
                {
                    otpTrackingLog.Status = SmsTrackingStatus.SystemError;
                    otpTrackingLog.ResponseMessage = $"Given Error Code Not Exist In Dictionary | Operator Type : {apiTrackingResponse.GetOperatorType()} | Error Code : {apiTrackingResponse.GetResponseCode()}";
                }
            }
            else
            {
                    otpTrackingLog.Status = SmsTrackingStatus.SystemError;
                    otpTrackingLog.ResponseMessage = $"Given Operator Type Not Exist In Dictionary | Operator Type : {apiTrackingResponse.GetOperatorType()}";
            }
            return otpTrackingLog;
        }


        public static T DeserializeXml<T>(this string toDeserialize)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));
          
            using (StringReader textReader = new StringReader(toDeserialize))
            {
                return (T)xmlSerializer.Deserialize(textReader);
            }
        }

        public static string SerializeXml<T>(this T toSerialize)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));
            var xmlnsEmpty = new XmlSerializerNamespaces();
            xmlnsEmpty.Add("", "");
            using (StringWriter textWriter = new StringWriter())
            {
                xmlSerializer.Serialize(textWriter, toSerialize, xmlnsEmpty);
                return textWriter.ToString();
            }
        }

        public static T SoapDeserializeXml<T>(this string toDeserialize)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));

            using (StringReader textReader = new StringReader(toDeserialize))
            {
                return (T)xmlSerializer.Deserialize(textReader);
            }
        }

        public static string SoapSerializeXml<T>(this T toSerialize)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));
            var xmlnsEmpty = new XmlSerializerNamespaces();
            xmlnsEmpty.Add("env", "http://schemas.xmlsoap.org/soap/encoding/");
            xmlnsEmpty.Add("m", "http://www.turkcell.com.tr/sms/webservices/register");
            using (StringWriter textWriter = new StringWriter())
            {
                xmlSerializer.Serialize(textWriter, toSerialize,xmlnsEmpty);
                return textWriter.ToString();
            }
        }

    };

    
}