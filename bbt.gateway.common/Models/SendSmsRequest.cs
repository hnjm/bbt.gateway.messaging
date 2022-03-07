using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace bbt.gateway.common.Models
{
    public abstract class SendSmsRequest
    {
        public Guid Id { get; set; }

        public Phone Phone { get; set; }

        // TODO: Consider in for all SMS messages 
        /// <summary>
        /// Consumer can set sender direclty.  If sender is set to Burgan(1) or On(2) by consumer do not load header informattion and user selected sender and related prefix/suffix.  
        /// </summary>
        public SenderType Sender { get; set; }
        public string Prefix { get; set; }
        public string Suffix { get; set; }

        public Process Process { get; set; }
        
        public MessageContentType ContentType { get; set; }

    }
}

