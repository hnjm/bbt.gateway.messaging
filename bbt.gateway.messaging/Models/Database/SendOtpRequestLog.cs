using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace bbt.gateway.messaging.Models
{
    public class SendOtpRequestLog
    {
        public Guid Id { get; set; }
        public PhoneConfiguration Phone { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; }
        public Process CreatedBy { get; set; }
        public ICollection<SendOtpResponseLog> ResponseLogs { get; set; }
    }
}
