using bbt.gateway.common.Models;

namespace bbt.gateway.common.Repositories
{
    public class SmsLogRepository : Repository<SmsLog>, ISmsLogRepository
    {
        public SmsLogRepository(DatabaseContext context) : base(context)
        { 
        
        }

    }
}
