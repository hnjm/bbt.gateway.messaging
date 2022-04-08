using bbt.gateway.common.Models;
using bbt.gateway.messaging.Api.Pusula;
using bbt.gateway.messaging.Api.Pusula.Model.GetByPhone;
using bbt.gateway.messaging.Api.Pusula.Model.GetCustomer;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Threading.Tasks;

namespace bbt.gateway.messaging.Workers
{
    public class TransactionManager : ITransactionManager
    {
        private readonly Guid _txnId;
        private ILogger<TransactionManager> _logger;
        private readonly PusulaClient _pusulaClient;

        private ulong _customerNo;
        private string _businessLine;
        private int _branchCode;

        public TransactionType TransactionType { get; set; }
        public ulong CustomerNo { get { return _customerNo; } }
        public string BusinessLine { get { return _businessLine; } }
        public int BranchCode { get { return _branchCode; } }
        public OperatorType Operator { get; set; }
        public Phone Phone { get; set; }

        public Guid TxnId {get {return _txnId;}}

        public TransactionManager(ILogger<TransactionManager> logger, PusulaClient pusulaClient)
        {
            _txnId = Guid.NewGuid();
            _logger = logger;
            _pusulaClient = pusulaClient;
        }

        public void LogState()
        {
            LogInformation(JsonConvert.SerializeObject(this));
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
                _customerNo = customer.CustomerNo;

                var customerDetail = await _pusulaClient.GetCustomer(new GetCustomerRequest()
                {
                    CustomerNo = _customerNo
                });

                if (customerDetail.IsSuccess)
                {
                    _businessLine = customerDetail.BusinessLine;
                    _branchCode = customerDetail.BranchCode;
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
                _customerNo = customer.CustomerNo;

                var customerDetail = await _pusulaClient.GetCustomer(new GetCustomerRequest()
                {
                    CustomerNo = _customerNo
                });

                if (customerDetail.IsSuccess)
                {
                    _businessLine = customerDetail.BusinessLine;
                    _branchCode = customerDetail.BranchCode;
                }
            }
        }

        public async Task GetCustomerInfoByCustomerNo(ulong CustomerNo)
        {
            _customerNo = CustomerNo;

            var customerDetail = await _pusulaClient.GetCustomer(new GetCustomerRequest()
            {
                CustomerNo = _customerNo
            });

            if (customerDetail.IsSuccess)
            {
                _businessLine = customerDetail.BusinessLine;
                _branchCode = customerDetail.BranchCode;
            }
        }

        public void LogCritical(string LogMessage)
        {
            _logger.LogCritical("TxnId:"+_txnId + " " + LogMessage);             
        }

        public void LogError(string LogMessage)
        {
            _logger.LogError("TxnId:" + _txnId + " " + LogMessage);
        }

        public void LogDebug(string LogMessage)
        {
            _logger.LogDebug("TxnId:" + _txnId + " " + LogMessage);
        }

        public void LogTrace(string LogMessage)
        {
            _logger.LogTrace("TxnId:" + _txnId + " " + LogMessage);
        }

        public void LogInformation(string LogMessage)
        {
            _logger.LogInformation("TxnId:" + _txnId + " " + LogMessage);
        }

        public void LogWarning(string LogMessage)
        {
            _logger.LogWarning("TxnId:" + _txnId + " " + LogMessage);
        }
    }

    public enum TransactionType
    {
        Otp,
        TransactionalSms,
        TransactionalMail,
        TransactionalPush,
        BulkSms,
        BulkPush,
        BulkMail
    }

}
