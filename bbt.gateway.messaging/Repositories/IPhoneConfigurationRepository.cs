using bbt.gateway.messaging.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace bbt.gateway.messaging.Repositories
{
    public interface IPhoneConfigurationRepository : IRepository<PhoneConfiguration>
    {
        IEnumerable<PhoneConfiguration> GetWithRelatedLogsAndBlacklistEntries(int countryCode, int prefix, int number, int count);
        PhoneConfiguration GetWithBlacklistEntires(int countryCode, int prefix, int number,DateTime blackListValidDate);
    }
}
