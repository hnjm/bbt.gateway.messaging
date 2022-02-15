using bbt.gateway.messaging.Models;
using bbt.gateway.messaging.Repositories;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace bbt.gateway.messaging.Workers.OperatorGateway
{
    public class OperatorIVN : OperatorGatewayBase, IOperatorGateway
    {
        public OperatorIVN()
        {
            Type = OperatorType.IVN;
        }


        public async Task<bool> SendOtp(Phone phone, string content, ConcurrentBag<OtpResponseLog> responses, Header header, bool useControlDays)
        {
            await Task.CompletedTask;
            var response = new OtpResponseLog { 
                Operator = OperatorType.Turkcell,
                Topic = "IVN otp sending",
                TrackingStatus = SmsTrackingStatus.Delivered
            };

            System.Diagnostics.Debug.WriteLine("IVN otp is send");
            response.ResponseCode = SendSmsResponseStatus.NotSubscriber;
            responses.Add(response);
            return true;
        }

      

        public async Task<OtpResponseLog> SendOtp(Phone phone, string content, Header header,bool useControlDays)
        {
           var response = new OtpResponseLog { 
                Operator = OperatorType.IVN,
                Topic = "IVN otp sending",
                TrackingStatus = SmsTrackingStatus.Delivered
            };

            System.Diagnostics.Debug.WriteLine("IVN otp is send");
            response.ResponseCode = SendSmsResponseStatus.NotSubscriber;

            return response;
        }

        public override async Task<OtpTrackingLog> CheckMessageStatus(OtpResponseLog response)
        {
            await Task.CompletedTask;
           throw new NotSupportedException();
        }
    }
}
