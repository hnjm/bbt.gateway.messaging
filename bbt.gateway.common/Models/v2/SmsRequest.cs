using System.ComponentModel.DataAnnotations;

namespace bbt.gateway.common.Models.v2
{
    public class SmsRequest
    {
        [Required]
        public SenderType Sender { get; set; }
        [Required]
        public SmsTypes SmsType { get; set; }
        [Required]
        public Phone Phone { get; set; }
        [Required(AllowEmptyStrings = false)]
        public string Content { get; set; }
        public long? CustomerNo { get; set; }
        [MinLength(10), MaxLength(11)]
        public string? CitizenshipNo { get; set; }
        public Process Process { get; set; }
    }
}
