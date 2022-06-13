using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bbt.gateway.common.Models
{
    public class TransactionalTrackingLog
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public bool IsDelivered { get; set; }
        public string Detail { get; set; }
        public DateTime QueriedAt { get; set; } = DateTime.Now;
        public SmsResponseLog SmsResponseLog { get; set; }
        public MailRequestLog MailRequestLog { get; set; }
        public PushNotificationRequestLog PushNotificationRequestLog { get; set; }
    }
}
