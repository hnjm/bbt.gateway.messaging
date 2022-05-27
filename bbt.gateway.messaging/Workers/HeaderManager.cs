using bbt.gateway.common.Models;
using bbt.gateway.common.Repositories;
using bbt.gateway.messaging.Api.Pusula;
using System;
using System.Collections.Generic;
using System.Linq;

namespace bbt.gateway.messaging.Workers
{
    public class HeaderManager
    {
        public List<Header> Headers = new List<Header>();
        private readonly ITransactionManager _transactionManager;
        private readonly IRepositoryManager _repositoryManager;
        private ulong _customerNo;

        public long CustomerNo { get { return (long)_customerNo; } }

        public HeaderManager(IRepositoryManager repositoryManager, 
            ITransactionManager transactionManager
            )
        {
            _transactionManager = transactionManager;
            _repositoryManager = repositoryManager;
            loadHeaders();
        }

        public Header[] Get(int page, int pageSize)
        {
            Header[] returnValue;
            returnValue = _repositoryManager.Headers.GetWithPagination(page, pageSize).ToArray();

            return returnValue;
        }

        public Header Get(MessageContentType contentType, HeaderInfo headerInfo)
        {
            Header header = null;

            if (headerInfo.Sender != SenderType.AutoDetect)
            {
                _transactionManager.CustomerRequestInfo.BusinessLine = headerInfo.Sender == SenderType.On ? "X" : "B";
                var defaultHeader = new Header();
                defaultHeader.SmsSender = headerInfo.Sender;
                defaultHeader.SmsPrefix = headerInfo.SmsPrefix;
                defaultHeader.SmsSuffix = headerInfo.SmsSuffix;

                defaultHeader.SmsTemplatePrefix = headerInfo.SmsTemplatePrefix;
                defaultHeader.SmsTemplateSuffix = headerInfo.SmsTemplateSuffix;

                defaultHeader.EmailTemplatePrefix = headerInfo.EmailTemplatePrefix;
                defaultHeader.EmailTemplateSuffix = headerInfo.EmailTemplateSuffix;
                return defaultHeader;
            }

            string businessLine = string.IsNullOrEmpty(_transactionManager.CustomerRequestInfo.BusinessLine) ? null : _transactionManager.CustomerRequestInfo.BusinessLine;
            int? branch = _transactionManager.CustomerRequestInfo.BranchCode != 0 ? _transactionManager.CustomerRequestInfo.BranchCode : null;



            header = get(contentType, businessLine, branch);

            return header;
        }

        public void Save(Header header)
        {

            if (header.Id == Guid.Empty)
            {
                header.Id = Guid.NewGuid();
                _repositoryManager.Headers.Add(header);
            }
            else
            {
                _repositoryManager.Headers.Update(header);
            }

            _repositoryManager.SaveChanges();
            loadHeaders();
        }

        public void Delete(Guid id)
        {
            var itemToDelete = new Header { Id = id };
            _repositoryManager.Headers.Remove(itemToDelete);

            loadHeaders();
        }

        private Header get(MessageContentType contentType, string businessLine, int? branch)
        {

            var firstPass = _repositoryManager.Headers.Find(h => h.BusinessLine == businessLine && h.Branch == branch && h.ContentType == contentType).FirstOrDefault();
            if (firstPass != null) return firstPass;

            // Check branch first
            var secondPass = _repositoryManager.Headers.Find(h => h.BusinessLine == null && h.Branch == branch && h.ContentType == contentType).FirstOrDefault();
            if (secondPass != null) return secondPass;

            var thirdPass = _repositoryManager.Headers.Find(h => h.BusinessLine == businessLine && h.Branch == null && h.ContentType == contentType).FirstOrDefault();
            if (thirdPass != null) return thirdPass;

            var lastPass = _repositoryManager.Headers.Find(h => h.BusinessLine == null && h.Branch == null && h.ContentType == contentType).FirstOrDefault();
            if (lastPass != null) return lastPass;



            //TODO: If db is not consistent, return firt value. Consider firing an enception 
            //LOGGING HEADER BULUNAMADI
            var header = new Header()
            {
                EmailTemplatePrefix = "",
                EmailTemplateSuffix = "",
                SmsPrefix = "",
                SmsSuffix = "",
                SmsSender = SenderType.Burgan,
                SmsTemplatePrefix = "",
                SmsTemplateSuffix = ""
            };



            return header;
        }

        private void loadHeaders()
        {
            Headers = _repositoryManager.Headers.GetAll().ToList();

        }


    }
}
