﻿using bbt.gateway.common.Models;
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
        [Post("/api/v2/Administration/otp/check-message")]
        Task<OtpTrackingLog> CheckOtpStatus(CheckSmsRequest request);
        [Post("/api/v2/Administration/sms/check-message")]
        Task<SmsTrackingLog> CheckSmsStatus(common.Models.v2.CheckFastSmsRequest request);
        [Post("/api/v1/Administration/templates/sms")]
        Task SetSmsTemplates();
        [Post("/api/v1/Administration/templates/mail")]
        Task SetMailTemplates();
        [Post("/api/v1/Administration/templates/push")]
        Task SetPushTemplates();
    }
}
