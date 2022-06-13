using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bbt.gateway.common.Models.v2
{
    public class OtpResponse
    {
        public Guid TxnId { get; set; }
        public SendSmsResponseStatus Status { get; set; }
        public string StatusMessage { get; set; }
    }
}
