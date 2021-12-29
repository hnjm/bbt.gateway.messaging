using bbt.gateway.messaging.Api.TurkTelekom;
using bbt.gateway.messaging.Api.TurkTelekom.Model;
using bbt.gateway.messaging.Models;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;

namespace bbt.gateway.messaging.Workers.OperatorGateway
{


    public class OperatorTurkTelekom : OperatorGatewayBase, IOperatorGateway
    {
        private readonly TurkTelekomApi _turkTelekomApi;
        private readonly Operator _operator;
        public OperatorTurkTelekom(TurkTelekomApi turkTelekomApi,OperatorManager operatorManager,DatabaseContext databaseContext) : base(operatorManager,databaseContext)
        {
            _turkTelekomApi = turkTelekomApi;
            Type = OperatorType.TurkTelekom;
            _operator = operatorManager.Get(OperatorType.TurkTelekom);
        }

        public async void SendOtp(Phone phone, string content, ConcurrentBag<OtpResponseLog> responses, Header header, bool useControlDays)
        {
            var turkTelekomResponse = await _turkTelekomApi.SendSms(CreateSmsRequest(phone,content,header,useControlDays));
            System.Diagnostics.Debug.WriteLine("TT otp is send");

            var response = turkTelekomResponse.BuildResponseForTurkTelekom();

            await Task.Run(() => TrackMessageStatus(response));

            responses.Add(response);
        }
        public async Task<OtpResponseLog> SendOtp(Phone phone, string content, Header header)
        {
           
            var turkTelekomResponse = await _turkTelekomApi.SendSms(CreateSmsRequest(phone, content, header,false));
            System.Diagnostics.Debug.WriteLine("TurkTelekom otp is send");

            var response = turkTelekomResponse.BuildResponseForTurkTelekom();

            await Task.Run(() => TrackMessageStatus(response));
            return response;
        }

        public async override Task<OtpTrackingLog> CheckMessageStatus(OtpResponseLog response)
        {
            var turkTelekomResponse = await _turkTelekomApi.CheckSmsStatus(CreateSmsStatusRequest(response.StatusQueryId));
            return turkTelekomResponse.BuildResponseForTurkTelekom(response);
        }

        private TurkTelekomSmsRequest CreateSmsRequest(Phone phone, string content, Header header,bool useControlDays)
        {
            var checkDate = useControlDays ?
                (DateTime.Now.AddDays(_operator.ControlDaysForOtp * -1).ToString("yyyyMMddHHmmss")) :
                (DateTime.Now.ToString("yyyyMMddHHmmss"));
            return new TurkTelekomSmsRequest()
            {
                UserCode = _operator.User,
                Password = _operator.Password,
                CheckDate = checkDate,
                Duration = "300",
                GsmNo = phone.CountryCode.ToString()+phone.Prefix.ToString() + phone.Number.ToString(),
                IsEncrypted = "False",
                IsNotification = "True",
                Header = "BURGAN BANK",
                Message = content,
                OnNetPortInControl = "True",
                OnNetSimChange = "True",
                PortInCheckDate = checkDate

            };
        }

        private TurkTelekomSmsStatusRequest CreateSmsStatusRequest(string MessageId, string LastMessageId = "")
        {
            return new TurkTelekomSmsStatusRequest() {
                UserCode = _operator.User,
                Password = _operator.Password,
                MessageId = MessageId,
                LastMessageId = LastMessageId
            };
        }
    }
}
