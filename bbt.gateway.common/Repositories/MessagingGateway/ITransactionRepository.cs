using bbt.gateway.common.Models;

namespace bbt.gateway.common.Repositories
{
    public interface ITransactionRepository : IRepository<Transaction>
    {
        public Task<(IEnumerable<Transaction>,int)> GetOtpMessagesWithPhoneAsync(int countryCode, int prefix, int number, DateTime startDate, DateTime endDate, int page, int pageSize);
        public Task<(IEnumerable<Transaction>, int)> GetTransactionalSmsMessagesWithPhoneAsync(int countryCode, int prefix, int number, DateTime startDate, DateTime endDate, int page, int pageSize);
        public Task<(IEnumerable<Transaction>, int)> GetOtpMessagesWithCustomerNoAsync(ulong customerNo, DateTime startDate, DateTime endDate, int page, int pageSize);
        public Task<(IEnumerable<Transaction>, int)> GetTransactionalSmsMessagesWithCustomerNoAsync(ulong customerNo, DateTime startDate, DateTime endDate, int page, int pageSize);
        public Task<(IEnumerable<Transaction>, int)> GetOtpMessagesWithCitizenshipNoAsync(string citizensipNo, DateTime startDate, DateTime endDate, int page, int pageSize);
        public Task<(IEnumerable<Transaction>, int)> GetTransactionalSmsMessagesWithCitizenshipNoAsync(string citizensipNo, DateTime startDate, DateTime endDate, int page, int pageSize);
        public Task<(IEnumerable<Transaction>, int)> GetMailMessagesWithMailAsync(string mail, DateTime startDate, DateTime endDate, int page, int pageSize);
        public Task<(IEnumerable<Transaction>, int)> GetMailMessagesWithCustomerNoAsync(ulong customerNo, DateTime startDate, DateTime endDate, int page, int pageSize);
        public Task<(IEnumerable<Transaction>, int)> GetMailMessagesWithCitizenshipNoAsync(string citizenshipNo, DateTime startDate, DateTime endDate, int page, int pageSize);
        public Task<(IEnumerable<Transaction>, int)> GetPushMessagesWithCustomerNoAsync(ulong customerNo, DateTime startDate, DateTime endDate, int page, int pageSize);
        public Task<(IEnumerable<Transaction>, int)> GetPushMessagesWithCitizenshipNoAsync(string citizenshipNo, DateTime startDate, DateTime endDate, int page, int pageSize);
        public Task<IEnumerable<Transaction>> GetReportTransaction(int phoneNumber,string date,string message);

        public Task<Transaction> GetWithIdAsync(Guid TxnId);
        public Task<Transaction> GetWithIdAsNoTrackingAsync(Guid TxnId);
    }
}
