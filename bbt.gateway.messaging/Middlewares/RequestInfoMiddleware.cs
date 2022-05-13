using bbt.gateway.common.Models;
using bbt.gateway.common.Repositories;
using bbt.gateway.messaging.Workers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.IO;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bbt.gateway.messaging.Middlewares
{
    public class RequestInfoMiddleware
    {
        private readonly RequestDelegate _next;
        
        public RequestInfoMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context,ITransactionManager transactionManager)
        {            
            await _next(context);
        }

    }


    public static class RequestInfoMiddlewareExtensions
    {
        public static IApplicationBuilder UseRequestInfoMiddleware(
            this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<RequestInfoMiddleware>();
        }
    }
}
