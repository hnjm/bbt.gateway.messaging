using bbt.gateway.common.Models;
using bbt.gateway.common.Repositories;
using bbt.gateway.messaging.Workers.OperatorGateway;
using bbt.gateway.common;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace bbt.gateway.messaging.Workers
{
    public class OtpSender
    {
        private readonly HeaderManager _headerManager;
        private readonly Func<OperatorType, IOperatorGateway> _operatorRepository;
        private readonly IRepositoryManager _repositoryManager;

        SendMessageSmsRequest _data;
        SendSmsResponseStatus returnValue;
        OtpRequestLog _requestLog;
        PhoneConfiguration phoneConfiguration;

        //Type[] operators = new Type[] { typeof(OperatorTurkcell), typeof(OperatorVodafone), typeof(OperatorTurkTelekom) };
        private readonly Dictionary<Type, OperatorType> operators = new Dictionary<Type, OperatorType>()
        {
            { typeof(OperatorTurkcell) , OperatorType.Turkcell},
            { typeof(OperatorVodafone) , OperatorType.Vodafone},
            { typeof(OperatorTurkTelekom) , OperatorType.TurkTelekom},
            { typeof(OperatorIVN) , OperatorType.IVN}
        };
        public OtpSender(HeaderManager headerManager,
            Func<OperatorType, IOperatorGateway> operatorRepository,
            IRepositoryManager repositoryManager)
        {
            _headerManager = headerManager;
            _operatorRepository = operatorRepository;
            _repositoryManager = repositoryManager;
        }

        public async Task<SendSmsResponseStatus> SendMessage(SendMessageSmsRequest sendMessageSmsRequest)
        {
            returnValue = SendSmsResponseStatus.ClientError;

            _data = sendMessageSmsRequest;
            _requestLog = new OtpRequestLog
            {
                CreatedBy = _data.Process,
                Phone = _data.Phone
            };

            // Load Phone configuration and related active blacklist entiries.
            phoneConfiguration = _repositoryManager.PhoneConfigurations.GetWithBlacklistEntires(
                _data.Phone.CountryCode, _data.Phone.Prefix, _data.Phone.Number, DateTime.Now);

                // if known number without any blacklist entry 
                if (
                phoneConfiguration != null &&
                phoneConfiguration.Operator != null &&
                !phoneConfiguration.BlacklistEntries.Any(b => b.Status == BlacklistStatus.NotResolved)
            )
            {
                //TODO: Operator ghonderemezse? Sim değişmişse?
                var responseLog = await SendMessageToKnown(phoneConfiguration,true);
                _requestLog.ResponseLogs.Add(responseLog);

                //Operator Change | Sim Change | Not Subscriber handle edilmeli
                if (responseLog.ResponseCode == SendSmsResponseStatus.NotSubscriber)
                {
                    await SendMessageToUnknownProcess(true);
                }
                if (responseLog.ResponseCode == SendSmsResponseStatus.OperatorChange
                    || responseLog.ResponseCode == SendSmsResponseStatus.SimChange)
                {
                    var blackListEntry = createBlackListEntry(phoneConfiguration, returnValue.ToString(), "SendMessageToUnknownProcess");
                    _repositoryManager.BlackListEntries.Add(blackListEntry);
                }

                
                returnValue = responseLog.ResponseCode;
            }
            else
            {
                if (phoneConfiguration != null &&
                    phoneConfiguration.BlacklistEntries.Any(b => b.Status == BlacklistStatus.NotResolved && b.ValidTo > DateTime.Today))
                {
                    returnValue = SendSmsResponseStatus.HasBlacklistRecord;
                    _requestLog.ResponseLogs.Add(new OtpResponseLog { 
                        Operator = (OperatorType)phoneConfiguration.Operator,
                        ResponseCode = SendSmsResponseStatus.HasBlacklistRecord                      
                    });
                }
                else
                {
                    //If configuration is not available then create clean phone configuration to phone number   
                    if (phoneConfiguration == null)
                    {
                        phoneConfiguration = createNewPhoneConfiguration();
                        _repositoryManager.PhoneConfigurations.Add(phoneConfiguration);
                    }

                    await SendMessageToUnknownProcess(false);
                }

            }

            _requestLog.PhoneConfiguration = phoneConfiguration;

            _repositoryManager.OtpRequestLogs.Add(_requestLog);
            _repositoryManager.SaveChanges();
            

            return returnValue;
        }

        private async Task SendMessageToUnknownProcess(bool discardCurrentOperator)
        {

            //if discardCurrentOperator is true,phone is known number
            var responseLogs = await SendMessageToUnknown(phoneConfiguration, discardCurrentOperator, discardCurrentOperator);

            // Decide which response code will be returned
            returnValue = responseLogs.UnifyResponse();

            //TODO: Blackliste eklenmesi gerekiyorsa ekle.
            if (returnValue == SendSmsResponseStatus.OperatorChange || returnValue == SendSmsResponseStatus.SimChange)
            {
                var blackListEntry = createBlackListEntry(phoneConfiguration, returnValue.ToString(), "SendMessageToUnknownProcess");
                _repositoryManager.BlackListEntries.Add(blackListEntry);
            }


            // Update with valid operator if any otp sending 
            var successAttempt = responseLogs.FirstOrDefault(l => (l.ResponseCode == SendSmsResponseStatus.Success 
            || l.ResponseCode == SendSmsResponseStatus.OperatorChange
            || l.ResponseCode == SendSmsResponseStatus.SimChange ));
            if (successAttempt != null)
                phoneConfiguration.Operator = successAttempt.Operator;

            // Add all response logs to request log
            responseLogs.ForEach(l => _requestLog.ResponseLogs.Add(l));

        }

        private async Task<List<OtpResponseLog>> SendMessageToUnknown(PhoneConfiguration phoneConfiguration,bool useControlDays, bool discardCurrentOperator = false)
        {
            var header = await _headerManager.Get(phoneConfiguration, _data.ContentType);
            _requestLog.Content = header.BuildContentForLog(_data.Content);

            ConcurrentBag<OtpResponseLog> responses = new ConcurrentBag<OtpResponseLog>();
            List<Task> tasks = new List<Task>();
            foreach (var currentElement in operators)
            {
                if (discardCurrentOperator)
                {
                    if (phoneConfiguration.Operator != currentElement.Value)
                    {
                        IOperatorGateway gateway = _operatorRepository(currentElement.Value);
                        tasks.Add(gateway.SendOtp(_data.Phone, header.BuildContentForSms(_data.Content), responses, header, useControlDays));
                    }
                }
                else
                {
                    IOperatorGateway gateway = _operatorRepository(currentElement.Value);
                    tasks.Add(gateway.SendOtp(_data.Phone, header.BuildContentForSms(_data.Content), responses, header, useControlDays));
                }
            }
            
            await Task.WhenAll(tasks);
            return responses.ToList();
        }

        private async Task<OtpResponseLog> SendMessageToKnown(PhoneConfiguration phoneConfiguration,bool useControlDays)
        {
            IOperatorGateway gateway = null;
             var header = await _headerManager.Get(phoneConfiguration, _data.ContentType);
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

            var result = await gateway.SendOtp(_data.Phone, header.BuildContentForSms(_data.Content), header,useControlDays);

            return result;
        }

        public async Task<OtpTrackingLog> CheckMessage(CheckSmsRequest checkSmsRequest)
        {
            IOperatorGateway gateway = null;

            switch (checkSmsRequest.Operator)
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

            var result = await gateway.CheckMessageStatus(checkSmsRequest);

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
