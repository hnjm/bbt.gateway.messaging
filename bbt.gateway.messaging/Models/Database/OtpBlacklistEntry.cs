using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace bbt.gateway.messaging.Models
{
    public class OtpBlackListEntry
    {
        public Guid Id { get; set; }
        public PhoneConfiguration PhoneConfiguration { get; set; }
        public Phone Phone { get; set; }
        public string Reason { get; set; }
        public string Source { get; set; }
        public string Status { get; set; }
        public DateTime ValidTo { get; set; }
        public Process CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public Process ResolvedBy { get; set; }
        public DateTime ResolvedAt { get; set; }
        public ICollection<OtpBlackListEntryLog> Logs { get; set; }
    }
}
