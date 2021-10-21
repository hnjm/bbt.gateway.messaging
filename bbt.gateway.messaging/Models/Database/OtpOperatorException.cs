using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace bbt.gateway.messaging.Models
{
    public class OtpOperatorException
    {
        public Guid Id { get; set; }
        public OperatorType Operator { get; set; }
        public OperatorType ReplaceWith { get; set; }
        public string Status { get; set; }
        public DateTime ValidTo { get; set; }
        public string CreatedAt { get; set; }
        public Process CreatedBy { get; set; }
    }
}
