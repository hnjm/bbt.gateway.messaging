﻿namespace bbt.gateway.common.Models.v2
{
    public class TemplatedMailResponse
    {
        public Guid TxnId { get; set; }
        public dEngageResponseCodes Status { get; set; }
        public string StatusMessage { get; set; }
    }
}
