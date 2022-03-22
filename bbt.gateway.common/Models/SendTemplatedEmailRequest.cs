﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace bbt.gateway.common.Models
{
    public class SendTemplatedEmailRequest : SendEmailRequest
    {
        
        public string CustomerNo { get; set; }
        public string Subject { get; set; }
        public string Content { get; set; }
        public string Template { get; set; }
    }
}
