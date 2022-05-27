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

        public Phone()
        {

        }

        public Phone(string phone)
        {
            CountryCode = Convert.ToInt32(phone.Substring(0, 2));
            Prefix = Convert.ToInt32(phone.Substring(2, 3));
            Number = Convert.ToInt32(phone.Substring(5, 7));
        }

        public override string ToString()
        {
            return $"+{CountryCode}{Prefix}{Number}";
        }

        public string Concatenate()
        {
            return $"{CountryCode}{Prefix}{Number}";
        }
    }
}
