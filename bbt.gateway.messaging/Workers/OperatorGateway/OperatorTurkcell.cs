using bbt.gateway.messaging.Models;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace bbt.gateway.messaging.Workers.OperatorGateway
{
    public class OperatorTurkcell : OperatorGatewayBase, IOperatorGateway
    {
        public OperatorTurkcell(OperatorManager operatorManager, DatabaseContext databaseContext) : base (operatorManager,databaseContext)
        {
            Type = OperatorType.Turkcell;
        }

        public void SendOtp(Phone phone, string content, ConcurrentBag<OtpResponseLog> responses, Header header, bool useControlDays)
        {
            var response = new OtpResponseLog
            {
                Operator = OperatorType.Turkcell,
                Topic = "Turkcell otp sending",
                TrackingStatus = SmsTrackingStatus.Pending
            };

            System.Diagnostics.Debug.WriteLine("Turkcell otp is send");

            response.ResponseCode = SendSmsResponseStatus.Success;
            response.StatusQueryId = "1254";

            responses.Add(response);

            Task.Run(() => TrackMessageStatus(response));
        }

        public OtpResponseLog SendOtp(Phone phone, string content, Header header)
        {
            var response = new OtpResponseLog
            {
                Operator = OperatorType.Turkcell,
                Topic = "Turkcell otp sending",
                TrackingStatus = SmsTrackingStatus.Pending
            };

            System.Diagnostics.Debug.WriteLine("Turkcell otp is send");
            response.ResponseCode = SendSmsResponseStatus.Success;

            Task.Run(() => TrackMessageStatus(response));

            return response;
        }

        public override OtpTrackingLog CheckMessageStatus(OtpResponseLog response)
        {
            return new OtpTrackingLog { LogId = response.Id, Status = SmsTrackingStatus.Pending, Detail = "No details" };
        }
    }
}
