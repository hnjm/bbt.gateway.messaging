using bbt.gateway.common.Models;
using System.Xml.Serialization;

namespace bbt.gateway.messaging.Api.TurkTelekom.Model
{
    [XmlRoot(ElementName = "VIPSMS_REPORT")]
    public class TurkTelekomSmsStatusResponse : OperatorApiTrackingResponse
    {
        private string _fullResponse;

        public TurkTelekomSmsStatusResponse()
        {
            this.OperatorType = OperatorType.TurkTelekom;
        }

        [XmlElement(ElementName = "SMS")]
        public TurkTelekomResponseSmsStatus ResponseSmsStatus { get; set; }

        public void SetFullResponse(string fullResponse)
        {
            this._fullResponse = fullResponse;
        }

        public override string GetFullResponse()
        {
            return this._fullResponse;
        }

        public override string GetResponseCode()
        {
            return ResponseSmsStatus.Status;
        }

        public override string GetResponseMessage()
        {
            return ResponseSmsStatus.StatusDesc;
        }
    }
}
