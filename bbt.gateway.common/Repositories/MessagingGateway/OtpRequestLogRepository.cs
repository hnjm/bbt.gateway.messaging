using bbt.gateway.common.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace bbt.gateway.common.Repositories
{
    public class OtpRequestLogRepository : Repository<OtpRequestLog>, IOtpRequestLogRepository
    {
        public OtpRequestLogRepository(DatabaseContext context) : base(context)
        { 
        
        }

        public IEnumerable<OtpRequestLog> GetWithResponseLogs(int countryCode, int prefix, int number, int page, int pageSize)
        {
            return Context.OtpRequestLogs
                .Where(c => c.Phone.CountryCode == countryCode && c.Phone.Prefix == prefix && c.Phone.Number == number)
                .Include(c => c.ResponseLogs).ThenInclude(r => r.TrackingLogs)
                .Skip(page * pageSize)
                .Take(pageSize);
        }


    }
}
