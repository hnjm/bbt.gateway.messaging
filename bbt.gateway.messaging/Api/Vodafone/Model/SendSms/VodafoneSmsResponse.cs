using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace bbt.gateway.messaging.Api.Vodafone.Model
{
    public class VodafoneSmsResponse : OperatorApiResponse
    {
        public string ResultCode { get; set; }
        public string MessageId { get; set; }
        public string ResultMessage { get; set; }
        public string RequestBody { get; set; }
        public string ResponseBody { get; set; }
        public VodafoneSmsResponse()
        {
            this.OperatorType = Models.OperatorType.Vodafone;
        }

        public override string GetMessageId()
        {
            return MessageId;
        }

        public override string GetResponseCode()
        {
            return ResultCode;
        }

        public override string GetResponseMessage()
        {
            return ResultMessage;
        }

        public override string GetRequestBody()
        {
            return RequestBody;
        }

        public override string GetResponseBody()
        {
            return ResponseBody;
        }
    }
}
