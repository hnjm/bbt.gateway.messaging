using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace bbt.gateway.common.Models
{
    public abstract class SendEmailRequest
    {
        public Guid Id { get; set; }

        public string Email { get; set; }

        public Process Process { get; set; }

    }
}

