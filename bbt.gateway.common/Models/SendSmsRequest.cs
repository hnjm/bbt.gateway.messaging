using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace bbt.gateway.common.Models
{
    public abstract class SendSmsRequest
    {
        [JsonProperty(Order = -110)]
        public Guid Id { get; set; }

        [JsonProperty(Order = -100)]
        public Phone Phone { get; set; }

        [JsonProperty(Order = -50)]
        public Process Process { get; set; }
        

    }
}

