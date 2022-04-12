using bbt.gateway.common.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace bbt.gateway.common.Models
{
    public class MailResponseLog
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Topic { get; set; }
        public string ResponseCode { get; set; }
        public string ResponseMessage { get; set; }
        public string StatusQueryId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public string RequestBody { get; set; }
        public string ResponseBody { get; set; }
    }
}
