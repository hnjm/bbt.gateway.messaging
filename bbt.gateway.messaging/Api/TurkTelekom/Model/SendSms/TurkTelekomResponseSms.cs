using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace bbt.gateway.messaging.Api.TurkTelekom.Model
{
    public class TurkTelekomResponseSms
    {
        [XmlElement(ElementName = "RETURNCODE")]
        public string ReturnCode { get; set; }
        [XmlElement(ElementName = "RETURNMESSAGE")]
        public string ReturnMessage { get; set; }
        [XmlElement(ElementName = "MESSAGEID")]
        public string MessageId { get; set; }
    }
}
