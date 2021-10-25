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

        public SendSmsResponseStatus SendMessage()
        {
            SendSmsResponseStatus returnValue;

            using (var db = new DatabaseContext())
            {
                db.Add(_requestLog);

                // Telefon bilgisi ve aktif olan tum kara liste kayitlari getirilir.
                var phoneConfiguration = db.PhoneConfigurations.Where(i =>
                    i.Phone.CountryCode == _data.Phone.CountryCode &&
                    i.Phone.Prefix == _data.Phone.Prefix &&
                    i.Phone.Number == _data.Phone.Number
                    )
                    .Include(c => c.BlacklistEntries.Where(b => b.ValidTo > DateTime.Now && (b.Status == Constants.Status.Blacklist.Active || b.Status == Constants.Status.Blacklist.Resolved)))
                    .FirstOrDefault();

                if (phoneConfiguration == null
                    || phoneConfiguration.Operator == null
                    || phoneConfiguration.BlacklistEntries.All(b => b.Status == Constants.Status.Blacklist.Resolved))
                {
                    if (phoneConfiguration == null) phoneConfiguration = createNewPhoneConfiguration(db);


                    var responseLogs = SendMessageToUnknown(
                        phoneConfiguration, 
                        phoneConfiguration.BlacklistEntries?.Count > 0);

                    returnValue = responseLogs.UnifyResponse();
                    responseLogs.ForEach(l => _requestLog.ResponseLogs.Add(l));
                }
                else
                {
                    if (phoneConfiguration.BlacklistEntries.Any(b => b.Status == Constants.Status.Blacklist.Active))
                    {
                        returnValue = SendSmsResponseStatus.HasBlacklistRecord;
                    }
                    else
                    {
                        var responseLog = SendMessageToKnown(phoneConfiguration);

                        _requestLog.ResponseLogs.Add(responseLog);
                        returnValue = SendSmsResponseStatus.Success;
                    }

                }
                _requestLog.PhoneConfiguration = phoneConfiguration;
                db.SaveChanges();
            }
            return returnValue;
        }

        private List<SendOtpResponseLog> SendMessageToUnknown(PhoneConfiguration phoneConfiguration, bool useControlDays)
        {
            Header header = loadHeader(phoneConfiguration);

            ConcurrentBag<SendOtpResponseLog> responses = new ConcurrentBag<SendOtpResponseLog>();

            Parallel.ForEach(operators, currentElement =>
            {
                IOperatorGateway gateway = (IOperatorGateway)Activator.CreateInstance(currentElement);
                gateway.SendOtp(_data.Phone, "test", responses, header, useControlDays);
            });

            return responses.ToList();
        }



        private SendOtpResponseLog SendMessageToKnown(PhoneConfiguration phoneConfiguration)
        {
            IOperatorGateway gateway = null;
            Header header = loadHeader(phoneConfiguration);

            switch (phoneConfiguration.Operator)
            {
                case OperatorType.Turkcell:
                    gateway = new OperatorTurkcell();
                    break;
                case OperatorType.Vodafone:
                    gateway = new OperatorVodafone();
                    break;

                case OperatorType.TurkTelekom:
                    gateway = new OperatorTurkTelekom();
                    break;
                case OperatorType.IVN:
                    gateway = new OperatorIVN();
                    break;
                default:
                    // Serious Exception
                    break;
            }

            var result = gateway.SendOtp(_data.Phone, _data.Content, header);

            return result;
        }

        private Header loadHeader(PhoneConfiguration phoneConfiguration)
        {
            var header = HeaderManager.Instance.GetHeader(phoneConfiguration, _data.ContentType);

            //Update request log to persisting content
            _requestLog.Content = header.BuildContentForLog(_data.Content);

            return header;
        }


        private PhoneConfiguration createNewPhoneConfiguration(DatabaseContext db)
        {
            var newConfig = new PhoneConfiguration
            {
                Phone = _data.Phone,
                Logs = new List<PhoneConfigurationLog>()
            };

            newConfig.Logs.Add(new PhoneConfigurationLog
            {
                Type = "Initialization",
                Action = "Send Otp Request",
                ParameterMaster = _requestLog.Id.ToString(),
                CreatedBy = _data.Process
            });

            _requestLog.PhoneConfiguration = newConfig;

            db.Add(newConfig);
            return newConfig;
        }
    }
}
