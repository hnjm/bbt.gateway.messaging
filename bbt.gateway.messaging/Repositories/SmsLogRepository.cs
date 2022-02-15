using bbt.gateway.messaging.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace bbt.gateway.messaging.Repositories
{
    public class SmsLogRepository : Repository<SmsLog>, ISmsLogRepository
    {
        public SmsLogRepository(DatabaseContext context) : base(context)
        { 
        
        }

    }
}
