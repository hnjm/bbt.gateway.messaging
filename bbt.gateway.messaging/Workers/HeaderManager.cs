using bbt.gateway.common.Models;
using bbt.gateway.common.Repositories;
using bbt.gateway.messaging.Api.Pusula;
using bbt.gateway.messaging.Api.Pusula.Model.GetByPhone;
using bbt.gateway.messaging.Api.Pusula.Model.GetCustomer;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace bbt.gateway.messaging.Workers
{
    public class HeaderManager
    {
        public List<Header> Headers = new List<Header>();
        private readonly IRepositoryManager _repositoryManager;
        private readonly PusulaClient _pusulaClient;

        public HeaderManager(IRepositoryManager repositoryManager, PusulaClient pusulaClient)
        {
            _repositoryManager = repositoryManager;
            _pusulaClient = pusulaClient;
            loadHeaders();
        }

        public Header[] Get(int page, int pageSize)
        {
            Header[] returnValue;
            returnValue = _repositoryManager.Headers.GetWithPagination(page, pageSize).ToArray();
            
            return returnValue;
        }

        public async Task<Header> Get(PhoneConfiguration config, MessageContentType contentType)
        {
            Header header = null;

            string businessLine = null;
            int? branch = null;

            if (config.CustomerNo != null)
            {

                var customerDetail = await _pusulaClient.GetCustomer(new GetCustomerRequest()
                {
                    CustomerNo = config.CustomerNo.Value
                });

                if (customerDetail.IsSuccess)
                {
                    businessLine = customerDetail.BusinessLine;
                    branch = customerDetail.BranchCode;
                }

                header = get(contentType, businessLine, branch);

            }
            else
            {
                var customer = await _pusulaClient.GetCustomerByPhoneNumber(new GetByPhoneNumberRequest() { 
                    CountryCode = config.Phone.CountryCode,
                    CityCode = config.Phone.Prefix,
                    TelephoneNumber = config.Phone.Number
                });

                if (customer.IsSuccess)
                {
                    var customerDetail = await _pusulaClient.GetCustomer(new GetCustomerRequest()
                    {
                        CustomerNo = customer.CustomerNo
                    });

                    if (customerDetail.IsSuccess)
                    {
                        // set customer no of phone configruation for future save
                        config.CustomerNo = customer.CustomerNo;

                        businessLine = customerDetail.BusinessLine;
                        branch = customerDetail.BranchCode;
                    }
                }

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
