﻿namespace bbt.gateway.common.Models.v2
{
    public class TemplatedPushResponse
    {
        public Guid TxnId { get; set; }
        public dEngageResponseCodes Status { get; set; }
        public string StatusMessage { get; set; }
    }
}
