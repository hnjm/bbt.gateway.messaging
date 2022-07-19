using bbt.gateway.common.Models;
using Microsoft.EntityFrameworkCore;

namespace bbt.gateway.common.Repositories
{
    public class TransactionRepository : Repository<Transaction>, ITransactionRepository
    {
        public TransactionRepository(DatabaseContext context) : base(context)
        {

        }

        public async Task<Transaction> GetWithIdAsync(Guid TxnId)
        {
            return await Context.Transactions.AsNoTracking()
                .Where(t => t.Id == TxnId)
                .Include(t => t.OtpRequestLog).ThenInclude(o => o.ResponseLogs).ThenInclude(o => o.TrackingLogs)
                .Include(t => t.SmsRequestLog).ThenInclude(s => s.ResponseLogs)
                .SingleOrDefaultAsync();
        }

        public async Task<(IEnumerable<Transaction>,int)> GetOtpMessagesWithPhoneAsync(int countryCode, int prefix, int number, DateTime startDate, DateTime endDate, int page, int pageSize)
        {
            IEnumerable<Transaction> list = await Context.Transactions.AsNoTracking()
               .Where(t => t.Phone.CountryCode == countryCode && t.Phone.Prefix == prefix && t.Phone.Number == number &&
                t.TransactionType == TransactionType.Otp && t.CreatedAt >= startDate && t.CreatedAt <= endDate)
               .Include(t => t.OtpRequestLog).ThenInclude(o => o.ResponseLogs).ThenInclude(o => o.TrackingLogs)
               .Include(t => t.OtpRequestLog.PhoneConfiguration)
               .OrderByDescending(t => t.CreatedAt)
                .Skip(page * pageSize)
                .Take(pageSize)
                .ToListAsync();

            int count = await Context.Transactions.AsNoTracking()
               .CountAsync(t => t.Phone.CountryCode == countryCode && t.Phone.Prefix == prefix && t.Phone.Number == number &&
                t.TransactionType == TransactionType.Otp && t.CreatedAt >= startDate && t.CreatedAt <= endDate);

            return (list, count);
                
        }

        public async Task<(IEnumerable<Transaction>,int)> GetOtpMessagesWithCustomerNoAsync(ulong customerNo, DateTime startDate, DateTime endDate, int page, int pageSize)
        {
            IEnumerable<Transaction> list = await Context.Transactions.AsNoTracking()
                .Where(t => t.CustomerNo == customerNo && t.TransactionType == TransactionType.Otp
                 && t.CreatedAt >= startDate && t.CreatedAt <= endDate)
                .Include(t => t.OtpRequestLog).ThenInclude(o => o.ResponseLogs).ThenInclude(o => o.TrackingLogs)
                .Include(t => t.OtpRequestLog.PhoneConfiguration)
                .OrderByDescending(t => t.CreatedAt)
                .Skip(page * pageSize)
                .Take(pageSize)
                .ToListAsync();

            int count = await Context.Transactions.AsNoTracking()
                .CountAsync(t => t.CustomerNo == customerNo && t.TransactionType == TransactionType.Otp
                 && t.CreatedAt >= startDate && t.CreatedAt <= endDate);

            return (list, count);

        }

        public async Task<(IEnumerable<Transaction>,int)> GetOtpMessagesWithCitizenshipNoAsync(string citizenshipNo, DateTime startDate, DateTime endDate, int page, int pageSize)
        {
            IEnumerable<Transaction> list = await Context.Transactions.AsNoTracking()
                .Where(t => t.CitizenshipNo == citizenshipNo && t.TransactionType == TransactionType.Otp
                 && t.CreatedAt >= startDate && t.CreatedAt <= endDate)
                .Include(t => t.OtpRequestLog).ThenInclude(o => o.ResponseLogs).ThenInclude(o => o.TrackingLogs)
                .Include(t => t.OtpRequestLog.PhoneConfiguration)
                .OrderByDescending(t => t.CreatedAt)
                .Skip(page * pageSize)
                .Take(pageSize)
                .ToListAsync();

            int count = await Context.Transactions.AsNoTracking()
                .CountAsync(t => t.CitizenshipNo == citizenshipNo && t.TransactionType == TransactionType.Otp
                 && t.CreatedAt >= startDate && t.CreatedAt <= endDate);

            return (list, count);
        }

