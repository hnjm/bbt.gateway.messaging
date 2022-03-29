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

        // TODO: Consider in for all SMS messages 
        /// <summary>
        /// Consumer can set sender direclty.  If sender is set to Burgan(1) or On(2) by consumer do not load header informattion and user selected sender and related prefix/suffix.  
        /// </summary>
        [JsonProperty(Order = -90)]
        public HeaderInfo HeaderInfo;
        [JsonProperty(Order = -50)]
        public Process Process { get; set; }
        [JsonProperty(Order = -85)]
        public MessageContentType ContentType { get; set; }

    }
}

