using bbt.gateway.messaging.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace bbt.gateway.messaging.Workers.OperatorGateway
{
    public class OperatorTurkTelekom : IOperatorGateway
    {
        public void SendOtp(Phone phone, string content, ConcurrentBag<SendOtpResponseLog> responses)
        {
            System.Diagnostics.Debug.WriteLine("TT otp is send");
        }
    }
}
