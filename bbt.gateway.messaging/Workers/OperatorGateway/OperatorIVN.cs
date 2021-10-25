using bbt.gateway.messaging.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace bbt.gateway.messaging.Workers.OperatorGateway
{
    public class OperatorIVN : OperatorGatewayBase, IOperatorGateway
    {
        public OperatorIVN()
        {
            Type = OperatorType.IVN;
        }


        public void SendOtp(Phone phone, string content, ConcurrentBag<SendOtpResponseLog> responses, Header header, bool useControlDays)
        {
            var response = new SendOtpResponseLog { 
                Operator = OperatorType.Turkcell,
                Topic = "IVN otp sending",
                TrackingStatus = SmsTrackingStatus.Pending
            };

            System.Diagnostics.Debug.WriteLine("IVN otp is send");
            response.ResponseCode = SendSmsResponseStatus.NotSubscriber;

            responses.Add(response);
        }

        public SendOtpResponseLog SendOtp(Phone phone, string content, Header header)
        {
           var response = new SendOtpResponseLog { 
                Operator = OperatorType.IVN,
                Topic = "IVN otp sending",
                TrackingStatus = SmsTrackingStatus.Pending
            };

            System.Diagnostics.Debug.WriteLine("IVN otp is send");
            response.ResponseCode = SendSmsResponseStatus.NotSubscriber;

            return response;
        }
    }
}
