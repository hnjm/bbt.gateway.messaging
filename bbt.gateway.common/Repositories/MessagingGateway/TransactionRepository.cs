using bbt.gateway.common.Models;
using Microsoft.EntityFrameworkCore;

namespace bbt.gateway.common.Repositories
{
    public class TransactionRepository : Repository<Transaction>, ITransactionRepository
    {
        public TransactionRepository(DatabaseContext context) : base(context)
        {

        }

        public Transaction GetWithId(Guid TxnId)
        {
            return Context.Transactions
                .Where(t => t.Id == TxnId)
                .Include(t => t.OtpRequestLog).ThenInclude(o => o.ResponseLogs).ThenInclude(o => o.TrackingLogs)
                .Include(t => t.SmsRequestLog).ThenInclude(s => s.ResponseLogs)
                .SingleOrDefault();
        }

        public IEnumerable<Transaction> GetWithPhone(int countryCode, int prefix, int number, int page, int pageSize)
        {
            return Context.Transactions
                .Where(t => (t.TransactionType == TransactionType.Otp || t.TransactionType == TransactionType.TransactionalSms
                || t.TransactionType == TransactionType.TransactionalTemplatedSms) && t.Phone.CountryCode == countryCode && t.Phone.Prefix == prefix && t.Phone.Number == number)
                .Include(t => t.OtpRequestLog).ThenInclude(o => o.ResponseLogs).ThenInclude(o => o.TrackingLogs)
                .Include(t => t.SmsRequestLog).ThenInclude(s => s.ResponseLogs)
                .OrderByDescending(t => t.CreatedAt)
                .Skip(page * pageSize)
                .Take(pageSize);
        }
    }
}
