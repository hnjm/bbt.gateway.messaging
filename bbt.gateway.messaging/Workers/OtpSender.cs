using bbt.gateway.messaging.Models;
using bbt.gateway.messaging.Workers.OperatorGateway;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace bbt.gateway.messaging.Workers
{
    public class OtpSender
    {
        SendMessageSmsRequest _data;

        SendOtpRequestLog _requestLog;

        Type[] operators = new Type[] { typeof(OperatorTurkcell), typeof(OperatorVodafone), typeof(OperatorTurkTelekom) };

        public OtpSender(SendMessageSmsRequest data)
        {
             _data = data;

            _requestLog = new SendOtpRequestLog
            {
                CreatedBy = _data.Process,
                Phone = _data.Phone
            };
        }

        public SendSmsResponse SendMessage()
        {
            using (var db = new DatabaseContext())
            {
                db.Add(_requestLog);

                // Telefon bilgisi ve aktif olan tum kara liste kayitlari getirilir.
                var phoneConfiguration = db.PhoneConfigurations.Where(i =>
                    i.Phone.CountryCode == _data.Phone.CountryCode &&
                    i.Phone.Prefix == _data.Phone.Prefix &&
                    i.Phone.Number == _data.Phone.Number
                    )
                    .Include(c => c.BlacklistEntries)
                    .FirstOrDefault();

                if (phoneConfiguration == null)
                {
                    var newConfig = new PhoneConfiguration
                    {
                        Phone = _data.Phone
                    };

                    _requestLog.PhoneConfiguration = newConfig;
                    db.Add(newConfig);
                    //db.SaveChanges();

                    var responseLogs = SendMessageToUnknown();
                    responseLogs.ForEach(l => {
                        _requestLog.ResponseLogs.Add(l);
                    });

                    db.SaveChanges();
                }
                else
                {
                    _requestLog.PhoneConfiguration = phoneConfiguration;
                    db.SaveChanges();

                    SendMessageToKnown(phoneConfiguration);
                }

                return null;
            }
        }

        private List<SendOtpResponseLog> SendMessageToUnknown()
        {
            ConcurrentBag<SendOtpResponseLog> responses = new ConcurrentBag<SendOtpResponseLog>();

            Parallel.ForEach(operators, currentElement =>
            {
                IOperatorGateway gateway = (IOperatorGateway)Activator.CreateInstance(currentElement);
                gateway.SendOtp(_data.Phone, "test", responses);
            });

            return responses.ToList();
        }

        private SendSmsResponse SendMessageToKnown(PhoneConfiguration phoneConfiguration)
        {
            return null;
        }
    }
}
