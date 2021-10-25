using bbt.gateway.messaging.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace bbt.gateway.messaging.Workers.OperatorGateway
{
    public class OperatorTurkcell : OperatorGatewayBase, IOperatorGateway
    {
        public OperatorTurkcell()
        {
            Type = OperatorType.Turkcell;
        }

        public void SendOtp(Phone phone, string content, ConcurrentBag<SendOtpResponseLog> responses, Header header, bool useControlDays)
        {
            var response = new SendOtpResponseLog
            {
                Operator = OperatorType.Turkcell,
                Topic = "Turkcell otp sending",
                TrackingStatus = SmsTrackingStatus.Pending
            };

            System.Diagnostics.Debug.WriteLine("Turkcell otp is send");

            response.ResponseCode = SendSmsResponseStatus.Success;

            responses.Add(response);
        }

        public SendOtpResponseLog SendOtp(Phone phone, string content, Header header)
        {
            var response = new SendOtpResponseLog
            {
                Operator = OperatorType.Turkcell,
                Topic = "Turkcell otp sending",
                TrackingStatus = SmsTrackingStatus.Pending
            };

            System.Diagnostics.Debug.WriteLine("Turkcell otp is send");
            response.ResponseCode = SendSmsResponseStatus.Success;

            return response;
        }
    }
}
