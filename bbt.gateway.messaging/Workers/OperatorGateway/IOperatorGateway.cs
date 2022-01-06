using bbt.gateway.messaging.Models;
using System.Collections.Concurrent;

namespace bbt.gateway.messaging.Workers.OperatorGateway
{
    public interface IOperatorGateway
    {
        void SendOtp(Phone phone, string content, ConcurrentBag<OtpResponseLog> responses, Header header, bool useControlDays);
        OtpResponseLog SendOtp(Phone phone, string content, Header header);

    }
}
