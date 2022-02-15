

namespace bbt.gateway.messaging.Models
{
    public enum SmsTrackingStatus
    {
        Delivered = 200,        
        DeviceClosed = 460,
        DeviceRejected = 461,
        Pending = 462,
        Expired = 463,
        UnknownNumber = 464,
        SystemError = 465
    }
}