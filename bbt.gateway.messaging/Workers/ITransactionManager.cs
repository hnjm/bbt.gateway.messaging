using bbt.gateway.common.Models;
using System;
using System.Threading.Tasks;

namespace bbt.gateway.messaging.Workers
{
    public interface ITransactionManager
    {
        public Guid TxnId { get; }
        public string Ip { get; set; }

        public OtpRequestLog OtpRequestLog { get; set; }
        public SmsRequestLog SmsRequestLog { get; set; }
        public MailRequestLog MailRequestLog { get; set; }
        public OtpRequestInfo OtpRequestInfo { get; set; }
        public SmsRequestInfo SmsRequestInfo { get; set; }
        public MailRequestInfo MailRequestInfo { get; set; }
        public CustomerRequestInfo CustomerRequestInfo { get; set; }
        public TransactionType TransactionType { get; set; }

        public Task GetCustomerInfoByPhone(Phone Phone);
        public Task GetCustomerInfoByEmail(string Email);
        public Task GetCustomerInfoByCitizenshipNumber(string CitizenshipNumber);
        public Task GetCustomerInfoByCustomerNo(ulong CustomerNo);
        public void LogState();

        public void LogCritical(string message);
        public void LogWarning(string message);
        public void LogError(string message);
        public void LogDebug(string message);
        public void LogTrace(string message);
        public void LogInformation(string message);

        
    }
}
