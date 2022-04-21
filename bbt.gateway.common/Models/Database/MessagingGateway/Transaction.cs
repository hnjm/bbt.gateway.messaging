using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace bbt.gateway.common.Models
{
    public class Transaction
    {
        public Guid Id { get; set; }
        public Phone Phone { get; set; }
        public string Mail { get; set; }
        public TransactionType TransactionType { get; set; }
        public string Request { get; set; }
        public string Response { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public string IpAdress { get; set; }
        public Process CreatedBy { get; set; }

        public OtpRequestLog OtpRequestLog { get; set; }
        public SmsRequestLog SmsRequestLog { get; set; }
        public MailRequestLog MailRequestLog { get; set; }
        
        
    }
}
