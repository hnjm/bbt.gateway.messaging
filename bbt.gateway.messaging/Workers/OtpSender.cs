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
                    .Include(c => c.BlacklistEntries.Where(b => b.ValidTo > DateTime.Now && b.Status == Constants.Status.Blacklist.Active))
                    .FirstOrDefault();

                if (phoneConfiguration == null)
                {
                    var newConfig = new PhoneConfiguration
                    {
                        Phone = _data.Phone
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

                    var responseLogs = SendMessageToUnknown(newConfig);

                    returnValue = responseLogs.UnifyResponse();

                    responseLogs.ForEach(l => _requestLog.ResponseLogs.Add(l));
                }
                else
                {
                    _requestLog.PhoneConfiguration = phoneConfiguration;

                    if (phoneConfiguration.BlacklistEntries.HasBlock())
                    {
                        returnValue = SendSmsResponseStatus.HasBlacklistRecord;
                    }
                    else
                    {
                        var responseLog = SendMessageToKnown(phoneConfiguration);
                        _requestLog.ResponseLogs.Add(responseLog);
                        returnValue = SendSmsResponseStatus.Success;
                    }
                    db.SaveChanges();
                }

                return returnValue;
            }
        }

        private List<SendOtpResponseLog> SendMessageToUnknown(PhoneConfiguration phoneConfiguration)
        {
            var header = HeaderManager.Instance.GetHeader(phoneConfiguration);

            ConcurrentBag<SendOtpResponseLog> responses = new ConcurrentBag<SendOtpResponseLog>();

            Parallel.ForEach(operators, currentElement =>
            {
                IOperatorGateway gateway = (IOperatorGateway)Activator.CreateInstance(currentElement);
                gateway.SendOtp(_data.Phone, "test", responses, header);
            });

            return responses.ToList();
        }

        private SendOtpResponseLog SendMessageToKnown(PhoneConfiguration phoneConfiguration)
        {
            IOperatorGateway gateway = null;
            var header = HeaderManager.Instance.GetHeader(phoneConfiguration);

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
    }
}
