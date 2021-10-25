using bbt.gateway.messaging.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace bbt.gateway.messaging.Workers
{
    public class HeaderManager
    {
        List<Header> headers = new List<Header>();

        private HeaderManager()
        {
            loadHeaders();
        }

        private static readonly Lazy<HeaderManager> lazy = new Lazy<HeaderManager>(() => new HeaderManager());
        public static HeaderManager Instance
        {
            get
            {
                return lazy.Value;
            }
        }

        public Header GetHeader(PhoneConfiguration config, MessageContentType contentType)
        {
            Header header = null;

            string businessLine = null;
            int? branch = null;

            if (config.CustomerNo != null)
            {
                // TODO: Find related headear record from customer no 
                // Assumption! using querying with customer no is better than phone number.
                header = getHeader(contentType, businessLine, branch);


            }
            else
            {
                // TODO: Find related headear record from phone number than update phone config
                var customerNo = 555;

                // set customer no of phone configruation for future save
                config.CustomerNo = customerNo;

                header = getHeader(contentType, businessLine, branch);
            }
            return header;
        }

        public void SaveHeader(Header header)
        {
            using (var db = new DatabaseContext())
            {

                if (header.Id == Guid.Empty)
                {
                    header.Id = Guid.NewGuid();
                    db.Headers.Add(header);
                }
                else
                {
                    db.Headers.Update(header);
                }
                db.SaveChanges();
            }
            
            //TODO: Meanwhile, dont forget to inform other pods to invalidate headers cahce.
            loadHeaders();
        }

        private Header getHeader(MessageContentType contentType, string businessLine, int? branch)
        {
            var firstPass = headers.Where(h => h.BusinessLine == businessLine && h.Branch == branch && h.ContentType == contentType).FirstOrDefault();
            if (firstPass != null) return firstPass;

            // Check branch first
            var secondPass = headers.Where(h => h.BusinessLine == null && h.Branch == branch && h.ContentType == contentType).FirstOrDefault();
            if (secondPass != null) return secondPass;

            var thirdPass = headers.Where(h => h.BusinessLine == businessLine && h.Branch == null && h.ContentType == contentType).FirstOrDefault();
            if (thirdPass != null) return thirdPass;

            var lastPass = headers.Where(h => h.BusinessLine == null && h.Branch == null && h.ContentType == contentType).FirstOrDefault();
            if (lastPass != null) return lastPass;

            // If db is not consistent, return firt value. Consider firing an enception 
            return headers.First();
        }

        private void loadHeaders()
        {
            using (var db = new DatabaseContext())
            {
                headers = db.Headers.ToList();
            }
        }


    }
}
