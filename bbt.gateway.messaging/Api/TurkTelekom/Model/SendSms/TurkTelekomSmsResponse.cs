using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace bbt.gateway.messaging.Api.TurkTelekom.Model
{
    [XmlRoot(ElementName = "VIPSMS")]
    public class TurkTelekomSmsResponse
    {
        [XmlElement(ElementName = "SMS")]
        public ResponseSms ResponseSms { get; set; }
       
    }
}
