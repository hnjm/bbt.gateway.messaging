using bbt.gateway.common.Models;
using System.Threading.Tasks;

namespace bbt.gateway.messaging.Workers
{
    public interface ITransactionManager
    {
        public ulong CustomerNo { get; }
        public string BusinessLine { get; }
        public int BranchCode { get; }

        public Task GetCustomerInfoByPhone(Phone Phone);
        public Task GetCustomerInfoByEmail(string Email);
        public Task GetCustomerInfoByCustomerNo(ulong CustomerNo);

        public void LogCritical(string message);
        public void LogWarning(string message);
        public void LogError(string message);
        public void LogDebug(string message);
        public void LogTrace(string message);
        public void LogInformation(string message);

        
    }
}
