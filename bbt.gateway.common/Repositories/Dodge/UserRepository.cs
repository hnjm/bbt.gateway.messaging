using bbt.gateway.common.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace bbt.gateway.common.Repositories
{
    public class UserRepository : DodgeRepository<User>, IUserRepository
    {
        public UserRepository(DodgeDatabaseContext context) : base(context)
        { 
            
        }

        public IEnumerable<User> GetWithDevices(string Username)
        {
            return Context.User
                .Where(u => u.Username == Username)
                .Include(c => c.Devices);
        }

    }
}
