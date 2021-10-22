﻿using bbt.gateway.messaging.Models;
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
             var response = new SendOtpResponseLog { 
                Operator = OperatorType.TurkTelekom,
                Topic = "TT otp sending"
            };

            System.Diagnostics.Debug.WriteLine("TT otp is send");

            response.OperatorResponseCode = OperatorResponseType.Success;

            responses.Add(response);
        }
        public SendOtpResponseLog SendOtp(Phone phone, string content)
        {
             var response = new SendOtpResponseLog { 
                Operator = OperatorType.TurkTelekom,
                Topic = "TurkTelekom otp sending"
            };

            System.Diagnostics.Debug.WriteLine("TurkTelekom otp is send");

            response.OperatorResponseCode = OperatorResponseType.Success;

            return response;
        }
    }
}
