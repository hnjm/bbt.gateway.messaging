using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using bbt.gateway.messaging.Api;
using bbt.gateway.common.Models;
using System.Text.RegularExpressions;
using System;

namespace bbt.gateway.messaging
{


    public static class Extensions
    {
        public static string ConvertToTurkish(this string str)
        {
            char[] turkishCharacters = {'ç','Ç','ğ','Ğ','ı','İ','Ö','ö','Ü','ü','ş','Ş'};            
            char[] englishCharacters = {'c','C','g','G','i','I','O','o','U','u','s','S'};
            
            for (int i = 0; i < turkishCharacters.Length; i++) str = str.Replace(turkishCharacters[i], englishCharacters[i]);
            return str;

        }

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
            return $"{header.SmsPrefix} {content} {header.SmsSuffix}";
        }

        public static string BuildContentForLog(this Header header, string content)
        {
            string maskedContent = content.MaskOtpContent();
            return $"[{header.SmsSender}]{header.SmsPrefix} {maskedContent} {header.SmsSuffix}";
        }

        public static OtpResponseLog BuildOperatorApiResponse(this OperatorApiResponse apiResponse)
        {
            var response = new OtpResponseLog
            {
                Operator = apiResponse.OperatorType,
                Topic = $"{apiResponse.OperatorType} otp sending",
                TrackingStatus = SmsTrackingStatus.Pending,
                RequestBody = apiResponse.RequestBody,
                ResponseBody = apiResponse.ResponseBody
            };
            
            response.StatusQueryId = apiResponse.MessageId;
            if (Constant.OperatorErrorCodes.ContainsKey(apiResponse.OperatorType))
            {
                var errorCodes = Constant.OperatorErrorCodes[apiResponse.OperatorType];
                if (errorCodes.ContainsKey(apiResponse.ResponseCode.Trim()))
                {
                    response.ResponseCode = errorCodes[apiResponse.ResponseCode].SmsResponseStatus;
                    if (string.IsNullOrEmpty(apiResponse.ResponseMessage))
                        response.ResponseMessage = errorCodes[apiResponse.ResponseCode].ReturnMessage;
                    else
                        response.ResponseMessage = apiResponse.ResponseMessage;
                }
                else 
                {
                    response.ResponseCode = SendSmsResponseStatus.ClientError;
                    response.ResponseMessage = $"Given Error Code Not Exist In Dictionary | Operator Type : {apiResponse.OperatorType} | Error Code : {apiResponse.ResponseCode}";
                }
            }
            else 
            {
                response.ResponseCode = SendSmsResponseStatus.ClientError;
                response.ResponseMessage =  $"Given Operator Type Not Exist In Dictionary | Operator Type : {apiResponse.OperatorType}";
            }

            return response;
        }

        public static OtpTrackingLog BuildOperatorApiTrackingResponse(this OperatorApiTrackingResponse apiTrackingResponse,CheckSmsRequest checkSmsRequest)
        {
            var otpTrackingLog = new OtpTrackingLog();
            otpTrackingLog.LogId = checkSmsRequest.OtpRequestLogId;
            otpTrackingLog.Detail = apiTrackingResponse.ResponseBody;
            if (Constant.OperatorTrackingErrorCodes.ContainsKey(apiTrackingResponse.OperatorType))
            {
                var errorCodes = Constant.OperatorTrackingErrorCodes[apiTrackingResponse.OperatorType];
                if (errorCodes.ContainsKey(apiTrackingResponse.ResponseCode.Trim()))
                {
                    otpTrackingLog.Status = errorCodes[apiTrackingResponse.ResponseCode].SmsTrackingStatus;
                    if (string.IsNullOrEmpty(apiTrackingResponse.ResponseMessage))
                        otpTrackingLog.ResponseMessage = errorCodes[apiTrackingResponse.ResponseCode].ReturnMessage;
                }
                else
                {
                    otpTrackingLog.Status = SmsTrackingStatus.SystemError;
                    otpTrackingLog.ResponseMessage = $"Given Error Code Not Exist In Dictionary | Operator Type : {apiTrackingResponse.OperatorType} | Error Code : {apiTrackingResponse.ResponseCode}";
                }
            }
            else
            {
                    otpTrackingLog.Status = SmsTrackingStatus.SystemError;
                    otpTrackingLog.ResponseMessage = $"Given Operator Type Not Exist In Dictionary | Operator Type : {apiTrackingResponse.OperatorType}";
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


    };

    
}