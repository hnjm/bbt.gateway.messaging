using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace bbt.gateway.messaging.Api.Pusula.Model.GetCustomer
{
    public class GetCustomerResponse
    {
        public bool IsSuccess { get; set; }
        public int BranchCode { get; set; }
        public string BusinessLine { get; set; }
    }
}
