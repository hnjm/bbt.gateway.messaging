namespace bbt.gateway.common.Models
{
    public class SmsTrackingLog
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public dEngageSmsTrackingStatus Status { get; set; }
        public string StatusReason { get; set; }
        public string Detail { get; set; }
        public DateTime QueriedAt { get; set; } = DateTime.Now;
        public SmsResponseLog SmsResponseLog { get; set; }
    }
}
