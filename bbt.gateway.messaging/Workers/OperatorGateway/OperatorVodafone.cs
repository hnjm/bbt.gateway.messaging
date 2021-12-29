﻿using bbt.gateway.messaging.Models;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace bbt.gateway.messaging.Workers.OperatorGateway
{
    public class OperatorVodafone : OperatorGatewayBase, IOperatorGateway
    {
        public OperatorVodafone(OperatorManager operatorManager, DatabaseContext databaseContext) : base (operatorManager,databaseContext)
        {
            Type = OperatorType.Vodafone;
        }
        public void SendOtp(Phone phone, string content, ConcurrentBag<OtpResponseLog> responses, Header header, bool useControlDays)
        {

            var response = new OtpResponseLog
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

        public async Task<OtpResponseLog> SendOtp(Phone phone, string content, Header header)
        {
            var response = new OtpResponseLog
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
        public override Task<OtpTrackingLog> CheckMessageStatus(OtpResponseLog response)
        {
            return null;
           //return new OtpTrackingLog { LogId = response.Id, Status = SmsTrackingStatus.Pending, Detail = "No details" };
        }
    }
}
