using System.Collections.Generic;

namespace bbt.gateway.messaging.Api.dEngage.Model.Contents
{
    public class SmsContentsResponse
    {
        public int code { get; set; }
        public string message { get; set; }
        public SmsResult data { get; set; }
        
    }

    public class SmsResult
    {
        public List<SmsContentInfo> result { get; set; }
        public bool queryForNextPage { get; set; }
        public int totalRowCount { get; set; }
    }

    public class SmsContentInfo
    {
        public string contentName { get; set; }
        public string publicId { get; set; }
    }
}
