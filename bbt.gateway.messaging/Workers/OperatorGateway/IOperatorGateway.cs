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
        void SendOtp(Phone phone, string content, ConcurrentBag<SendOtpResponseLog> responses, Header header);
        SendOtpResponseLog SendOtp(Phone phone, string content, Header header);
    }
}
