using bbt.gateway.common.Models;

namespace bbt.gateway.messaging.Api
{
    public abstract class OperatorApiTrackingResponse
    {
        protected OperatorType OperatorType;

        public OperatorType GetOperatorType()
        {
            return this.OperatorType;
        }

        public abstract string GetResponseCode();
        public abstract string GetResponseMessage();
        public abstract string GetFullResponse();
    }
}
