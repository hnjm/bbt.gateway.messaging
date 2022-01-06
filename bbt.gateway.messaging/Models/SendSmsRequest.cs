using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace bbt.gateway.messaging.Models
{
    public abstract class SendSmsRequest
    {
        public Guid Id { get; set; }

        public Phone Phone { get; set; }

        public Process Process { get; set; }
        
        public MessageContentType ContentType { get; set; }

    }
}