        public async Task<(IEnumerable<Transaction>,int)> GetTransactionalSmsMessagesWithPhoneAsync(int countryCode, int prefix, int number, DateTime startDate, DateTime endDate, int page, int pageSize)
        {
            IEnumerable<Transaction> list = await Context.Transactions.AsNoTracking()
                .Where(t => t.Phone.CountryCode == countryCode && t.Phone.Prefix == prefix && t.Phone.Number == number && (t.TransactionType == TransactionType.TransactionalSms
                || t.TransactionType == TransactionType.TransactionalTemplatedSms)
                && t.CreatedAt >= startDate && t.CreatedAt < endDate)
                .Include(t => t.SmsRequestLog).ThenInclude(s => s.ResponseLogs)
                .OrderByDescending(t => t.CreatedAt)
                .Skip(page * pageSize)
                .Take(pageSize)
                .ToListAsync();

            int count = await Context.Transactions.AsNoTracking()
                .CountAsync(t => t.Phone.CountryCode == countryCode && t.Phone.Prefix == prefix && t.Phone.Number == number && (t.TransactionType == TransactionType.TransactionalSms
                || t.TransactionType == TransactionType.TransactionalTemplatedSms)
                && t.CreatedAt >= startDate && t.CreatedAt < endDate);

            return (list, count);
        }

        public async Task<(IEnumerable<Transaction>,int)> GetTransactionalSmsMessagesWithCustomerNoAsync(ulong customerNo, DateTime startDate, DateTime endDate, int page, int pageSize)
        {
            IEnumerable<Transaction> list = await Context.Transactions.AsNoTracking()
                .Where(t => t.CustomerNo == customerNo &&
                (t.TransactionType == TransactionType.TransactionalSms
                || t.TransactionType == TransactionType.TransactionalTemplatedSms)
                && t.CreatedAt >= startDate && t.CreatedAt < endDate)
                .Include(t => t.SmsRequestLog).ThenInclude(s => s.ResponseLogs)
                .OrderByDescending(t => t.CreatedAt)
                .Skip(page * pageSize)
                .Take(pageSize)
                .ToListAsync();

            int count = await Context.Transactions.AsNoTracking()
                .CountAsync(t => t.CustomerNo == customerNo &&
                (t.TransactionType == TransactionType.TransactionalSms
                || t.TransactionType == TransactionType.TransactionalTemplatedSms)
                && t.CreatedAt >= startDate && t.CreatedAt < endDate);

            return (list, count);
        }

        public async Task<(IEnumerable<Transaction>,int)> GetTransactionalSmsMessagesWithCitizenshipNoAsync(string citizenshipNo, DateTime startDate, DateTime endDate, int page, int pageSize)
        {
            IEnumerable<Transaction> list = await Context.Transactions.AsNoTracking()
                .Where(t => t.CitizenshipNo == citizenshipNo &&
                (t.TransactionType == TransactionType.TransactionalSms
                || t.TransactionType == TransactionType.TransactionalTemplatedSms)
                && t.CreatedAt >= startDate && t.CreatedAt < endDate)
                .Include(t => t.SmsRequestLog).ThenInclude(s => s.ResponseLogs)
                .OrderByDescending(t => t.CreatedAt)
                .Skip(page * pageSize)
                .Take(pageSize)
                .ToListAsync();


            int count = await Context.Transactions.AsNoTracking()
                .CountAsync(t => t.CitizenshipNo == citizenshipNo &&
                (t.TransactionType == TransactionType.TransactionalSms
                || t.TransactionType == TransactionType.TransactionalTemplatedSms)
                && t.CreatedAt >= startDate && t.CreatedAt < endDate);

            return (list, count);
        }

