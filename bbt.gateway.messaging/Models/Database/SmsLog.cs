using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace bbt.gateway.messaging.Models
{
    public class SmsLog
    {
        public Guid Id { get; set; }
        public PhoneConfiguration PhoneConfiguration { get; set; }
        public string Content { get; set; }
        public OperatorType Operator { get; set; }
        public int OperatorResponseCode { get; set; }
        public string OperatorResponseMessage { get; set; }
        public string Status { get; set; }
        public Process CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
