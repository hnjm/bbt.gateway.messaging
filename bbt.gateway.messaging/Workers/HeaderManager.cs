using bbt.gateway.messaging.Models;
using bbt.gateway.messaging.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;


namespace bbt.gateway.messaging.Workers
{
    public class HeaderManager
    {
        public List<Header> Headers = new List<Header>();
        private readonly IRepositoryManager _repositoryManager;

        public HeaderManager(IRepositoryManager repositoryManager)
        {
            _repositoryManager = repositoryManager;
            loadHeaders();
        }

        public Header[] Get(int page, int pageSize)
        {
            Header[] returnValue;
            returnValue = _repositoryManager.Headers.GetWithPagination(page, pageSize).ToArray();
            
            return returnValue;
        }

        public Header Get(PhoneConfiguration config, MessageContentType contentType)
        {
            Header header = null;

            string businessLine = null;
            int? branch = null;

            if (config.CustomerNo != null)
            {
                // TODO: Find related headear record from customer no 
                // Assumption! using querying with customer no is better than phone number.
                header = get(contentType, businessLine, branch);


            }
            else
            {
                // TODO: Find related headear record from phone number than update phone config
                var customerNo = 555;

                // set customer no of phone configruation for future save
                config.CustomerNo = customerNo;

                header = get(contentType, businessLine, branch);
            }
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
            //TODO: Meanwhile, dont forget to inform other pods to invalidate headers cahce.
            //loadHeaders();
        }

        public void Delete(Guid id)
        {
            var itemToDelete = new Header { Id = id };
            _repositoryManager.Headers.Remove(itemToDelete);

            //TODO: Meanwhile, dont forget to inform other pods to invalidate headers cahce.
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
            return _repositoryManager.Headers.FirstOrDefault();
        }

        private void loadHeaders()
        {
            Headers = _repositoryManager.Headers.GetAll().ToList();
            
        }


    }
}
