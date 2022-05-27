using bbt.gateway.common.Models;
using bbt.gateway.common.Repositories;
using bbt.gateway.messaging.Workers.OperatorGateway;
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
        private readonly ITransactionManager _transactionManager;

        SendMessageSmsRequest _data;
        SendSmsResponseStatus returnValue;
        OtpRequestLog _requestLog;
        PhoneConfiguration phoneConfiguration;

        private readonly Dictionary<Type, OperatorType> operators = new Dictionary<Type, OperatorType>()
        {
            { typeof(OperatorTurkcell) , OperatorType.Turkcell},
            { typeof(OperatorVodafone) , OperatorType.Vodafone},
            { typeof(OperatorTurkTelekom) , OperatorType.TurkTelekom},
            { typeof(OperatorIVN) , OperatorType.IVN}
        };
        public OtpSender(HeaderManager headerManager,
            Func<OperatorType, IOperatorGateway> operatorRepository,
            IRepositoryManager repositoryManager,
            ITransactionManager transactionManager)
        {
            _headerManager = headerManager;
            _operatorRepository = operatorRepository;
            _repositoryManager = repositoryManager;
            _transactionManager = transactionManager;
        }

        public async Task<SendSmsOtpResponse> SendMessage(SendMessageSmsRequest sendMessageSmsRequest)
        {
            SendSmsOtpResponse sendSmsOtpResponse = new() { 
                TxnId = _transactionManager.TxnId
            };
            //Set returnValue ClientError for Unexpected Errors
            returnValue = SendSmsResponseStatus.ClientError;

            //Set Request Body To Class Variable
            _data = sendMessageSmsRequest;

            //Turkish Character Conversion And Length Validation
            _data.Content = _data.Content.ConvertToTurkish();
            if(_data.Content.Length > 160)
            {
                _transactionManager.LogError("Otp Maximum Characters Count Exceed");
                returnValue = SendSmsResponseStatus.MaximumCharactersCountExceed;
                sendSmsOtpResponse.Status = returnValue;
                return sendSmsOtpResponse;
            }

            _requestLog = new OtpRequestLog
            {
                CreatedBy = _data.Process,
                Phone = _data.Phone
            };

            // Load Phone configuration and related active blacklist entiries.
            phoneConfiguration = _repositoryManager.PhoneConfigurations.GetWithBlacklistEntires(
                _data.Phone.CountryCode, _data.Phone.Prefix, _data.Phone.Number, DateTime.Now);

            //Get blacklist records from current Otp System
            var oldBlacklistRecord = _repositoryManager.DirectBlacklists.Find(b => b.GsmNumber == _data.Phone.ToString()).OrderByDescending(b => b.RecordDate).FirstOrDefault();

            if (oldBlacklistRecord != null)
            {
                var blackListRecord = phoneConfiguration.BlacklistEntries.FirstOrDefault(b => b.SmsId == oldBlacklistRecord.SmsId);
                if (blackListRecord != null)
                {
                    if (blackListRecord.Status == BlacklistStatus.NotResolved && oldBlacklistRecord.IsVerified)
                    {
                        //Resolve Blacklist entry
                        blackListRecord.Status = BlacklistStatus.Resolved;
                        blackListRecord.ResolvedAt = oldBlacklistRecord.VerifyDate;
                        blackListRecord.ResolvedBy = new Process { Name = oldBlacklistRecord.VerifiedBy };
                        _repositoryManager.SaveChanges();
                    }
                    if (blackListRecord.Status == BlacklistStatus.NotResolved && !oldBlacklistRecord.IsVerified)
                    {
                        oldBlacklistRecord.TryCount++;
                        _repositoryManager.SaveSmsBankingChanges();
                    }
                }
                else
                {
                    if (!oldBlacklistRecord.IsVerified)
                    {

                        //Increase Try Count
                        oldBlacklistRecord.TryCount++;
                        _repositoryManager.SaveSmsBankingChanges();

                        //Insert Blacklist entry
                        var blacklistEntry = createBlackListEntry(phoneConfiguration, returnValue.ToString(), "SendMessageToKnownProcess", oldBlacklistRecord.SmsId);
                        phoneConfiguration.BlacklistEntries.Add(blacklistEntry);
                        _repositoryManager.BlackListEntries.Add(blacklistEntry);
                    }
                }
            }

            // if known number without any blacklist entry 
            if (
            phoneConfiguration != null &&
            phoneConfiguration.Operator != null &&
            !phoneConfiguration.BlacklistEntries.Any(b => b.Status == BlacklistStatus.NotResolved)
            )
            {

                var responseLog = await SendMessageToKnown(phoneConfiguration,true);
                _requestLog.ResponseLogs.Add(responseLog);

                if (responseLog.ResponseCode == SendSmsResponseStatus.OperatorChange
                    || responseLog.ResponseCode == SendSmsResponseStatus.SimChange)
                {
                    _transactionManager.LogError("OperatorChange or SimChange Has Occured");

                    //Add to Blacklist If Not Exists
                    if (!phoneConfiguration.BlacklistEntries.Any(b => b.Status == BlacklistStatus.NotResolved && b.ValidTo > DateTime.Today))
                    {
                        _transactionManager.LogError("Phone has a blacklist record");
                        var oldBlackListEntry = createOldBlackListEntry(_headerManager.CustomerNo, phoneConfiguration.Phone.ToString());
                        _repositoryManager.DirectBlacklists.Add(oldBlackListEntry);
                        _repositoryManager.SaveSmsBankingChanges();

                        var blackListEntry = createBlackListEntry(phoneConfiguration, returnValue.ToString(), "SendMessageToKnownProcess",oldBlackListEntry.SmsId);
                        _repositoryManager.BlackListEntries.Add(blackListEntry);
                    }
                }
                returnValue = responseLog.ResponseCode;
                
                //Operator Change | Sim Change | Not Subscriber handle edilmeli
                if (responseLog.ResponseCode == SendSmsResponseStatus.NotSubscriber)
                {
                    _transactionManager.LogError("Known Number Changed Operator");

                    //Known Number Returns Not Subscriber For Related Operator
                    //Try to Send Sms With Another Operators
                    //Should pass true for discarding current operator
                    await SendMessageToUnknownProcess(true);
                }                
            }
            else
            {
                //Known Number With Active Blacklist Entry
                if (phoneConfiguration != null &&
                    phoneConfiguration.BlacklistEntries.Any(b => b.Status == BlacklistStatus.NotResolved && b.ValidTo > DateTime.Today))
                {
                    _transactionManager.LogError("Phone has a blacklist record");
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
            _transactionManager.Transaction.OtpRequestLog = _requestLog;

            sendSmsOtpResponse.Status = returnValue;
            return sendSmsOtpResponse;
        }

        private async Task SendMessageToUnknownProcess(bool discardCurrentOperator)
        {

            //if discardCurrentOperator is true,phone is known number
            var responseLogs = await SendMessageToUnknown(phoneConfiguration, discardCurrentOperator, discardCurrentOperator);

            // Decide which response code will be returned
            returnValue = responseLogs.UnifyResponse();

            //Blackliste eklenmesi gerekiyorsa ekle.
            if (returnValue == SendSmsResponseStatus.OperatorChange || returnValue == SendSmsResponseStatus.SimChange)
            {
                _transactionManager.LogError("OperatorChange or SimChange Has Occured");
                if (phoneConfiguration.BlacklistEntries.All(b => b.Status == BlacklistStatus.Resolved))
                {
                    var oldBlackListEntry = createOldBlackListEntry(_headerManager.CustomerNo, phoneConfiguration.Phone.ToString());
                    _repositoryManager.DirectBlacklists.Add(oldBlackListEntry);
                    _repositoryManager.SaveSmsBankingChanges();

                    var blackListEntry = createBlackListEntry(phoneConfiguration, returnValue.ToString(), "SendMessageToUnknownProcess", oldBlackListEntry.SmsId);
                    _repositoryManager.BlackListEntries.Add(blackListEntry);
                }
            }


            // Update with valid operator if any otp sending 
            var successAttempt = responseLogs.FirstOrDefault(l => (l.ResponseCode == SendSmsResponseStatus.Success 
            || l.ResponseCode == SendSmsResponseStatus.OperatorChange
            || l.ResponseCode == SendSmsResponseStatus.SimChange ));

            if (successAttempt != null)
            {
                _transactionManager.OtpRequestInfo.Operator = successAttempt.Operator;
                phoneConfiguration.Operator = successAttempt.Operator;
                _transactionManager.OtpRequestInfo.PhoneConfiguration = phoneConfiguration;
            }

            // Add all response logs to request log
            responseLogs.ForEach(l => _requestLog.ResponseLogs.Add(l));

        }

        private async Task<List<OtpResponseLog>> SendMessageToUnknown(PhoneConfiguration phoneConfiguration,bool useControlDays,bool discardCurrentOperator = false)
        {
            var header =  _headerManager.Get(_data.ContentType,_data.HeaderInfo);
            _requestLog.Content = header.BuildContentForLog(_data.Content);

            ConcurrentBag<OtpResponseLog> responses = new ConcurrentBag<OtpResponseLog>();
            List<Task> tasks = new List<Task>();
            if (_data.Phone.CountryCode == 90)
            {
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
            }
            else 
            {
                IOperatorGateway gateway = _operatorRepository(OperatorType.Turkcell);
                tasks.Add(gateway.SendOtp(_data.Phone, header.BuildContentForSms(_data.Content), responses, header, useControlDays));
            }

            await Task.WhenAll(tasks);
            return responses.ToList();
        }

        private async Task<OtpResponseLog> SendMessageToKnown(PhoneConfiguration phoneConfiguration,bool useControlDays)
        {
            _transactionManager.OtpRequestInfo.Operator = phoneConfiguration.Operator.Value;

            IOperatorGateway gateway = null;
            var header =  _headerManager.Get(_data.ContentType, _data.HeaderInfo);
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

        private BlackListEntry createBlackListEntry(PhoneConfiguration phoneConfiguration,string reason,string source,long smsId)
        {
            var newBlackListEntry = new BlackListEntry();
            newBlackListEntry.PhoneConfiguration = phoneConfiguration;
            newBlackListEntry.PhoneConfigurationId = phoneConfiguration.Id;
            newBlackListEntry.Reason = reason;
            newBlackListEntry.Source = source;
            newBlackListEntry.CreatedBy = _data.Process;
            newBlackListEntry.ValidTo = DateTime.Now.AddMonths(1);
            newBlackListEntry.SmsId = smsId;
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

        private SmsDirectBlacklist createOldBlackListEntry(long customerNo,string phone)
        {
            var newOldBlackListEntry = new SmsDirectBlacklist
            {
                CustomerNo = customerNo,
                GsmNumber = phone,
                RecordDate = DateTime.Now,
                IsVerified = false,
                TryCount = 0,
                Explanation = "Messaging Gateway tarafından eklendi."
            };

            return newOldBlackListEntry;
        }
    }
}
