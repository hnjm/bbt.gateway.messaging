using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace bbt.gateway.messaging.Models
{
    public class SmsTrackingLog
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public SmsTrackingStatus Status { get; set; }
        public string Detail { get; set; }
        public DateTime QueriedAt { get; set; } = DateTime.Now;
    }
}
