using System.Collections.Generic;

namespace bbt.gateway.messaging.Api.dEngage.Model.Transactional
{
    public class SendPushRequest
    {
        public string appId { get; set; }
        public string token { get; set; }
        public List<string> tags { get; set; } = new();
        public string contentId { get; set; }
        public string current { get; set; }
        public string customParameters { get; set; }
        public string contactKey { get; set; }
    }

}
