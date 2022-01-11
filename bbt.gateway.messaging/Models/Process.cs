using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace bbt.gateway.messaging.Models
{
    public class Process
    {
        [Required(AllowEmptyStrings = false,ErrorMessage = "Bu alan boş bırakılamaz.")]
        public string Name { get; set; }
        public string ItemId { get; set; }
        public string Action { get; set; }
        public string Identity { get; set; }
    }
}
