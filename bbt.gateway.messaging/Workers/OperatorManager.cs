using bbt.gateway.messaging.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace bbt.gateway.messaging.Workers
{
    public class OperatorManager
    {
        private readonly DatabaseContext _databaseContext; 
        List<Operator> operators = new List<Operator>();

        public OperatorManager(DatabaseContext databaseContext)
        {
            _databaseContext = databaseContext;
            loadOperators();
        }

        public Operator[] Get()
        {
            Operator[] returnValue;
            
            returnValue = _databaseContext.Operators.ToArray();
            
            return returnValue;
        }

        public Operator Get(OperatorType type)
        {
            return operators.FirstOrDefault(o => o.Type == type);
        }


        public void Save(Operator data)
        {
           
            if (!operators.Any(o => o.Id == data.Id))
            {
                throw new NotSupportedException("Adding new operator is not allowed.");
            }
            else
            {
                _databaseContext.Operators.Update(data);
            }
            _databaseContext.SaveChanges();

            //TODO: Meanwhile, dont forget to inform other pods to invalidate headers cahce.
            loadOperators();
        }

        private void loadOperators()
        {
           operators = _databaseContext.Operators.ToList();
        }
    }
}
