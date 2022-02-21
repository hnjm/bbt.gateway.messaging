using bbt.gateway.common.Models;
using System.Collections.Generic;

namespace bbt.gateway.common.Repositories
{
    public interface IBlacklistEntryRepository : IRepository<BlackListEntry>
    {
        IEnumerable<BlackListEntry> getWithLogs(int countryCode, int prefix, int number, int page, int pageSize);
    }
}
