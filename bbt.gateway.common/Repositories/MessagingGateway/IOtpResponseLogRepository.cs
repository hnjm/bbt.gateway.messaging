using bbt.gateway.common.Models;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace bbt.gateway.common.Repositories
{
    public interface IOtpResponseLogRepository : IRepository<OtpResponseLog>
    {
        Task<IEnumerable<OtpResponseLog>> GetOtpResponseLogsWithTrackingLogAsync(Expression<Func<OtpResponseLog, bool>> predicate);
    }
}
