﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace bbt.gateway.common.Models
{
    public class SendTemplatedSmsRequest : SendSmsRequest
    {
        public string TeamplateParams { get; set; }
        public string Template { get; set; }
        public string CustomerNo { get; set; }
        public string ContactId { get; set; }
    }
}
