using bbt.gateway.messaging.Models;
using bbt.gateway.messaging.Workers.OperatorGateway;
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

        OtpRequestLog _requestLog;

        Type[] operators = new Type[] { typeof(OperatorTurkcell), typeof(OperatorVodafone), typeof(OperatorTurkTelekom) };

        public OtpSender(SendMessageSmsRequest data)
        {
            _data = data;

            _requestLog = new OtpRequestLog
            {
                CreatedBy = _data.Process,
                Phone = _data.Phone
            };
        }

        public SendSmsResponseStatus SendMessage()
        {
            SendSmsResponseStatus returnValue = SendSmsResponseStatus.ClientError;

            using (var db = new DatabaseContext())
            {
                // Load Phone configuration and related active blacklist entiries.
                var phoneConfiguration = db.PhoneConfigurations.Where(i =>
                    i.Phone.CountryCode == _data.Phone.CountryCode &&
                    i.Phone.Prefix == _data.Phone.Prefix &&
                    i.Phone.Number == _data.Phone.Number
                    )
                    .Include(c => c.BlacklistEntries.Where(b => b.ValidTo > DateTime.Now))
                    .FirstOrDefault();

                // if known number without blacklist entry 
                if (
                    phoneConfiguration != null &&
                    phoneConfiguration.Operator != null &&
                    !phoneConfiguration.BlacklistEntries.Any(b => b.Status == BlacklistStatus.NotResolved)
                )
                {
                    var responseLog = SendMessageToKnown(phoneConfiguration);
                    _requestLog.ResponseLogs.Add(responseLog);
                    returnValue = SendSmsResponseStatus.Success;
                }
                else
                {
                    //If configuration is not available then create clean phone configuration to phone number   
                    if (phoneConfiguration == null)
                    {
                        phoneConfiguration = createNewPhoneConfiguration();
                        db.Add(phoneConfiguration);
                    }


                    // If phone is enter to blacklist and reason is resolved, then do not apply black list control.
                    var useControlDays = !(
                        phoneConfiguration.BlacklistEntries != null && 
                        phoneConfiguration.BlacklistEntries.Count > 0 && 
                        phoneConfiguration.BlacklistEntries.All(b => b.Status == BlacklistStatus.Resolved)
                        );

                    var responseLogs = SendMessageToUnknown(phoneConfiguration, useControlDays);

                    // Decide method return code    
                    returnValue = responseLogs.UnifyResponse();

                    // Update with valid operator if any otp sending 
                    var successAttempt = responseLogs.FirstOrDefault(l => l.ResponseCode == SendSmsResponseStatus.Success);
                    if (successAttempt != null)
                        phoneConfiguration.Operator = successAttempt.Operator;

                    // Add all response logs to request log
                    responseLogs.ForEach(l => _requestLog.ResponseLogs.Add(l));
                }

                _requestLog.PhoneConfiguration = phoneConfiguration;
                db.Add(_requestLog);
                db.SaveChanges();
            }

            return returnValue;
        }

        private List<OtpResponseLog> SendMessageToUnknown(PhoneConfiguration phoneConfiguration, bool useControlDays)
        {
            Header header = loadHeader(phoneConfiguration);

            ConcurrentBag<OtpResponseLog> responses = new ConcurrentBag<OtpResponseLog>();

            Parallel.ForEach(operators, currentElement =>
            {
                IOperatorGateway gateway = (IOperatorGateway)Activator.CreateInstance(currentElement);
                gateway.SendOtp(_data.Phone, "test", responses, header, useControlDays);
            });

            return responses.ToList();
        }

        private OtpResponseLog SendMessageToKnown(PhoneConfiguration phoneConfiguration)
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
            var header = HeaderManager.Instance.Get(phoneConfiguration, _data.ContentType);

            //Update request log to persisting content
            _requestLog.Content = header.BuildContentForLog(_data.Content);

            return header;
        }


        private PhoneConfiguration createNewPhoneConfiguration()
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
                RelatedId = _requestLog.Id,
                CreatedBy = _data.Process
            });

            _requestLog.PhoneConfiguration = newConfig;
            return newConfig;
        }
    }
}
