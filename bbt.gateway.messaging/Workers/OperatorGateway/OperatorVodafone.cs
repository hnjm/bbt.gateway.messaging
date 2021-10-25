using bbt.gateway.messaging.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace bbt.gateway.messaging.Workers.OperatorGateway
{
    public class OperatorVodafone : OperatorGatewayBase, IOperatorGateway
    {
        public OperatorVodafone()
        {
            Type = OperatorType.Vodafone;
        }
        public void SendOtp(Phone phone, string content, ConcurrentBag<SendOtpResponseLog> responses, Header header, bool useControlDays)
        {

            var response = new SendOtpResponseLog
            {
                Operator = OperatorType.Vodafone,
                Topic = "Vodafone otp sending",
                TrackingStatus = SmsTrackingStatus.Pending
            };

            System.Diagnostics.Debug.WriteLine("Vodafone otp is send");
            response.ResponseCode = SendSmsResponseStatus.NotSubscriber;

            responses.Add(response);

            Task.Run(() => TrackMessageStatus(response));
        }

        public SendOtpResponseLog SendOtp(Phone phone, string content, Header header)
        {
            var response = new SendOtpResponseLog
            {
                Operator = OperatorType.Vodafone,
                Topic = "Vodafone otp sending",
                TrackingStatus = SmsTrackingStatus.Pending
            };

            System.Diagnostics.Debug.WriteLine("Vodafone otp is send");
            response.ResponseCode = SendSmsResponseStatus.NotSubscriber;
            Task.Run(() => TrackMessageStatus(response));

            return response;
        }
        public override SmsTrackingLog CheckMessageStatus(SendOtpResponseLog response)
        {
           return new SmsTrackingLog { SendOtpResponseLogId = response.Id, Status = SmsTrackingStatus.Pending, Detail = "No details" };
        }
    }
}
