using System.Collections.Generic;

namespace bbt.gateway.messaging.Api.dEngage.Model.Contents
{
    public class PushContentsResponse
    {
        public int code { get; set; }
        public string message { get; set; }
        public PushResult data { get; set; }
    }

    public class PushResult
    {
        public List<PushContentInfo> result { get; set; }
    }

    public class PushContentInfo
    {
        public string name { get; set; }
        public string id { get; set; }
    }
}
