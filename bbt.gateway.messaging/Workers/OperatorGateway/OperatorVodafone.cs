using bbt.gateway.messaging.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace bbt.gateway.messaging.Workers.OperatorGateway
{
    public class OperatorVodafone : IOperatorGateway
    {
        public void SendOtp(Phone phone, string content, ConcurrentBag<SendOtpResponseLog> responses)
        {
            responses.Add(new SendOtpResponseLog { 
                Operator = OperatorType.Vodafone,
                Topic = "Otp Sending over service"

               
            
            });


            System.Diagnostics.Debug.WriteLine("Vodafone otp is send");
        }
    }1
}
