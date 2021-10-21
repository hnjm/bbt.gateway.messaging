using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace bbt.gateway.messaging.Models
{
    public class SendMessageSmsRequest : SendSmsRequest
    {
        public string Content { get; set; }
    }
}
