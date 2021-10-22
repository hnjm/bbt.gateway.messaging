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
            var response = new SendOtpResponseLog { 
                Operator = OperatorType.Vodafone,
                Topic = "Vodafone otp sending"
            };

            System.Diagnostics.Debug.WriteLine("Vodafone otp is send");
            response.OperatorResponseCode = OperatorResponseType.Success;

            responses.Add(response);
        }


        public SendOtpResponseLog SendOtp(Phone phone, string content)
        {
             var response = new SendOtpResponseLog { 
                Operator = OperatorType.Vodafone,
                Topic = "Vodafone otp sending"
            };

            System.Diagnostics.Debug.WriteLine("Vodafone otp is send");
            response.OperatorResponseCode = OperatorResponseType.Success;

            return response;
        }


        
    }
}
