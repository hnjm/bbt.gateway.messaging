using bbt.gateway.messaging.Api.TurkTelekom;
using bbt.gateway.messaging.Api.TurkTelekom.Model;
using bbt.gateway.messaging.Models;
using bbt.gateway.messaging.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;

namespace bbt.gateway.messaging.Workers.OperatorGateway
{


    public class OperatorTurkTelekom : OperatorGatewayBase, IOperatorGateway
    {
        private readonly TurkTelekomApi _turkTelekomApi;
        public OperatorTurkTelekom(TurkTelekomApi turkTelekomApi)
        {
            _turkTelekomApi = turkTelekomApi;
            Type = OperatorType.TurkTelekom;
        }

        public async Task<bool> SendOtp(Phone phone, string content, ConcurrentBag<OtpResponseLog> responses, Header header, bool useControlDays)
        {
            var turkTelekomResponse = await _turkTelekomApi.SendSms(CreateSmsRequest(phone,content,header,useControlDays));
            System.Diagnostics.Debug.WriteLine("TT otp is send");

            var response =  turkTelekomResponse.BuildOperatorApiResponse();

            Task.Run(() => TrackMessageStatus(response));

            responses.Add(response);

            return true;
        }
        public async Task<OtpResponseLog> SendOtp(Phone phone, string content, Header header, bool useControlDays)
        {
            var turkTelekomResponse = await _turkTelekomApi.SendSms(CreateSmsRequest(phone, content, header, useControlDays));
            System.Diagnostics.Debug.WriteLine("TurkTelekom otp is send");

            var response = turkTelekomResponse.BuildOperatorApiResponse();

            Task.Run(() => TrackMessageStatus(response));
            return response;
        }

        public override async Task<OtpTrackingLog> CheckMessageStatus(OtpResponseLog response)
        {
            var turkTelekomResponse = await _turkTelekomApi.CheckSmsStatus(CreateSmsStatusRequest(response.StatusQueryId));
            return turkTelekomResponse.BuildOperatorApiTrackingResponse(response);
        }

        private TurkTelekomSmsRequest CreateSmsRequest(Phone phone, string content, Header header,bool useControlDays)
        {
            DateTime checkDate = DateTime.Now.AddDays(-1 * OperatorConfig.ControlDaysForOtp);
            if (useControlDays)
            {
                var phoneConfiguration = GetPhoneConfiguration(phone);
                if (phoneConfiguration.BlacklistEntries != null &&
                    phoneConfiguration.BlacklistEntries.Count > 0)
                {
                    var blackListEntry = phoneConfiguration.BlacklistEntries
                    .Where(b => b.Status == BlacklistStatus.Resolved).OrderByDescending(b => b.CreatedAt)
                    .FirstOrDefault();

                    if (blackListEntry != null)
                    {
                        if (blackListEntry.ResolvedAt != null)
                        {
                            DateTime resolvedDate = blackListEntry.ResolvedAt.Value;
                            checkDate = checkDate > resolvedDate ? checkDate : resolvedDate;
                        }
                    }
                }
            }
            return new TurkTelekomSmsRequest()
            {
                UserCode = OperatorConfig.User,
                Password = OperatorConfig.Password,
                CheckDate = checkDate.ToString("yyyyMMddHHmmss"),
                Duration = "300",
                GsmNo = phone.CountryCode.ToString()+phone.Prefix.ToString() + phone.Number.ToString(),
                IsEncrypted = "False",
                IsNotification = "True",
                Header = "BURGAN BANK",
                Message = content,
                OnNetPortInControl = "True",
                OnNetSimChange = "True",
                PortInCheckDate = checkDate.ToString("yyyyMMddHHmmss")

            };
        }

        private TurkTelekomSmsStatusRequest CreateSmsStatusRequest(string MessageId, string LastMessageId = "")
        {
            return new TurkTelekomSmsStatusRequest() {
                UserCode = OperatorConfig.User,
                Password = OperatorConfig.Password,
                MessageId = MessageId,
                LastMessageId = LastMessageId
            };
        }
    }
}
