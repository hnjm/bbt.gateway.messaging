using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bbt.gateway.common.Models
{
    public class MiddlewareRequest
    {
        public string CustomerNo { get; set; }
        public Phone Phone { get; set; }
        public string Email { get; set; }
    }
}
