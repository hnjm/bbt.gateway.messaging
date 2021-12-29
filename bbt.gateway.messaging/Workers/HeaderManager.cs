using bbt.gateway.messaging.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace bbt.gateway.messaging.Workers
{
    public class HeaderManager
    {
        private readonly DatabaseContext _databaseContext;
        public List<Header> Headers = new List<Header>();

        public HeaderManager(DatabaseContext databaseContext)
        {
            _databaseContext = databaseContext;
            loadHeaders();
        }

        public Header[] Get(int page, int pageSize)
        {

            Header[] returnValue;
            returnValue = _databaseContext.Headers
                .Skip(page * pageSize)
                .Take(pageSize)
                .ToArray();
            
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
                _databaseContext.Headers.Add(header);
            }
            else
            {
                _databaseContext.Headers.Update(header);
            }
            _databaseContext.SaveChanges();
            

            //TODO: Meanwhile, dont forget to inform other pods to invalidate headers cahce.
            loadHeaders();
        }

        public void Delete(Guid id)
        {
            
            var itemToDelete = new Header { Id = id };
            _databaseContext.Remove(itemToDelete);
            _databaseContext.SaveChanges();

            //TODO: Meanwhile, dont forget to inform other pods to invalidate headers cahce.
            loadHeaders();
        }

        private Header get(MessageContentType contentType, string businessLine, int? branch)
        {
            var firstPass = Headers.Where(h => h.BusinessLine == businessLine && h.Branch == branch && h.ContentType == contentType).FirstOrDefault();
            if (firstPass != null) return firstPass;

            // Check branch first
            var secondPass = Headers.Where(h => h.BusinessLine == null && h.Branch == branch && h.ContentType == contentType).FirstOrDefault();
            if (secondPass != null) return secondPass;

            var thirdPass = Headers.Where(h => h.BusinessLine == businessLine && h.Branch == null && h.ContentType == contentType).FirstOrDefault();
            if (thirdPass != null) return thirdPass;

            var lastPass = Headers.Where(h => h.BusinessLine == null && h.Branch == null && h.ContentType == contentType).FirstOrDefault();
            if (lastPass != null) return lastPass;

            //TODO: If db is not consistent, return firt value. Consider firing an enception 
            return Headers.First();
        }

        private void loadHeaders()
        {
            
            Headers = _databaseContext.Headers.ToList();
            
        }


    }
}
