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

        public (IEnumerable<Transaction>,int) GetOtpMessagesWithPhone(int countryCode, int prefix, int number, DateTime startDate, DateTime endDate, int page, int pageSize)
        {
            IEnumerable<Transaction> list = Context.Transactions
               .Where(t => (t.TransactionType == TransactionType.Otp) && t.Phone.CountryCode == countryCode && t.Phone.Prefix == prefix && t.Phone.Number == number
                && t.CreatedAt >= startDate && t.CreatedAt <= endDate)
               .Include(t => t.OtpRequestLog).ThenInclude(o => o.ResponseLogs).ThenInclude(o => o.TrackingLogs)
               .OrderByDescending(t => t.CreatedAt)
                .Skip(page * pageSize)
                .Take(pageSize);

            var count = list.Count();
            return (list.OrderByDescending(t => t.CreatedAt)
                .Skip(page * pageSize)
                .Take(pageSize), count);
                
        }

        public (IEnumerable<Transaction>,int) GetOtpMessagesWithCustomerNo(ulong customerNo, DateTime startDate, DateTime endDate, int page, int pageSize)
        {
            IEnumerable<Transaction> list = Context.Transactions
                .Where(t => t.TransactionType == TransactionType.Otp && t.CustomerNo == customerNo
                 && t.CreatedAt >= startDate && t.CreatedAt <= endDate)
                .Include(t => t.OtpRequestLog).ThenInclude(o => o.ResponseLogs).ThenInclude(o => o.TrackingLogs);

            int count = Context.Transactions
                .Count(t => t.TransactionType == TransactionType.Otp && t.CustomerNo == customerNo
                 && t.CreatedAt >= startDate && t.CreatedAt <= endDate);

            return (list.OrderByDescending(t => t.CreatedAt)
                .Skip(page * pageSize)
                .Take(pageSize), count);

        }

        public (IEnumerable<Transaction>,int) GetOtpMessagesWithCitizenshipNo(string citizenshipNo, DateTime startDate, DateTime endDate, int page, int pageSize)
        {
            IEnumerable<Transaction> list = Context.Transactions
                .Where(t => t.TransactionType == TransactionType.Otp && t.CitizenshipNo == citizenshipNo
                 && t.CreatedAt >= startDate && t.CreatedAt <= endDate)
                .Include(t => t.OtpRequestLog).ThenInclude(o => o.ResponseLogs).ThenInclude(o => o.TrackingLogs)
                .OrderByDescending(t => t.CreatedAt)
                .Skip(page * pageSize)
                .Take(pageSize);

            int count = Context.Transactions
                .Count(t => t.TransactionType == TransactionType.Otp && t.CitizenshipNo == citizenshipNo
                 && t.CreatedAt >= startDate && t.CreatedAt <= endDate);

            return (list, count);
        }

        public (IEnumerable<Transaction>,int) GetTransactionalSmsMessagesWithPhone(int countryCode, int prefix, int number, DateTime startDate, DateTime endDate, int page, int pageSize)
        {
            IEnumerable<Transaction> list = Context.Transactions
                .Where(t => (t.TransactionType == TransactionType.TransactionalSms
                || t.TransactionType == TransactionType.TransactionalTemplatedSms) && t.Phone.CountryCode == countryCode && t.Phone.Prefix == prefix && t.Phone.Number == number
                && t.CreatedAt >= startDate && t.CreatedAt < endDate)
                .Include(t => t.SmsRequestLog).ThenInclude(s => s.ResponseLogs);

            var count = list.Count();
            return (list.OrderByDescending(t => t.CreatedAt)
                .Skip(page * pageSize)
                .Take(pageSize), count);
        }

        public (IEnumerable<Transaction>,int) GetTransactionalSmsMessagesWithCustomerNo(ulong customerNo, DateTime startDate, DateTime endDate, int page, int pageSize)
        {
            IEnumerable<Transaction> list = Context.Transactions
                .Where(t => (t.TransactionType == TransactionType.TransactionalSms
                || t.TransactionType == TransactionType.TransactionalTemplatedSms) && t.CustomerNo == customerNo
                && t.CreatedAt >= startDate && t.CreatedAt < endDate)
                .Include(t => t.SmsRequestLog).ThenInclude(s => s.ResponseLogs)
                .OrderByDescending(t => t.CreatedAt)
                .Skip(page * pageSize)
                .Take(pageSize);

            int count = Context.Transactions
                .Count(t => (t.TransactionType == TransactionType.TransactionalSms
                || t.TransactionType == TransactionType.TransactionalTemplatedSms) && t.CustomerNo == customerNo
                && t.CreatedAt >= startDate && t.CreatedAt < endDate);

            return (list, count);
        }

        public (IEnumerable<Transaction>,int) GetTransactionalSmsMessagesWithCitizenshipNo(string citizenshipNo, DateTime startDate, DateTime endDate, int page, int pageSize)
        {
            IEnumerable<Transaction> list = Context.Transactions
                .Where(t => (t.TransactionType == TransactionType.TransactionalSms
                || t.TransactionType == TransactionType.TransactionalTemplatedSms) && t.CitizenshipNo == citizenshipNo
                && t.CreatedAt >= startDate && t.CreatedAt < endDate)
                .Include(t => t.SmsRequestLog).ThenInclude(s => s.ResponseLogs)
                .OrderByDescending(t => t.CreatedAt)
                .Skip(page * pageSize)
                .Take(pageSize);


            int count = Context.Transactions
                .Count(t => (t.TransactionType == TransactionType.TransactionalSms
                || t.TransactionType == TransactionType.TransactionalTemplatedSms) && t.CitizenshipNo == citizenshipNo
                && t.CreatedAt >= startDate && t.CreatedAt < endDate);

            return (list, count);
        }

        public (IEnumerable<Transaction>, int) GetMailMessagesWithMail(string email, DateTime startDate, DateTime endDate, int page, int pageSize)
        {
            IEnumerable<Transaction> list = Context.Transactions
            .Where(t => (t.TransactionType == TransactionType.TransactionalMail
                || t.TransactionType == TransactionType.TransactionalTemplatedMail) && t.Mail == email && t.CreatedAt >= startDate && t.CreatedAt <= endDate)
            .Include(t => t.MailRequestLog).ThenInclude(s => s.ResponseLogs)
            .OrderByDescending(t => t.CreatedAt)
                .Skip(page * pageSize)
                .Take(pageSize);

            var count = Context.Transactions
            .Count(t => (t.TransactionType == TransactionType.TransactionalMail
                || t.TransactionType == TransactionType.TransactionalTemplatedMail) && t.Mail == email && t.CreatedAt >= startDate && t.CreatedAt <= endDate);

            return (list, count);
        }

        public (IEnumerable<Transaction>, int) GetMailMessagesWithCustomerNo(ulong customerNo, DateTime startDate, DateTime endDate, int page, int pageSize)
        {
            IEnumerable<Transaction> list = Context.Transactions
            .Where(t => (t.TransactionType == TransactionType.TransactionalMail
                || t.TransactionType == TransactionType.TransactionalTemplatedMail) && t.CustomerNo == customerNo && t.CreatedAt >= startDate && t.CreatedAt <= endDate)
            .Include(t => t.MailRequestLog).ThenInclude(s => s.ResponseLogs)
            .OrderByDescending(t => t.CreatedAt)
                .Skip(page * pageSize)
                .Take(pageSize);

            int count = Context.Transactions
            .Count(t => (t.TransactionType == TransactionType.TransactionalMail
                || t.TransactionType == TransactionType.TransactionalTemplatedMail) && t.CustomerNo == customerNo && t.CreatedAt >= startDate && t.CreatedAt <= endDate)
;
            return (list, count);
        }
        public (IEnumerable<Transaction>, int) GetMailMessagesWithCitizenshipNo(string citizenshipNo, DateTime startDate, DateTime endDate, int page, int pageSize)
        {
            IEnumerable<Transaction> list = Context.Transactions
            .Where(t => (t.TransactionType == TransactionType.TransactionalMail
                || t.TransactionType == TransactionType.TransactionalTemplatedMail) && t.CitizenshipNo == citizenshipNo && t.CreatedAt >= startDate && t.CreatedAt <= endDate)
            .Include(t => t.MailRequestLog).ThenInclude(s => s.ResponseLogs)
            .OrderByDescending(t => t.CreatedAt)
                .Skip(page * pageSize)
                .Take(pageSize);

            int count = Context.Transactions
            .Count(t => (t.TransactionType == TransactionType.TransactionalMail
                || t.TransactionType == TransactionType.TransactionalTemplatedMail) && t.CitizenshipNo == citizenshipNo && t.CreatedAt >= startDate && t.CreatedAt <= endDate);

            return (list, count);
        }

        public (IEnumerable<Transaction>, int) GetPushMessagesWithCustomerNo(ulong customerNo, DateTime startDate, DateTime endDate, int page, int pageSize)
        {
            IEnumerable<Transaction> list = Context.Transactions
                .Where(t => (t.CustomerNo == customerNo && t.CreatedAt >= startDate && t.CreatedAt <= endDate &&
                (t.TransactionType == TransactionType.TransactionalPush || t.TransactionType == TransactionType.TransactionalTemplatedPush)))
                .Include(t => t.PushNotificationRequestLog).ThenInclude(s => s.ResponseLogs)
                .OrderByDescending(t => t.CreatedAt)
                .Skip(page * pageSize)
                .Take(pageSize);

            int count = Context.Transactions
                .Count(t => (t.CustomerNo == customerNo && t.CreatedAt >= startDate && t.CreatedAt <= endDate &&
                (t.TransactionType == TransactionType.TransactionalPush || t.TransactionType == TransactionType.TransactionalTemplatedPush)));
            return (list, count);
        }

        public (IEnumerable<Transaction>, int) GetPushMessagesWithCitizenshipNo(string citizenshipNo, DateTime startDate, DateTime endDate, int page, int pageSize)
        {
            IEnumerable<Transaction> list = Context.Transactions
                .Where(t => (t.CitizenshipNo == citizenshipNo && t.CreatedAt >= startDate && t.CreatedAt <= endDate &&
                (t.TransactionType == TransactionType.TransactionalPush || t.TransactionType == TransactionType.TransactionalTemplatedPush)))
                .Include(t => t.PushNotificationRequestLog).ThenInclude(s => s.ResponseLogs)
                .OrderByDescending(t => t.CreatedAt)
                .Skip(page * pageSize)
                .Take(pageSize);

            int count = Context.Transactions
                .Count(t => (t.CitizenshipNo == citizenshipNo && t.CreatedAt >= startDate && t.CreatedAt <= endDate &&
                (t.TransactionType == TransactionType.TransactionalPush || t.TransactionType == TransactionType.TransactionalTemplatedPush)));


            return (list, count);

        }
    }
}
