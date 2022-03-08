using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;


namespace bbt.gateway.common.Models
{
    public class SendMessageSmsRequest : SendSmsRequest
    {
        [Required(AllowEmptyStrings = false,ErrorMessage = "Bu alan boş bırakılamaz.")]
        public string Content { get; set; }
    }
}
