﻿using bbt.gateway.common.Attributes;
using System.ComponentModel.DataAnnotations;

namespace bbt.gateway.common.Models.v2
{
    public class TemplatedSmsRequestString
    {
        [Required]
        public SenderType Sender { get; set; }
        [Required]
        public PhoneString Phone { get; set; }
        [Required]
        public string Template { get; set; }
        public string? TemplateParams { get; set; }
        public long? CustomerNo { get; set; }

        [CitizenshipNo(10, 11)]
        public string? CitizenshipNo { get; set; }
        public string[] Tags { get; set; }
        public Process Process { get; set; }
    }
}
