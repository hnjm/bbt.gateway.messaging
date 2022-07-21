using bbt.gateway.common.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace bbt.gateway.common.Models
{
    public class PushNotificationResponseLog : dEngageResponse
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Topic { get; set; }
        public string ResponseCode { get; set; }
        public string ResponseMessage { get; set; }
        public ICollection<PushTrackingLog> TrackingLogs { get; set; } = new List<PushTrackingLog>();
        public string StatusQueryId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public override string GetResponseCode()
        {
            return ResponseCode;
        }
    }
}
