﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace bbt.gateway.messaging.Models
{
    public class PhoneConfigurationLog
    {
        public Guid Id { get; set; }
        public PhoneConfiguration Phone { get; set; }
        public string Type { get; set; }
        public string Action { get; set; }
        public string ParameterMaster { get; set; }
        public string ParameterSlave { get; set; }
        public Process CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }


    }
}
