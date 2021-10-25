using bbt.gateway.messaging.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace bbt.gateway.messaging.Workers.OperatorGateway
{
    public abstract class OperatorGatewayBase
    {
        private OperatorType type;
        protected OperatorType Type
        {
            get { return type; }
            set
            {
                type = value;
                OperatorConfig = OperatorManager.Instance.Get(value);
            }
        }
        protected Operator OperatorConfig { get; set; }
    }
}
