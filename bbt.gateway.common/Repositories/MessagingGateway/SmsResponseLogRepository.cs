using bbt.gateway.common.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace bbt.gateway.common.Repositories
{
    public class SmsResponseLogRepository : Repository<SmsResponseLog>, ISmsResponseLogRepository
    {
        public SmsResponseLogRepository(DatabaseContext context) : base(context)
        { 
        
        }

        public async Task<IEnumerable<SmsResponseLog>> GetSmsResponseLogsAsync(Expression<Func<SmsResponseLog, bool>> predicate)
        {
            return await Context.SmsResponseLog
                .Where(predicate)
                .Take(10)
                .OrderBy(s => s.CreatedAt)
                .ToListAsync();
        }

    }
}
