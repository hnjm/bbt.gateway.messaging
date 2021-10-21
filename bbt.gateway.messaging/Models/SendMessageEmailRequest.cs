using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace bbt.gateway.messaging.Models
{
    public class SendMessageEmailRequest : SendEmailRequest
    {
        public string Subject { get; set; }
        public string Content { get; set; }
    }
}
