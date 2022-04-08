using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace bbt.gateway.common.Models
{
    public class OtpRequestLog
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid TxnId { get; set; }
        public PhoneConfiguration PhoneConfiguration { get; set; }
        public Phone Phone { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public Process CreatedBy { get; set; }
        public ICollection<OtpResponseLog> ResponseLogs { get; set; } = new List<OtpResponseLog>();
        
    }
}
