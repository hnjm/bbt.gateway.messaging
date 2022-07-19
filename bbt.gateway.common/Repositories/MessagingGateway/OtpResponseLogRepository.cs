using bbt.gateway.common.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace bbt.gateway.common.Repositories
{
    public class OtpResponseLogRepository : Repository<OtpResponseLog>, IOtpResponseLogRepository
    {
        public OtpResponseLogRepository(DatabaseContext context) : base(context)
        { 
        
        }

        public async Task<IEnumerable<OtpResponseLog>> GetOtpResponseLogsWithTrackingLogAsync(Expression<Func<OtpResponseLog, bool>> predicate)
        {
            return await Context.OtpResponseLog.AsNoTracking()
                .Where(predicate)
                .Include("TrackingLogs")
                .ToListAsync();
        }
    }
}
