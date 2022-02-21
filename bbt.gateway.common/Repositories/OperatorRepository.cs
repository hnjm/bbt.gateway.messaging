using bbt.gateway.common.Models;

namespace bbt.gateway.common.Repositories
{
    public class OperatorRepository : Repository<Operator>, IOperatorRepository
    {
        public OperatorRepository(DatabaseContext context) : base(context)
        { 
        
        }

    }
}
