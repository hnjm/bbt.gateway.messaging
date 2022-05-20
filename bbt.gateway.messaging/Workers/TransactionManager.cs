using bbt.gateway.common.Models;
using bbt.gateway.messaging.Api.Pusula;
using bbt.gateway.messaging.Api.Pusula.Model.GetByCitizenshipNumber;
using bbt.gateway.messaging.Api.Pusula.Model.GetByPhone;
using bbt.gateway.messaging.Api.Pusula.Model.GetCustomer;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Threading.Tasks;

namespace bbt.gateway.messaging.Workers
{
    public class TransactionManager : ITransactionManager
    {
        private readonly Guid _txnId;
        private Serilog.ILogger _logger;
        private readonly PusulaClient _pusulaClient;
        public TransactionType TransactionType { get; set; }

        public Guid TxnId { get { return _txnId; } }
        public string Ip { get; set; }
        public OtpRequestLog OtpRequestLog { get; set; }
        public SmsRequestLog SmsRequestLog { get; set; }
        public MailRequestLog MailRequestLog { get; set; }
        public PushNotificationRequestLog PushNotificationRequestLog { get; set; }

        public OtpRequestInfo OtpRequestInfo { get; set; } = new();
        public SmsRequestInfo SmsRequestInfo { get; set; } = new();
        public MailRequestInfo MailRequestInfo { get; set; } = new();
        public PushRequestInfo PushRequestInfo { get; set; } = new();

        public CustomerRequestInfo CustomerRequestInfo { get; set; } = new();
        public bool UseFakeSmtp { get; set; }
        public TransactionManager(ILogger<TransactionManager> logger, PusulaClient pusulaClient)
        {
            _txnId = Guid.NewGuid();
            _logger = Log.ForContext<TransactionManager>();
            _pusulaClient = pusulaClient;
            
        }

        public void LogState()
        {
            
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
                    SetCustomerRequestInfo(customerDetail);
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
                    SetCustomerRequestInfo(customerDetail);
                }
            }
        }

        public async Task GetCustomerInfoByCitizenshipNumber(string CitizenshipNumber)
        {
            var customer = await _pusulaClient.GetCustomerByCitizenshipNumber(new GetByCitizenshipNumberRequest()
            {
                CitizenshipNumber = CitizenshipNumber
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
                    SetCustomerRequestInfo(customerDetail);
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
                SetCustomerRequestInfo(customerDetail);
            }
        }

        private void SetCustomerRequestInfo(GetCustomerResponse customerDetail)
        {
            CustomerRequestInfo.BusinessLine = customerDetail.BusinessLine;
            CustomerRequestInfo.BranchCode = customerDetail.BranchCode;
            CustomerRequestInfo.MainPhone = customerDetail.MainPhone;
            CustomerRequestInfo.MainEmail = customerDetail.MainEmail;
        }

        public void LogCritical(string LogMessage)
        {
            _logger.Fatal("TxnId:" + _txnId + " | Type : " + TransactionType + " :" + LogMessage);
        }

        public void LogError(string LogMessage)
        {
            _logger.Error("TxnId:" + _txnId + " | Type : " + TransactionType + " :" + LogMessage);
        }

        public void LogDebug(string LogMessage)
        {
            _logger.Debug("TxnId:" + _txnId + " | Type : " + TransactionType + " :" + LogMessage);
        }

        public void LogTrace(string LogMessage)
        {
            _logger.Verbose("TxnId:" + _txnId + " | Type : " + TransactionType + " :" + LogMessage);
        }

        public void LogInformation(string LogMessage)
        {
            _logger.Information("TxnId:" + _txnId + " | Type : " + TransactionType + " :" + LogMessage);
        }

        public void LogWarning(string LogMessage)
        {
            _logger.Warning("TxnId:" + _txnId + " | Type : " + TransactionType + " :" + LogMessage);
        }
    }

    

    public class CustomerRequestInfo
    {
        public ulong? CustomerNo { get; set; }
        public string BusinessLine { get; set; }
        public int BranchCode { get; set; }
        public Phone? MainPhone { get; set; }
        public string MainEmail { get; set; }
        public string Tckn { get; set; }
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

    public class PushRequestInfo
    {
        public string ContactId { get; set; }
        public OperatorType Operator { get; set; }
        public string TemplateId { get; set; }
        public string TemplateParams { get; set; }
        public string CustomParameters { get; set; }
        public Process Process { get; set; }
    }

}
