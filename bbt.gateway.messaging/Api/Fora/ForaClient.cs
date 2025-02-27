﻿using bbt.gateway.messaging.Api.Fora.Model.Permission;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Threading.Tasks;

namespace bbt.gateway.messaging.Api.Fora
{
    public class ForaClient
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        public ForaClient(IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<PermissionResponse> getPermission(string CitizenshipNo)
        {
            PermissionResponse response = new();
            try
            {
                using var httpClient = _httpClientFactory.CreateClient("default");
                var res = await httpClient.GetAsync($"{_configuration.GetValue<string>("Api:Fora:EndPoints:GetPermission")}{CitizenshipNo}");

                response.ResponseCode = 0;
                response.ResponseMesssage = await res.Content.ReadAsStringAsync();
            }
            catch (System.Exception ex)
            {
                response.ResponseCode = -999;
                response.ResponseMesssage = ex.Message;
            }

            return response;
        }
    }
}
