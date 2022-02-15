using bbt.gateway.messaging.Models;
using bbt.gateway.messaging.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace bbt.gateway.messaging.Workers.OperatorGateway
{
    public abstract class OperatorGatewayBase
    {
        private OperatorType type;
       
        protected OperatorGatewayBase() 
        {
            
        }

        protected OperatorType Type
        {
            get { return type; }
            set
            {
                type = value;
                using var databaseContext = new DatabaseContext();
                OperatorConfig = databaseContext.Operators.FirstOrDefault(o => o.Type == type);
            }
        }
        protected Operator OperatorConfig { get; set; }

        protected void SaveOperator()
        {
            using var databaseContext = new DatabaseContext();
            databaseContext.Operators.Update(OperatorConfig);
            databaseContext.SaveChanges();
        }

        protected PhoneConfiguration GetPhoneConfiguration(Phone phone)
        {
            using var databaseContext = new DatabaseContext();
            return databaseContext.PhoneConfigurations.Where(i =>
                i.Phone.CountryCode == phone.CountryCode &&
                i.Phone.Prefix == phone.Prefix &&
                i.Phone.Number == phone.Number
                )
                .FirstOrDefault();
        }

        public abstract  Task<OtpTrackingLog> CheckMessageStatus(OtpResponseLog response);

        public async Task<bool> TrackMessageStatus(OtpResponseLog response)
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


            using var _databaseContext = new DatabaseContext();
            _databaseContext.AddRange(logs);
            _databaseContext.SaveChanges();

            System.Diagnostics.Debug.WriteLine($"{Type} tracking otp is finished");
            return true;

        }
    }



}
