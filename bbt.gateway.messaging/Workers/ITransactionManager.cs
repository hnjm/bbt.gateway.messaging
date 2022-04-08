using bbt.gateway.common.Models;
using System;
using System.Threading.Tasks;

namespace bbt.gateway.messaging.Workers
{
    public interface ITransactionManager
    {
        public Guid TxnId { get; }
        public ulong CustomerNo { get; }
        public string BusinessLine { get; }
        public int BranchCode { get; }
        public OperatorType Operator { get; set; }
        public TransactionType TransactionType { get; set; }
        public Phone Phone { get; set; }

        public Task GetCustomerInfoByPhone(Phone Phone);
        public Task GetCustomerInfoByEmail(string Email);
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
