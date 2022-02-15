using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace bbt.gateway.messaging.Api.Turkcell.Model
{

    public class TurkcellSmsStatusResponse : OperatorApiTrackingResponse
    {
        private string _fullResponse = "";
        public string ResultCode { get; set; }
        public string ResultMessage { get; set; }

        public TurkcellSmsStatusResponse()
        {
            OperatorType = Models.OperatorType.Turkcell;
        }

        public void SetFullResponse(string fullResponse)
        {
            _fullResponse = fullResponse;
        }
        public override string GetFullResponse()
        {
            return _fullResponse;
        }

        public override string GetResponseCode()
        {
            return ResultCode;
        }

        public override string GetResponseMessage()
        {
            return ResultMessage;
        }
    }

}
