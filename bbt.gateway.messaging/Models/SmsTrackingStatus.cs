using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace bbt.gateway.messaging.Models
{
    public enum SmsTrackingStatus
    {
        Delivered = 200,        
        DeviceClosed = 460,
        DeviceRejected = 461,
        Pending = 462,
        Expired = 463,
    }
}