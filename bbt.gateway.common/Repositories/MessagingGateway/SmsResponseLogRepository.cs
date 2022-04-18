using bbt.gateway.common.Models;

namespace bbt.gateway.common.Repositories
{
    public class SmsResponseLogRepository : Repository<SmsResponseLog>, ISmsResponseLogRepository
    {
        public SmsResponseLogRepository(DatabaseContext context) : base(context)
        { 
        
        }

    }
}
