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
        [JsonProperty(Order = -85)]
        public MessageContentType ContentType { get; set; }
    }
}
