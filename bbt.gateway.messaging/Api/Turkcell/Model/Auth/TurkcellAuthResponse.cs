using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace bbt.gateway.messaging.Api.Turkcell.Model
{
    public class TurkcellAuthResponse
    {
        public string ResultCode { get; set; }
        public string AuthToken { get; set; }
    }
}
