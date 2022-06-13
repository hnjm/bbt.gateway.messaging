using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bbt.gateway.common.Models.v2
{
    public class Phone
    {
        [Required(ErrorMessage = "This Field is Mandatory")]
        [Range(1,10000,ErrorMessage = "CountryCode field is in between 1 and 10000")]
        public int CountryCode { get; set; }
        [Required(ErrorMessage = "This Field is Mandatory")]
        [Range(1, 10000, ErrorMessage = "CountryCode field is in between 1 and 10000")]
        public int Prefix { get; set; }
        [Required(ErrorMessage = "This Field is Mandatory")]
        [Range(1, 9999999, ErrorMessage = "CountryCode field is in between 1 and 10000")]
        public int Number { get; set; }

    }
}
