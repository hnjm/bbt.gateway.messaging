using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace bbt.gateway.common.Models
{
    public class SendMessageSmsRequest : SendSmsRequest
    {
        [JsonProperty(Order = -80)]
        [Required(AllowEmptyStrings = false,ErrorMessage = "Bu alan boş bırakılamaz.")]
        public string Content { get; set; }
        /// <summary>
        /// Consumer can set sender direclty.  If sender is set to Burgan(1) or On(2) by consumer do not load header informattion and user selected sender and related prefix/suffix.  
        /// </summary>
        [JsonProperty(Order = -90)]
        public HeaderInfo HeaderInfo;
        [JsonProperty(Order = -85)]
        public MessageContentType ContentType { get; set; }
    }
}
