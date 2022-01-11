using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace bbt.gateway.messaging.Api.TurkTelekom.Model
{
    [XmlRoot(ElementName = "VIPSMS_REPORT")]
    public class TurkTelekomSmsStatusResponse
    {
        [XmlElement(ElementName = "SMS")]
        public TurkTelekomResponseSmsStatus ResponseSmsStatus { get; set; }
       
    }
}
