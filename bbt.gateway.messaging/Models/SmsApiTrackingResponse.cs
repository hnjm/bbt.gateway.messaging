using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace bbt.gateway.messaging.Models
{
    public class SmsApiTrackingResponse
    {
        public SmsTrackingStatus SmsTrackingStatus { get; set; }
        public string ReturnMessage { get; set; }
    }
}
