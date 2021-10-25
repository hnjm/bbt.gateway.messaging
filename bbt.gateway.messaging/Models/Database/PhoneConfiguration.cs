using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace bbt.gateway.messaging.Models
{
    public class PhoneConfiguration
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Phone Phone { get; set; } 
        public int? CustomerNo { get; set; } 
        public OperatorType? Operator { get; set; }
        public ICollection<PhoneConfigurationLog> Logs { get; set; }
        public ICollection<SendOtpRequestLog> OtpLogs {get;set;}
        public ICollection<SendSmsLog> SmsLogs { get; set; }
        public ICollection<OtpBlackListEntry> BlacklistEntries { get; set; }
    }
}
