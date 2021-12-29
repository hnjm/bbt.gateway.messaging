using bbt.gateway.messaging.Models;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace bbt.gateway.messaging.Workers.OperatorGateway
{
    public class OperatorIVN : OperatorGatewayBase, IOperatorGateway
    {
        public OperatorIVN(OperatorManager operatorManager, DatabaseContext databaseContext) : base(operatorManager,databaseContext)
        {
            Type = OperatorType.IVN;
        }


        public void SendOtp(Phone phone, string content, ConcurrentBag<OtpResponseLog> responses, Header header, bool useControlDays)
        {
            var response = new OtpResponseLog { 
                Operator = OperatorType.Turkcell,
                Topic = "IVN otp sending",
                TrackingStatus = SmsTrackingStatus.Delivered
            };

            System.Diagnostics.Debug.WriteLine("IVN otp is send");
            response.ResponseCode = SendSmsResponseStatus.NotSubscriber;
            responses.Add(response);
        }

      

        public async Task<OtpResponseLog> SendOtp(Phone phone, string content, Header header)
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

        public override Task<OtpTrackingLog> CheckMessageStatus(OtpResponseLog response)
        {
           throw new NotSupportedException();
        }
    }
}
