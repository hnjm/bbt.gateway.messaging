using bbt.gateway.common.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace bbt.gateway.common.Repositories
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
                .Include(c => c.BlacklistEntries.Where(b => b.ValidTo > blackListValidDate.SetKindUtc()))
                .FirstOrDefault();
        }

        public IEnumerable<PhoneConfiguration> GetWithRelatedLogsAndBlacklistEntries(int countryCode, int prefix, int number, int count)
        {
            return Context.PhoneConfigurations.Where(c => c.Phone.CountryCode == countryCode && c.Phone.Prefix == prefix && c.Phone.Number == number)
                .Include(c => c.BlacklistEntries.Take(count).OrderByDescending(l => l.CreatedAt))
                .Include(c => c.OtpLogs.Take(count).OrderByDescending(l => l.CreatedAt)).ThenInclude(o => o.ResponseLogs)
                .Include(c => c.Logs.Take(count).OrderByDescending(l => l.CreatedAt))
                .Include(c => c.SmsLogs.Take(count).OrderByDescending(l => l.CreatedAt));
        }

       
    }
}
