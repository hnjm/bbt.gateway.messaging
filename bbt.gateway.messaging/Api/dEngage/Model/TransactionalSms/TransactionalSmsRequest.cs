using System.Collections.Generic;

namespace bbt.gateway.messaging.Api.dEngage.Model
{
    public class TransactionalSmsRequest
    {
        public TransactionalSmsContent content { get; set; }
        public TransactionalSmsSend send { get; set; }
        public List<string> tags{get;set;} = new();
    }

    public class TransactionalSmsContent
    {
        public string smsFromId{get;set;}
        public string message{get;set;}
    }

    public class TransactionalSmsSend
    {
        public string to{get;set;}
    }
}