        public async Task<(IEnumerable<Transaction>, int)> GetMailMessagesWithMailAsync(string email, DateTime startDate, DateTime endDate, int page, int pageSize)
        {
            IEnumerable<Transaction> list = await Context.Transactions.AsNoTracking()
            .Where(t => t.Mail == email &&
                (t.TransactionType == TransactionType.TransactionalMail
                || t.TransactionType == TransactionType.TransactionalTemplatedMail) &&
                t.CreatedAt >= startDate && t.CreatedAt <= endDate)
            .Include(t => t.MailRequestLog).ThenInclude(s => s.ResponseLogs)
            .OrderByDescending(t => t.CreatedAt)
                .Skip(page * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var count = await Context.Transactions.AsNoTracking()
            .CountAsync(t => t.Mail == email &&
                (t.TransactionType == TransactionType.TransactionalMail
                || t.TransactionType == TransactionType.TransactionalTemplatedMail) &&
                t.CreatedAt >= startDate && t.CreatedAt <= endDate);

            return (list, count);
        }

        public async Task<(IEnumerable<Transaction>, int)> GetMailMessagesWithCustomerNoAsync(ulong customerNo, DateTime startDate, DateTime endDate, int page, int pageSize)
        {
            IEnumerable<Transaction> list = await Context.Transactions.AsNoTracking()
            .Where(t => t.CustomerNo == customerNo &&
            (t.TransactionType == TransactionType.TransactionalMail
                || t.TransactionType == TransactionType.TransactionalTemplatedMail) &&
            t.CreatedAt >= startDate && t.CreatedAt <= endDate)
            .Include(t => t.MailRequestLog).ThenInclude(s => s.ResponseLogs)
            .OrderByDescending(t => t.CreatedAt)
                .Skip(page * pageSize)
                .Take(pageSize)
                .ToListAsync();

            int count = await Context.Transactions.AsNoTracking()
            .CountAsync(t => t.CustomerNo == customerNo &&
            (t.TransactionType == TransactionType.TransactionalMail
                || t.TransactionType == TransactionType.TransactionalTemplatedMail) &&
            t.CreatedAt >= startDate && t.CreatedAt <= endDate);

            return (list, count);
        }
        public async Task<(IEnumerable<Transaction>, int)> GetMailMessagesWithCitizenshipNoAsync(string citizenshipNo, DateTime startDate, DateTime endDate, int page, int pageSize)
        {
            IEnumerable<Transaction> list = await Context.Transactions.AsNoTracking()
            .Where(t => t.CitizenshipNo == citizenshipNo &&
            (t.TransactionType == TransactionType.TransactionalMail
                || t.TransactionType == TransactionType.TransactionalTemplatedMail) &&
            t.CreatedAt >= startDate && t.CreatedAt <= endDate)
            .Include(t => t.MailRequestLog).ThenInclude(s => s.ResponseLogs)
            .OrderByDescending(t => t.CreatedAt)
                .Skip(page * pageSize)
                .Take(pageSize)
                .ToListAsync();

            int count = await Context.Transactions.AsNoTracking()
            .CountAsync(t => t.CitizenshipNo == citizenshipNo &&
            (t.TransactionType == TransactionType.TransactionalMail
                || t.TransactionType == TransactionType.TransactionalTemplatedMail) &&
            t.CreatedAt >= startDate && t.CreatedAt <= endDate);

            return (list, count);
        }

        public async Task<(IEnumerable<Transaction>, int)> GetPushMessagesWithCustomerNoAsync(ulong customerNo, DateTime startDate, DateTime endDate, int page, int pageSize)
        {
            IEnumerable<Transaction> list = await Context.Transactions.AsNoTracking()
                .Where(t => (t.CustomerNo == customerNo &&
                 (t.TransactionType == TransactionType.TransactionalPush || t.TransactionType == TransactionType.TransactionalTemplatedPush) &&
                 t.CreatedAt >= startDate && t.CreatedAt <= endDate
                 ))
                .Include(t => t.PushNotificationRequestLog).ThenInclude(s => s.ResponseLogs)
                .OrderByDescending(t => t.CreatedAt)
                .Skip(page * pageSize)
                .Take(pageSize)
                .ToListAsync();

            int count = await Context.Transactions.AsNoTracking()
                .CountAsync(t => (t.CustomerNo == customerNo &&
                 (t.TransactionType == TransactionType.TransactionalPush || t.TransactionType == TransactionType.TransactionalTemplatedPush) &&
                 t.CreatedAt >= startDate && t.CreatedAt <= endDate
                 ));

            return (list, count);
        }

        public async Task<(IEnumerable<Transaction>, int)> GetPushMessagesWithCitizenshipNoAsync(string citizenshipNo, DateTime startDate, DateTime endDate, int page, int pageSize)
        {
            IEnumerable<Transaction> list = await Context.Transactions.AsNoTracking()
                .Where(t => (t.CitizenshipNo == citizenshipNo &&
                (t.TransactionType == TransactionType.TransactionalPush || t.TransactionType == TransactionType.TransactionalTemplatedPush) &&
                t.CreatedAt >= startDate && t.CreatedAt <= endDate
                ))
                .Include(t => t.PushNotificationRequestLog).ThenInclude(s => s.ResponseLogs)
                .OrderByDescending(t => t.CreatedAt)
                .Skip(page * pageSize)
                .Take(pageSize)
                .ToListAsync();

            int count = await Context.Transactions.AsNoTracking()
                .CountAsync(t => (t.CitizenshipNo == citizenshipNo &&
                (t.TransactionType == TransactionType.TransactionalPush || t.TransactionType == TransactionType.TransactionalTemplatedPush) &&
                t.CreatedAt >= startDate && t.CreatedAt <= endDate
                ));

            return (list, count);

        }
    }
}
