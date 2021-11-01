using bbt.gateway.messaging.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace bbt.gateway.messaging.Workers.OperatorGateway
{
    public abstract class OperatorGatewayBase
    {
        private OperatorType type;
        protected OperatorType Type
        {
            get { return type; }
            set
            {
                type = value;
                OperatorConfig = OperatorManager.Instance.Get(value);
            }
        }
        protected Operator OperatorConfig { get; set; }

        public abstract OtpTrackingLog CheckMessageStatus(OtpResponseLog response);

        public void TrackMessageStatus(OtpResponseLog response)
        {
            System.Diagnostics.Debug.WriteLine($"{Type} tracking otp is started");

            List<OtpTrackingLog> logs = new List<OtpTrackingLog>();

            var maxRetryCount = 5;
            while (maxRetryCount-- > 0)
            {
                Task.Delay(1000);
                var log = CheckMessageStatus(response);
                logs.Add(log);

                if (log.Status == SmsTrackingStatus.Delivered || log.Status == SmsTrackingStatus.DeviceRejected || log.Status == SmsTrackingStatus.Expired)
                    break;


                System.Diagnostics.Debug.WriteLine($"{Type} is tracking otp status. Times : {maxRetryCount}");
            }

            using (var db = new DatabaseContext())
            {
                db.AddRange(logs);
                db.SaveChanges();
            }

            System.Diagnostics.Debug.WriteLine($"{Type} tracking otp is finished");
        }
    }



}
