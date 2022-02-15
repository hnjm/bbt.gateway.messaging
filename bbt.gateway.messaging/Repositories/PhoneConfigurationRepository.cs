using bbt.gateway.messaging.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace bbt.gateway.messaging.Repositories
{
    public class PhoneConfigurationRepository : Repository<PhoneConfiguration>, IPhoneConfigurationRepository
    {
        public PhoneConfigurationRepository(DatabaseContext context) : base(context)
        { 
        
        }

        public PhoneConfiguration GetWithBlacklistEntires(int countryCode, int prefix, int number, DateTime blackListValidDate)
        {
            return Context.PhoneConfigurations.Where(i =>
                i.Phone.CountryCode == countryCode &&
                i.Phone.Prefix == prefix &&
                i.Phone.Number == number
                )
                .Include(c => c.BlacklistEntries.Where(b => b.ValidTo > blackListValidDate))
                .FirstOrDefault();
        }

        public IEnumerable<PhoneConfiguration> GetWithRelatedLogsAndBlacklistEntries(int countryCode, int prefix, int number, int count)
        {
            return Context.PhoneConfigurations.Where(c => c.Phone.CountryCode == countryCode && c.Phone.Prefix == prefix && c.Phone.Number == number)
                .Include(c => c.BlacklistEntries.Take(count).OrderBy(l => l.CreatedAt))
                .Include(c => c.OtpLogs.Take(count).OrderBy(l => l.CreatedAt))
                .Include(c => c.Logs.Take(count).OrderBy(l => l.CreatedAt))
                .Include(c => c.SmsLogs.Take(count).OrderBy(l => l.CreatedAt));
        }

       
    }
}
