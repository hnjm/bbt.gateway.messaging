using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace bbt.gateway.messaging.Models
{
    public class Process
    {
        public string Name { get; set; }
        public string ItemId { get; set; }
        public string Action { get; set; }
        public string Identity { get; set; }
    }
}
