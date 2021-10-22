using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace bbt.gateway.messaging.Models
{
    public enum OperatorResponseType
    {
        Success = 1,
        NotMember = 2,
        Closed = 3,
        Newcomer = 4,
        SimChanged = 5,

        Unclassified = 100,
        ClientError = 400,
        ServerError = 500,
    }
}
