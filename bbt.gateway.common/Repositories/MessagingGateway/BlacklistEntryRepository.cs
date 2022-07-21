using bbt.gateway.common.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace bbt.gateway.common.Repositories
{
    public class BlacklistEntryRepository : Repository<BlackListEntry>, IBlacklistEntryRepository
    {
        public BlacklistEntryRepository(DatabaseContext context) : base(context)
        { 
        
        }

        public async Task<IEnumerable<BlackListEntry>> GetWithLogsAsync(int countryCode, int prefix, int number, int page, int pageSize)
        {
            return await Context.BlackListEntries
                
                .Where(b => b.PhoneConfiguration.Phone.CountryCode == countryCode && b.PhoneConfiguration.Phone.Prefix == prefix && b.PhoneConfiguration.Phone.Number == number)
                .Include(b => b.Logs)
                .Skip(page)
                .Take(pageSize)
                .ToListAsync();
        }
    }
}
