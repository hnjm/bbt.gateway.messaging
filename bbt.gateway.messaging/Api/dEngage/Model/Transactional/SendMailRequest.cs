﻿using System.Collections.Generic;

namespace bbt.gateway.messaging.Api.dEngage.Model.Transactional
{
    public class SendMailRequest
    {
        public ContentMail content { get; set; } = new();
        public SendMail send { get; set; } = new();
        public List<Attachment> attachments { get; set; }
        public string current { get; set; }
        public string replyTo { get; set; }
    }

    public class ContentMail
    {
        public string fromNameId { get; set; }
        public string subject { get;set; }
        public string html { get; set; }
        public string templateId { get; set; }
    }

    public class SendMail
    {
        public string to { get; set; }
    }

    public class Attachment
    {
        public string fileName { get; set; }
        public string fileContent { get; set; }
    }

}
