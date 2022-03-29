using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace bbt.gateway.common.Models
{
    public  class Phone
    {
        public int CountryCode { get; set; }
        public int Prefix { get; set; }
        public int Number { get; set; }

        public override string ToString()
        {
            return $"+{CountryCode}{Prefix}{Number}";
        }
    }
}
