using bbt.gateway.messaging.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace bbt.gateway.messaging.Workers.OperatorGateway
{
    public interface IOperatorGateway
    {
        void SendOtp(Phone phone, string content, ConcurrentBag<OtpResponseLog> responses, Header header, bool useControlDays);
        OtpResponseLog SendOtp(Phone phone, string content, Header header);

    }
}
