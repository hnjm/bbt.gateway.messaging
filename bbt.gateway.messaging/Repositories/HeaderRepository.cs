using bbt.gateway.messaging.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace bbt.gateway.messaging.Repositories
{
    public class HeaderRepository : Repository<Header>, IHeaderRepository
    {
        public HeaderRepository(DatabaseContext context) : base(context)
        { 
        
        }

    }
}
