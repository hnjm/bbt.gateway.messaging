using bbt.gateway.messaging.Api.TurkTelekom.Model;
using bbt.gateway.common.Models;
using bbt.gateway.messaging.Api.TurkTelekom;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace bbt.gateway.messaging.Workers.OperatorGateway
{

    public class OperatorTurkTelekom : OperatorGatewayBase, IOperatorGateway
    {
        private readonly ITurkTelekomApi _turkTelekomApi;
        public OperatorTurkTelekom(TurkTelekomApiFactory turkTelekomApiFactory, IConfiguration configuration,
            ITransactionManager transactionManager) : base(configuration, transactionManager)
        {
            _turkTelekomApi = turkTelekomApiFactory(TransactionManager.UseFakeSmtp);
            Type = OperatorType.TurkTelekom;
            _turkTelekomApi.SetOperatorType(OperatorConfig);
        }

        public async Task<bool> SendOtp(Phone phone, string content, ConcurrentBag<OtpResponseLog> responses, Header header, bool useControlDays)
        {
            var turkTelekomResponse = await _turkTelekomApi.SendSms(await CreateSmsRequest(phone,content,header,true));

            var response =  turkTelekomResponse.BuildOperatorApiResponse();
            responses.Add(response);

            return true;
        }
        public async Task<OtpResponseLog> SendOtp(Phone phone, string content, Header header, bool useControlDays)
        {
            var turkTelekomResponse = await _turkTelekomApi.SendSms(await CreateSmsRequest(phone, content, header, true));

            var response = turkTelekomResponse.BuildOperatorApiResponse();

            return response;
        }

        public async Task<OtpTrackingLog> CheckMessageStatus(CheckSmsRequest checkSmsRequest)
        {
            var turkTelekomResponse = await _turkTelekomApi.CheckSmsStatus(CreateSmsStatusRequest(checkSmsRequest.StatusQueryId));
            return turkTelekomResponse.BuildOperatorApiTrackingResponse(checkSmsRequest);
        }

        private async Task<TurkTelekomSmsRequest> CreateSmsRequest(Phone phone, string content, Header header,bool useControlDays)
        {
            DateTime checkDate = DateTime.Now.AddDays(-1 * OperatorConfig.ControlDaysForOtp);
            if (useControlDays)
            {
                var phoneConfiguration = await GetPhoneConfiguration(phone);

                if (phoneConfiguration != null)
                {
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
                        else
                        {
                            checkDate = checkDate > TransactionManager.OldBlacklistVerifiedAt ? checkDate : TransactionManager.OldBlacklistVerifiedAt;
                        }
                    }
                    else
                    {
                        checkDate = checkDate > TransactionManager.OldBlacklistVerifiedAt ? checkDate : TransactionManager.OldBlacklistVerifiedAt;
                    }
                }
                else
                {
                    checkDate = checkDate > TransactionManager.OldBlacklistVerifiedAt ? checkDate : TransactionManager.OldBlacklistVerifiedAt;
                }
            }
            return new TurkTelekomSmsRequest()
            {
                UserCode = OperatorConfig.User,
                Password = OperatorConfig.Password,
                CheckDate = checkDate.ToString("yyyyMMddHHmmss"),
                Duration = "300",
                GsmNo = phone.CountryCode.ToString()+phone.Prefix.ToString() + phone.Number.ToString().PadLeft(7,'0'),
                IsEncrypted = "False",
                IsNotification = "True",
                Header = Constant.OperatorSenders[header.SmsSender][OperatorType.TurkTelekom],
                Message = content.Trim(),
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
