using bbt.gateway.common.Models;
using Microsoft.Extensions.Configuration;

namespace bbt.gateway.messaging.Workers.OperatorGateway
{
    public interface IOperatorGatewayBase
    {
        public IConfiguration Configuration { get; }
        public PhoneConfiguration GetPhoneConfiguration(Phone phone);
        public void SaveOperator();
        public Operator OperatorConfig { get; set; }
        public OperatorType Type { get; set; }
        public ITransactionManager TransactionManager { get; }
    }
}
