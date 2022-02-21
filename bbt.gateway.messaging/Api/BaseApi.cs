using bbt.gateway.common.Models;
using System.Linq;

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
            }
        }

        public void SetOperatorType(Operator op) => OperatorConfig = op;

        protected Operator OperatorConfig { get; set; }
    }
}
