using System.Collections.Generic;

namespace bbt.gateway.common.Api.dEngage.Model.Transactional
{
    public class SendPushRequest
    {
        public string contactKey { get; set; }
        public string contentId { get; set; }   
        public string current { get; set; }
        public string customParameters { get; set; }
        public string[] Tags { get; set; }
        public common.Models.v2.InboxParams? inboxParams { get; set; }
    }

    public class CustomParameter
    {
        public string key { get; set; }
        public string value { get; set; }
    }

}
