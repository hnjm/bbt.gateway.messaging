using System.Collections.Generic;
using bbt.gateway.messaging.Models;

namespace bbt.gateway.messaging
{


    public static class Extensions
    {
        public static bool HasBlock(this ICollection<OtpBlackListEntry> entries)
        {
            return true;
        }


          public static SendSmsResponseStatus UnifyResponse(this ICollection<SendOtpResponseLog> logs)
        {
            return SendSmsResponseStatus.Success;
        }
    }
}