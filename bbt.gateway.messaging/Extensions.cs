using System.Collections.Generic;
using System.Linq;
using bbt.gateway.messaging.Models;

namespace bbt.gateway.messaging
{


    public static class Extensions
    {

        public static SendSmsResponseStatus UnifyResponse(this ICollection<SendOtpResponseLog> logs)
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
    }
}