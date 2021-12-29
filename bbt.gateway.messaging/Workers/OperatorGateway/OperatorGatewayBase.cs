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

        public abstract Task<OtpTrackingLog> CheckMessageStatus(OtpResponseLog response);

        public async void TrackMessageStatus(OtpResponseLog response)
        {
            System.Diagnostics.Debug.WriteLine($"{Type} tracking otp is started");

            List<OtpTrackingLog> logs = new List<OtpTrackingLog>();

            var maxRetryCount = 5;
            while (maxRetryCount-- > 0)
            {
                await Task.Delay(1000);
                var log = await CheckMessageStatus(response);
                logs.Add(log);

                if (log.Status == SmsTrackingStatus.Delivered || log.Status == SmsTrackingStatus.DeviceRejected || log.Status == SmsTrackingStatus.Expired)
                    break;


                System.Diagnostics.Debug.WriteLine($"{Type} is tracking otp status. Times : {maxRetryCount}");
            }


            var contextOptions = new DbContextOptionsBuilder<DatabaseContext>()
            .UseSqlServer(Environment.GetEnvironmentVariable("SQL_CONNECTION"))
            .Options;

            using var context = new DatabaseContext(contextOptions);
            context.AddRange(logs);
            context.SaveChanges();
            

            System.Diagnostics.Debug.WriteLine($"{Type} tracking otp is finished");
        }
    }



}
