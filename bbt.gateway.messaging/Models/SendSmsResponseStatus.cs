using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace bbt.gateway.messaging.Models
{
    public enum SendSmsResponseStatus
    {
        Success = 200,        
        HasBlacklistRecord = 460,
        SimChange = 461,
        OperatorChange = 462,
        RejectedByOperator = 463,
        NotSubscriber = 464,
        OperatorRejected = 465,
        ClientError = 466,
        ServerError = 465,
    }
}