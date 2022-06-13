using bbt.gateway.common.Models;


namespace bbt.gateway.common.Repositories
{
    public class TransactionalLogRepository : Repository<TransactionalTrackingLog>, ITransactionalTrackingLogRepository
    {
        public TransactionalLogRepository(DatabaseContext context) : base(context)
        { 
        
        }

    }
}
