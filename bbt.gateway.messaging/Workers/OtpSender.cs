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

        Type[] operators = new Type[] { typeof(OperatorTurkcell), typeof(OperatorVodafone), typeof(OperatorTurkTelekom) };

        public OtpSender(SendMessageSmsRequest data)
        {
            _data = data;
        }

        public SendSmsResponse SendMessage()
        {
            using (var db = new DatabaseContext())
            {
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
                    SendMessageToUnknown();

                }
                else {

                    SendMessageToKnown(phoneConfiguration);
                }

                return null;
            }
        }


        private SendSmsResponse SendMessageToUnknown()
        {

            ConcurrentBag<SendOtpResponseLog> responses = new ConcurrentBag<SendOtpResponseLog>();

            Parallel.ForEach(operators, currentElement =>
            {
                IOperatorGateway gateway =  (IOperatorGateway)Activator.CreateInstance(currentElement);
                gateway.SendOtp(_data.Phone, "test", responses);
            });



            return null; 

        }

        private SendSmsResponse SendMessageToKnown(PhoneConfiguration phoneConfiguration)
        {
            return null;
        }


    }
}
