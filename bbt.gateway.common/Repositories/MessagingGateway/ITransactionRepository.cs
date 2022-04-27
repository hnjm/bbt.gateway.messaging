using bbt.gateway.common.Models;

namespace bbt.gateway.common.Repositories
{
    public interface ITransactionRepository : IRepository<Transaction>
    {
        public IEnumerable<Transaction> GetWithPhone(int countryCode, int prefix, int number, int page, int pageSize);
        public Transaction GetWithId(Guid TxnId);
    }
}
