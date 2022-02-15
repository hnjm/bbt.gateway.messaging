using bbt.gateway.messaging.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace bbt.gateway.messaging.Api
{
    public class BaseApi
    {
        private OperatorType _type;
        public BaseApi()
        { 
        
        }

        protected OperatorType Type
        {
            get { return _type; }
            set
            {
                _type = value;
                using var databaseContext = new DatabaseContext();
                OperatorConfig = databaseContext.Operators.FirstOrDefault(o => o.Type == _type);
            }
        }
        protected Operator OperatorConfig { get; set; }
    }
}
