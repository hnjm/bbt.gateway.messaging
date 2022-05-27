using bbt.gateway.common.Models;
using bbt.gateway.common.Repositories;
using System;
using System.Threading.Tasks;

namespace bbt.gateway.messaging.Workers
{
    public interface ITransactionManager
    {
        public Guid TxnId { get; }
        public Transaction Transaction { get; set; }
        public OtpRequestInfo OtpRequestInfo { get; set; }
        public SmsRequestInfo SmsRequestInfo { get; set; }
        public MailRequestInfo MailRequestInfo { get; set; }
        public PushRequestInfo PushRequestInfo { get; set; }
        public CustomerRequestInfo CustomerRequestInfo { get; set; }
        public bool UseFakeSmtp { get; set; }
        public void AddTransaction();
        public void SaveTransaction();
        public Task GetCustomerInfoByPhone();
        public Task GetCustomerInfoByEmail();
        public Task GetCustomerInfoByCitizenshipNumber();
        public Task GetCustomerInfoByCustomerNo();
        public void LogState();

        public void LogCritical(string message);
        public void LogWarning(string message);
        public void LogError(string message);
        public void LogDebug(string message);
        public void LogTrace(string message);
        public void LogInformation(string message);

        
    }
}
