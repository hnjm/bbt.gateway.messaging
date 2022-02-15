using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace bbt.gateway.messaging.Models
{
    public class SmsApiResponse
    {
        public SendSmsResponseStatus SmsResponseStatus { get; set; }
        public string ReturnMessage { get; set; }
    }
}
