using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace bbt.gateway.messaging.Api.TurkTelekom.Model
{
    [XmlRoot(ElementName = "VIPSMS")]
    public class TurkTelekomSmsResponse : OperatorApiResponse
    {
        [XmlIgnore]
        public string RequestBody { get; set; }
        [XmlIgnore]
        public string ResponseBody { get; set; }

        [XmlElement(ElementName = "SMS")]
        public TurkTelekomResponseSms ResponseSms { get; set; }

        public TurkTelekomSmsResponse()
        {
            this.OperatorType = Models.OperatorType.TurkTelekom;
        }

        public override string GetMessageId()
        {
            return ResponseSms.MessageId;
        }

        public override string GetResponseCode()
        {
            return ResponseSms.ResponseCode;
        }

        public override string GetResponseMessage()
        {
            return ResponseSms.ResponseMessage;
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
