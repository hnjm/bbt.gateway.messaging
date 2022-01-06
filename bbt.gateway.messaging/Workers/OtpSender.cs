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
        private readonly DatabaseContext _databaseContext;
        private readonly HeaderManager _headerManager;
        private readonly Func<OperatorType, IOperatorGateway> _operatorRepository;
        SendMessageSmsRequest _data;

        OtpRequestLog _requestLog;

        //Type[] operators = new Type[] { typeof(OperatorTurkcell), typeof(OperatorVodafone), typeof(OperatorTurkTelekom) };
        private readonly Dictionary<Type, OperatorType> operators = new Dictionary<Type, OperatorType>()
        {
            { typeof(OperatorTurkcell) , OperatorType.Turkcell},
            { typeof(OperatorVodafone) , OperatorType.Vodafone},
            { typeof(OperatorTurkTelekom) , OperatorType.TurkTelekom},
            { typeof(OperatorIVN) , OperatorType.IVN}
        };
        public OtpSender(DatabaseContext databaseContext,HeaderManager headerManager,
            Func<OperatorType, IOperatorGateway> operatorRepository)
        {
            _databaseContext = databaseContext;
            _headerManager = headerManager;
            _operatorRepository = operatorRepository;
        }

        public SendSmsResponseStatus SendMessage(SendMessageSmsRequest sendMessageSmsRequest)
        {
            SendSmsResponseStatus returnValue = SendSmsResponseStatus.ClientError;

            _data = sendMessageSmsRequest;
            _requestLog = new OtpRequestLog
            {
                CreatedBy = _data.Process,
                Phone = _data.Phone
            };

            // Load Phone configuration and related active blacklist entiries.
            var phoneConfiguration = _databaseContext.PhoneConfigurations.Where(i =>
                i.Phone.CountryCode == _data.Phone.CountryCode &&
                i.Phone.Prefix == _data.Phone.Prefix &&
                i.Phone.Number == _data.Phone.Number
                )
                .Include(c => c.BlacklistEntries.Where(b => b.ValidTo > DateTime.Today))
                .FirstOrDefault();

            // if known number without any blacklist entry 
            if (
                phoneConfiguration != null &&
                phoneConfiguration.Operator != null &&
                !phoneConfiguration.BlacklistEntries.Any(b => b.Status == BlacklistStatus.NotResolved)
            )
            {
                //TODO: Operator ghonderemezse? Sim değişmişse?
                var responseLog = SendMessageToKnown(phoneConfiguration);
                _requestLog.ResponseLogs.Add(responseLog);
                returnValue = responseLog.ResponseCode;
            }
            else
            {
                //If configuration is not available then create clean phone configuration to phone number   
                if (phoneConfiguration == null)
                {
                    phoneConfiguration = createNewPhoneConfiguration();
                    _databaseContext.Add(phoneConfiguration);
                }


                // If phone is in blacklist and reason is resolved, then do not apply black list control.
                var useControlDays = !(
                    phoneConfiguration.BlacklistEntries != null &&
                    phoneConfiguration.BlacklistEntries.Count > 0 &&
                    phoneConfiguration.BlacklistEntries.All(b => b.Status == BlacklistStatus.Resolved)
                    );

                var responseLogs = SendMessageToUnknown(phoneConfiguration, useControlDays);

                // Decide which response code will be returned
                returnValue = responseLogs.UnifyResponse();

                //TODO: Blackliste eklenmesi gerekiyorsa ekle.
                if (returnValue == SendSmsResponseStatus.OperatorChange || returnValue == SendSmsResponseStatus.SimChange)
                {
                    var blackListEntry = createBlackListEntry(phoneConfiguration, returnValue.ToString(), "türk telekom");
                }


                // Update with valid operator if any otp sending 
                var successAttempt = responseLogs.FirstOrDefault(l => l.ResponseCode == SendSmsResponseStatus.Success);
                if (successAttempt != null)
                    phoneConfiguration.Operator = successAttempt.Operator;

                // Add all response logs to request log
                responseLogs.ForEach(l => _requestLog.ResponseLogs.Add(l));
            }

            _requestLog.PhoneConfiguration = phoneConfiguration;
            _databaseContext.Add(_requestLog);
            _databaseContext.SaveChanges();
            

            return returnValue;
        }

        private List<OtpResponseLog> SendMessageToUnknown(PhoneConfiguration phoneConfiguration, bool useControlDays)
        {
            var header = _headerManager.Get(phoneConfiguration, _data.ContentType);
            _requestLog.Content = header.BuildContentForLog(_data.Content);

            ConcurrentBag<OtpResponseLog> responses = new ConcurrentBag<OtpResponseLog>();

            Parallel.ForEach(operators, (currentElement) =>
            {
                IOperatorGateway gateway = (IOperatorGateway)_operatorRepository(currentElement.Value);
                gateway.SendOtp(_data.Phone, header.BuildContentForSms(_data.Content), responses, header, useControlDays);
            });

            return responses.ToList();
        }

        private OtpResponseLog SendMessageToKnown(PhoneConfiguration phoneConfiguration)
        {
            IOperatorGateway gateway = null;
             var header = _headerManager.Get(phoneConfiguration, _data.ContentType);
            _requestLog.Content = header.BuildContentForLog(_data.Content);

            switch (phoneConfiguration.Operator)
            {
                case OperatorType.Turkcell:
                    gateway = _operatorRepository(OperatorType.Turkcell);
                    break;
                case OperatorType.Vodafone:
                    gateway = _operatorRepository(OperatorType.Vodafone);
                    break;
                case OperatorType.TurkTelekom:
                    gateway = _operatorRepository(OperatorType.TurkTelekom);
                    break;
                case OperatorType.IVN:
                    gateway = _operatorRepository(OperatorType.IVN);
                    break;
                default:
                    // Serious Exception
                    break;
            }

            var result = gateway.SendOtp(_data.Phone, header.BuildContentForSms(_data.Content), header);

            return result;
        }

        private PhoneConfiguration createNewPhoneConfiguration()
        {
            var newConfig = new PhoneConfiguration
            {
                Phone = _data.Phone,
                Logs = new List<PhoneConfigurationLog>{
                    new PhoneConfigurationLog
                    {
                        Type = "Initialization",
                        Action = "Send Otp Request",
                        RelatedId = _requestLog.Id,
                        CreatedBy = _data.Process
                    }}
            };

            return newConfig;
        }

        private BlackListEntry createBlackListEntry(PhoneConfiguration phoneConfiguration,string reason,string source)
        {
            var newBlackListEntry = new BlackListEntry();
            newBlackListEntry.PhoneConfiguration = phoneConfiguration;
            newBlackListEntry.PhoneConfigurationId = phoneConfiguration.Id;
            newBlackListEntry.Reason = reason;
            newBlackListEntry.Source = source;
            newBlackListEntry.CreatedBy = _data.Process;
            newBlackListEntry.ValidTo = DateTime.Now;
            newBlackListEntry.Logs = new List<BlackListEntryLog>
            {
                new BlackListEntryLog
                {
                    Action = "Added To Blacklist",
                    BlackListEntry = newBlackListEntry,
                    CreatedBy = _data.Process,
                    ParameterMaster = "master",
                    ParameterSlave = "slave",
                    Type = "type"
                }
            };

            return newBlackListEntry;
        }
    }
}
