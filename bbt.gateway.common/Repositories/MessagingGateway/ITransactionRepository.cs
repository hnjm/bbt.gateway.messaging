using bbt.gateway.common.Models;

namespace bbt.gateway.common.Repositories
{
    public interface ITransactionRepository : IRepository<Transaction>
    {
        public (IEnumerable<Transaction>,int) GetOtpMessagesWithPhone(int countryCode, int prefix, int number, DateTime startDate, DateTime endDate, int page, int pageSize);
        public (IEnumerable<Transaction>, int) GetTransactionalSmsMessagesWithPhone(int countryCode, int prefix, int number, DateTime startDate, DateTime endDate, int page, int pageSize);
        public (IEnumerable<Transaction>, int) GetOtpMessagesWithCustomerNo(ulong customerNo, DateTime startDate, DateTime endDate, int page, int pageSize);
        public (IEnumerable<Transaction>, int) GetTransactionalSmsMessagesWithCustomerNo(ulong customerNo, DateTime startDate, DateTime endDate, int page, int pageSize);
        public (IEnumerable<Transaction>, int) GetOtpMessagesWithCitizenshipNo(string citizensipNo, DateTime startDate, DateTime endDate, int page, int pageSize);
        public (IEnumerable<Transaction>, int) GetTransactionalSmsMessagesWithCitizenshipNo(string citizensipNo, DateTime startDate, DateTime endDate, int page, int pageSize);

        public (IEnumerable<Transaction>, int) GetMailMessagesWithMail(string mail, DateTime startDate, DateTime endDate, int page, int pageSize);
        public (IEnumerable<Transaction>, int) GetMailMessagesWithCustomerNo(ulong customerNo, DateTime startDate, DateTime endDate, int page, int pageSize);
        public (IEnumerable<Transaction>, int) GetMailMessagesWithCitizenshipNo(string citizenshipNo, DateTime startDate, DateTime endDate, int page, int pageSize);

        public (IEnumerable<Transaction>, int) GetPushMessagesWithCustomerNo(ulong customerNo, DateTime startDate, DateTime endDate, int page, int pageSize);
        public (IEnumerable<Transaction>, int) GetPushMessagesWithCitizenshipNo(string citizenshipNo, DateTime startDate, DateTime endDate, int page, int pageSize);

        public Transaction GetWithId(Guid TxnId);
    }
}
