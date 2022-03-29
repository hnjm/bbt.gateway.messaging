using bbt.gateway.common.Models;
using System;
using System.Collections.Generic;

namespace bbt.gateway.common.Repositories
{
    public interface IPhoneConfigurationRepository : IRepository<PhoneConfiguration>
    {
        IEnumerable<PhoneConfiguration> GetWithRelatedLogsAndBlacklistEntries(int countryCode, int prefix, int number, int count);
        PhoneConfiguration GetWithBlacklistEntires(int countryCode, int prefix, int number,DateTime blackListValidDate);
    }
}
