using bbt.gateway.messaging.Api.Pusula.Model.GetByPhone;
using bbt.gateway.messaging.Api.Pusula.Model.GetCustomer;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace bbt.gateway.messaging.Api.Pusula
{
    public class PusulaClient
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<PusulaClient> _logger;

        public PusulaClient(IConfiguration configuration,ILogger<PusulaClient> logger)
        {
            _configuration = configuration;
            _logger = logger;
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri(_configuration.GetValue<string>("Api:Pusula:BaseAddress"));
        }

        public async Task<GetByPhoneNumberResponse> GetCustomerByPhoneNumber(GetByPhoneNumberRequest getByPhoneNumberRequest)
        {
            GetByPhoneNumberResponse getByPhoneNumberResponse = new();
            try
            {
                var queryParams = new Dictionary<string, string>()
                {
                    {"CountryCode", getByPhoneNumberRequest.CountryCode.ToString()},
                    {"CityCode",getByPhoneNumberRequest.CityCode.ToString()},
                    {"TelephoneNumber",getByPhoneNumberRequest.TelephoneNumber.ToString()}
                };
            
                var httpResponse = await _httpClient.GetAsync(
                    QueryHelpers.AddQueryString(_configuration.GetValue<string>("Api:Pusula:EndPoints:GetByPhoneNumber"),queryParams));
                

                if (httpResponse.IsSuccessStatusCode)
                {
                    var response = httpResponse.Content.ReadAsStringAsync().Result.DeserializeXml<DataTable>();
                    if (response.diffgram.DocumentElement != null)
                    {
                        getByPhoneNumberResponse.IsSuccess = true;
                        getByPhoneNumberResponse.CustomerNo = response.diffgram.DocumentElement[0].CustomerNumber;
                    }
                    else
                    {
                        getByPhoneNumberResponse.IsSuccess = false;
                    }
                }
                else
                {
                    _logger.LogError("Pusula Client Hata Oluştu." + httpResponse.Content.ReadAsStringAsync().Result);
                    getByPhoneNumberResponse.IsSuccess = false;
                }
            }
            catch (Exception ex)
            {
                getByPhoneNumberResponse.IsSuccess = false;
                _logger.LogError("Pusula Client Hata Oluştu." +ex.ToString());
            }

            return getByPhoneNumberResponse;
        }

        public async Task<GetByEmailResponse> GetCustomerByEmail(GetByEmailRequest getByEmailRequest)
        {
            GetByEmailResponse getByEmailResponse = new();
            try
            {
                var queryParams = new Dictionary<string, string>()
                {
                    {"eMail", getByEmailRequest.Email}
                };

                var httpResponse = await _httpClient.GetAsync(
                    QueryHelpers.AddQueryString(_configuration.GetValue<string>("Api:Pusula:EndPoints:GetByEmail"), queryParams));


                if (httpResponse.IsSuccessStatusCode)
                {
                    var customerNo = (long)System.Xml.Linq.XElement.Parse(httpResponse.Content.ReadAsStringAsync().Result);
                    if (customerNo > 0 && customerNo != 55)
                    {
                        getByEmailResponse.IsSuccess = true;
                        getByEmailResponse.CustomerNo = (ulong)customerNo;
                    }
                    else 
                    {
                        getByEmailResponse.IsSuccess = false;
                        //logging
                    }
                }
                else
                {
                    _logger.LogError("Pusula Client Hata Oluştu." + httpResponse.Content.ReadAsStringAsync().Result);
                    getByEmailResponse.IsSuccess = false;
                }
            }
            catch (Exception ex)
            {
                getByEmailResponse.IsSuccess = false;
                _logger.LogError("Pusula Client Hata Oluştu." + ex.ToString());
            }

            return getByEmailResponse;
        }

        public async Task<GetCustomerResponse> GetCustomer(GetCustomerRequest getCustomerRequest)
        {
            GetCustomerResponse getCustomerResponse = new();
            try
            {
                var queryParams = new Dictionary<string, string>()
                {
                    {"custNo", getCustomerRequest.CustomerNo.ToString()},
                };
                var t = QueryHelpers.AddQueryString(_configuration.GetValue<string>("Api:Pusula:EndPoints:GetCustomer"), queryParams);
                var httpResponse = await _httpClient.GetAsync(
                    QueryHelpers.AddQueryString(_configuration.GetValue<string>("Api:Pusula:EndPoints:GetCustomer"), queryParams));

                if (httpResponse.IsSuccessStatusCode)
                {
                    var response = httpResponse.Content.ReadAsStringAsync().Result.DeserializeXml<DataSet>();
                    if (response.diffgram.CusInfo != null)
                    {
                        getCustomerResponse.IsSuccess = true;
                        getCustomerResponse.BranchCode = response.diffgram.CusInfo.CustomerIndividual.MainBranchCode;
                        getCustomerResponse.BusinessLine = response.diffgram.CusInfo.CustomerIndividual.BusinessLine;
                    }
                    else
                    {
                        getCustomerResponse.IsSuccess = false;
                    }
                }
                else
                {
                    _logger.LogError("Pusula Client Hata Oluştu." + httpResponse.Content.ReadAsStringAsync().Result);
                    getCustomerResponse.IsSuccess = false;
                }
            }
            catch (Exception ex)
            {
                getCustomerResponse.IsSuccess = false;
                _logger.LogError("Pusula Client Hata Oluştu." + ex.ToString());
            }

            return getCustomerResponse;
        }
    }
}
