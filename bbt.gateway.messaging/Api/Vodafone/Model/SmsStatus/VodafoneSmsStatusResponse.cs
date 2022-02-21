using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;
using bbt.gateway.common.Models;

namespace bbt.gateway.messaging.Api.Vodafone.Model
{
    public class VodafoneSmsStatusResponse : OperatorApiTrackingResponse
    {
        public string ResponseCode { get; set; }
        public string ResponseMessage { get; set; }

        private string _fullResponse;
        public VodafoneSmsStatusResponse()
        {
            OperatorType = OperatorType.Vodafone;
        }

        public void SetFullResponse(string fullResponse)
        {
            this._fullResponse = fullResponse;
        }

        public override string GetFullResponse()
        {
            return _fullResponse;
        }

        public override string GetResponseCode()
        {
            return ResponseCode;
        }

        public override string GetResponseMessage()
        {
            return ResponseMessage;
        }
    }
}
