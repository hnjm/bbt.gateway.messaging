using bbt.gateway.messaging.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace bbt.gateway.messaging.Repositories
{
    public class BlacklistEntryRepository : Repository<BlackListEntry>, IBlacklistEntryRepository
    {
        public BlacklistEntryRepository(DatabaseContext context) : base(context)
        { 
        
        }

    }
}
