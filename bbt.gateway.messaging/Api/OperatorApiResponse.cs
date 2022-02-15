using bbt.gateway.messaging.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace bbt.gateway.messaging.Api
{
    public abstract class OperatorApiResponse
    {
        protected OperatorType OperatorType;
        public string t;
        public OperatorType GetOperatorType()
        {
            return this.OperatorType;
        }


        public abstract string GetMessageId();
        public abstract string GetResponseCode();
        public abstract string GetResponseMessage();
        public abstract string GetRequestBody();
        public abstract string GetResponseBody();
    }
}
