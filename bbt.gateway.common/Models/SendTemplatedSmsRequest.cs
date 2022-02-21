using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace bbt.gateway.common.Models
{
    public class SendTemplatedSmsRequest : SendSmsRequest
    {
        public string Template { get; set; }

        public object Data { get; set; }
    }
}
