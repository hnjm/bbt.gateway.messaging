using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using bbt.gateway.messaging.Api.TurkTelekom.Model;
using bbt.gateway.messaging.Models;

namespace bbt.gateway.messaging
{


    public static class Extensions
    {

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
            return $"[{header.SmsSender}]{header.SmsPrefix} {content} {header.SmsSuffix}";
        }

        public static OtpResponseLog BuildResponseForTurkTelekom(this TurkTelekomSmsResponse turkTelekomResponse)
        {
            var response = new OtpResponseLog
            {
                Operator = OperatorType.TurkTelekom,
                Topic = "TurkTelekom otp sending",
                TrackingStatus = SmsTrackingStatus.Pending
            };

            response.ResponseMessage = turkTelekomResponse.ResponseSms.ReturnMessage;
            switch (turkTelekomResponse.ResponseSms.ReturnCode)
            {
                case "0":
                    response.ResponseCode = SendSmsResponseStatus.Success;
                    response.StatusQueryId = turkTelekomResponse.ResponseSms.MessageId;
                    break;
                case "8":
                    response.ResponseCode = SendSmsResponseStatus.NotSubscriber;
                    break;
                case "9":
                    response.ResponseCode = SendSmsResponseStatus.SimChange;
                    break;
                case "15":
                    response.ResponseCode = SendSmsResponseStatus.OperatorChange;
                    break;
                case "29":
                    response.ResponseCode = SendSmsResponseStatus.RejectedByOperator;
                    break;
                default:
                    response.ResponseCode = SendSmsResponseStatus.ServerError;
                    break;
            }

            return response;
        }

        public static OtpTrackingLog BuildResponseForTurkTelekom(this TurkTelekomSmsStatusResponse turkTelekomResponse,OtpResponseLog response)
        {
            var otpTrackingLog = new OtpTrackingLog { LogId = response.Id, Status = (turkTelekomResponse.ResponseSmsStatus.DCode == "000" ? SmsTrackingStatus.Delivered : SmsTrackingStatus.Pending), Detail = turkTelekomResponse.SerializeXml<TurkTelekomSmsStatusResponse>()};
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
                xmlSerializer.Serialize(textWriter, toSerialize,xmlnsEmpty);
                return textWriter.ToString();
            }
        }
    }
}