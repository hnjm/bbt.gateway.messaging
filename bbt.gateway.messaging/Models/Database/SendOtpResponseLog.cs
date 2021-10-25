using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace bbt.gateway.messaging.Models
{
    public class SendOtpResponseLog
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public OperatorType Operator { get; set; }
        public string Topic { get; set; }
        public SendSmsResponseStatus ResponseCode { get; set; }
        public string ResponseMessage { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
