using bbt.gateway.messaging.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace bbt.gateway.messaging.Workers.OperatorGateway
{
    public abstract class OperatorGatewayBase
    {
        private OperatorType type;
        private readonly OperatorManager _operatorManager;
        private readonly DatabaseContext  _databaseContext;
        protected OperatorGatewayBase(OperatorManager operatorManager,DatabaseContext databaseContext) 
        {
            _operatorManager = operatorManager;
            _databaseContext = databaseContext;
        }

        protected OperatorType Type
        {
            get { return type; }
            set
            {
                type = value;
                OperatorConfig = _operatorManager.Get(value);
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


            var dbContextOptions = new DbContextOptionsBuilder<DatabaseContext>()
            .UseSqlServer(Environment.GetEnvironmentVariable("SQL_CONNECTION"))
            .Options;

            using var dbContext = new DatabaseContext(dbContextOptions);
            dbContext.AddRange(logs);
            dbContext.SaveChanges();
            

            System.Diagnostics.Debug.WriteLine($"{Type} tracking otp is finished");
        }
    }



}
