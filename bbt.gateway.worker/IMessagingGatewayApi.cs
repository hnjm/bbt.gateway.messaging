using bbt.gateway.common.Models;
using Refit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bbt.gateway.worker
{
    public interface IMessagingGatewayApi
    {
        [Post("/api/v1/Messaging/sms/check-message")]
        Task<OtpTrackingLog> CheckMessageStatus(CheckSmsRequest request);
    }
}
