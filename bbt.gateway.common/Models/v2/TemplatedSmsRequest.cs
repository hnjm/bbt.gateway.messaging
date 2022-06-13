using System.ComponentModel.DataAnnotations;

namespace bbt.gateway.common.Models.v2
{
    public class TemplatedSmsRequest
    {
        [Required]
        public SenderType Sender { get; set; }
        [Required]
        public Phone Phone { get; set; }
        [Required]
        public string Template { get; set; }
        public string? TemplateParams { get; set; }
        public long? CustomerNo { get; set; }
        [MinLength(10), MaxLength(11)]
        public string? CitizenshipNo { get; set; }
        public Process Process { get; set; }
    }
}
