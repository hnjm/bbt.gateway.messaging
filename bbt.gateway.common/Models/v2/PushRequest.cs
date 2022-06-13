using System.ComponentModel.DataAnnotations;

namespace bbt.gateway.common.Models.v2
{
    public class PushRequest
    {
        [Required]
        public SenderType Sender { get; set; }
        [Required(AllowEmptyStrings = false)]
        [MinLength(10), MaxLength(11)]
        public string CitizenshipNo { get; set; }
        [Required]
        public string Content { get; set; }
        public long? CustomerNo { get; set; }
        public Process Process { get; set; }
    }
}
