using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace bbt.gateway.common.Models.v2
{
    public class Process
    {
        [Required(AllowEmptyStrings = false,ErrorMessage = "This field is mandatory.")]
        public string Name { get; set; }
        public string ItemId { get; set; }
        public string Action { get; set; }
        [Required(AllowEmptyStrings = false, ErrorMessage = "This field is mandatory.")]
        public string Identity { get; set; }
    }
}
