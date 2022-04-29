using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace bbt.gateway.common.Models
{
    public class SendTemplatedEmailRequest : SendEmailRequest
    {
        public string TemplateParams { get; set; }
        public long? CustomerNo { get; set; }
        public string ContactId { get; set; }
        public string Template { get; set; }
    }
}
