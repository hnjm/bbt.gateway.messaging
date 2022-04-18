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
        public string TemplateId { get; set; }
        public string TemplateParams { get; set; }
        public string Content { get; set; }
        public MessageContentType ContentType { get; set; }
        public Process Process { get; set; }
    }
}
