using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace bbt.gateway.common.Models
{
    public class SmsResponseLog : dEngageResponse
    {
        public Guid Id { get; set; }
        public string Content { get; set; }
        public OperatorType Operator { get; set; }
        public int OperatorResponseCode { get; set; }
        public string OperatorResponseMessage { get; set; }
        public string StatusQueryId { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }

        public override string GetResponseCode()
        {
            return OperatorResponseCode.ToString();
        }
    }
}
