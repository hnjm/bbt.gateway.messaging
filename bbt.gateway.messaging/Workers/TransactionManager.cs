﻿using bbt.gateway.common.Models;
using bbt.gateway.messaging.Api.Pusula;
using bbt.gateway.messaging.Api.Pusula.Model.GetByPhone;
using bbt.gateway.messaging.Api.Pusula.Model.GetCustomer;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace bbt.gateway.messaging.Workers
{
    public class TransactionManager : ITransactionManager
    {
        private readonly Guid _txnId;
        private ILogger<TransactionManager> _logger;
        private readonly PusulaClient _pusulaClient;

        public TransactionType TransactionType { get; set; }

        public Guid TxnId { get { return _txnId; } }

        public OtpRequestInfo OtpRequestInfo { get; set; } = new();
        public SmsRequestInfo SmsRequestInfo { get; set; } = new();
        public MailRequestInfo MailRequestInfo { get; set; } = new();
        public CustomerRequestInfo CustomerRequestInfo { get; set; } = new();

        public TransactionManager(ILogger<TransactionManager> logger, PusulaClient pusulaClient)
        {
            _txnId = Guid.NewGuid();
            _logger = logger;
            _pusulaClient = pusulaClient;
        }

        public void LogState()
        {
            var serializeOptions = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            };
            switch (TransactionType)
            {
                case TransactionType.Otp:
                    LogInformation(JsonConvert.SerializeObject(new { CustomerRequestInfo, OtpRequestInfo }));
                    break;
                case TransactionType.TransactionalSms:
                    LogInformation(JsonConvert.SerializeObject(new { CustomerRequestInfo, SmsRequestInfo }));
                    break;
                case TransactionType.TransactionalTemplatedSms:
                    LogInformation(JsonConvert.SerializeObject(new { CustomerRequestInfo, SmsRequestInfo }));
                    break;
                case TransactionType.TransactionalMail:
                    LogInformation(JsonConvert.SerializeObject(new { CustomerRequestInfo, MailRequestInfo }));
                    break;
                case TransactionType.TransactionalTemplatedMail:
                    LogInformation(JsonConvert.SerializeObject(new { CustomerRequestInfo, MailRequestInfo }));
                    break;
                case TransactionType.TransactionalPush:
                    LogInformation(JsonConvert.SerializeObject(this));
                    break;
                case TransactionType.TransactionalTemplatedPush:
                    LogInformation(JsonConvert.SerializeObject(this));
                    break;
                default:
                    break;
            }

        }

        public async Task GetCustomerInfoByPhone(Phone Phone)
        {
            var customer = await _pusulaClient.GetCustomerByPhoneNumber(new GetByPhoneNumberRequest()
            {
                CountryCode = Phone.CountryCode,
                CityCode = Phone.Prefix,
                TelephoneNumber = Phone.Number
            });

            if (customer.IsSuccess)
            {

                CustomerRequestInfo.CustomerNo = customer.CustomerNo;

                var customerDetail = await _pusulaClient.GetCustomer(new GetCustomerRequest()
                {
                    CustomerNo = customer.CustomerNo
                });

                if (customerDetail.IsSuccess)
                {
                    CustomerRequestInfo.BusinessLine = customerDetail.BusinessLine;
                    CustomerRequestInfo.BranchCode = customerDetail.BranchCode;
                }
            }

        }

        public async Task GetCustomerInfoByEmail(string Email)
        {
            var customer = await _pusulaClient.GetCustomerByEmail(new GetByEmailRequest()
            {
                Email = Email
            });

            if (customer.IsSuccess)
            {
                CustomerRequestInfo.CustomerNo = customer.CustomerNo;

                var customerDetail = await _pusulaClient.GetCustomer(new GetCustomerRequest()
                {
                    CustomerNo = customer.CustomerNo
                });

                if (customerDetail.IsSuccess)
                {
                    CustomerRequestInfo.BusinessLine = customerDetail.BusinessLine;
                    CustomerRequestInfo.BranchCode = customerDetail.BranchCode;
                }
            }
        }

        public async Task GetCustomerInfoByCustomerNo(ulong CustomerNo)
        {
            CustomerRequestInfo.CustomerNo = CustomerNo;

            var customerDetail = await _pusulaClient.GetCustomer(new GetCustomerRequest()
            {
                CustomerNo = CustomerNo
            });

            if (customerDetail.IsSuccess)
            {
                CustomerRequestInfo.BusinessLine = customerDetail.BusinessLine;
                CustomerRequestInfo.BranchCode = customerDetail.BranchCode;
            }
        }

        public void LogCritical(string LogMessage)
        {
            _logger.LogCritical("TxnId:" + _txnId + " | Type : " + TransactionType + " :" + LogMessage);
        }

        public void LogError(string LogMessage)
        {
            _logger.LogError("TxnId:" + _txnId + " | Type : " + TransactionType + " :" + LogMessage);
        }

        public void LogDebug(string LogMessage)
        {
            _logger.LogDebug("TxnId:" + _txnId + " | Type : " + TransactionType + " :" + LogMessage);
        }

        public void LogTrace(string LogMessage)
        {
            _logger.LogTrace("TxnId:" + _txnId + " | Type : " + TransactionType + " :" + LogMessage);
        }

        public void LogInformation(string LogMessage)
        {
            _logger.LogInformation("TxnId:" + _txnId + " | Type : " + TransactionType + " :" + LogMessage);
        }

        public void LogWarning(string LogMessage)
        {
            _logger.LogWarning("TxnId:" + _txnId + " | Type : " + TransactionType + " :" + LogMessage);
        }
    }

    

    public class CustomerRequestInfo
    {
        public ulong? CustomerNo { get; set; }
        public string BusinessLine { get; set; }
        public int BranchCode { get; set; }
    }

    public class OtpRequestInfo
    {
        public Phone Phone { get; set; }
        [JsonIgnore]
        public PhoneConfiguration PhoneConfiguration { get; set; }
        public OperatorType Operator { get; set; }
        public bool BlacklistCheck { get; set; }
        public string Content { get; set; }
        public Process Process { get; set; }
    }

    public class SmsRequestInfo
    {
        public Phone Phone { get; set; }
        [JsonIgnore]
        public PhoneConfiguration PhoneConfiguration { get; set; }
        public OperatorType Operator { get; set; }
        public string Content { get; set; }
        public string TemplateId { get; set; }
        public string TemplateParams { get; set; }
        public Process Process { get; set; }
    }

    public class MailRequestInfo
    {
        public string Email { get; set; }
        [JsonIgnore]
        public MailConfiguration MailConfiguration { get; set; }
        public OperatorType Operator { get; set; }
        public string Content { get; set; }
        public string TemplateId { get; set; }
        public string TemplateParams { get; set; }
        public Process Process { get; set; }
    }

}
