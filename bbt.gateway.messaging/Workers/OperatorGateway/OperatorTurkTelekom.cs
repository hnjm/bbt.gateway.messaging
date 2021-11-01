using bbt.gateway.messaging.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace bbt.gateway.messaging.Workers.OperatorGateway
{


    public class OperatorTurkTelekom : OperatorGatewayBase, IOperatorGateway
    {

        public OperatorTurkTelekom()
        {
            Type = OperatorType.TurkTelekom;
        }

        public void SendOtp(Phone phone, string content, ConcurrentBag<OtpResponseLog> responses, Header header, bool useControlDays)
        {
            var response = new OtpResponseLog
            {
                Operator = OperatorType.TurkTelekom,
                Topic = "TT otp sending",
                TrackingStatus = SmsTrackingStatus.Pending
            };

            System.Diagnostics.Debug.WriteLine("TT otp is send");

            response.ResponseCode = SendSmsResponseStatus.NotSubscriber;

            Task.Run(() => TrackMessageStatus(response));

            responses.Add(response);
        }
        public OtpResponseLog SendOtp(Phone phone, string content, Header header)
        {
            var response = new OtpResponseLog
            {
                Operator = OperatorType.TurkTelekom,
                Topic = "TurkTelekom otp sending",
                TrackingStatus = SmsTrackingStatus.Pending
            };

            System.Diagnostics.Debug.WriteLine("TurkTelekom otp is send");

            response.ResponseCode = SendSmsResponseStatus.NotSubscriber;

            Task.Run(() => TrackMessageStatus(response));


            return response;
        }


        public override OtpTrackingLog CheckMessageStatus(OtpResponseLog response)
        {
            return new OtpTrackingLog { LogId = response.Id, Status = SmsTrackingStatus.Pending, Detail = "No details" };
        }
    }
}
