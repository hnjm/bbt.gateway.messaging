using bbt.gateway.common.Models;
using bbt.gateway.messaging.Workers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace bbt.gateway.messaging.Middlewares
{
    public class CustomerInfoMiddleware
    {
        private readonly RequestDelegate _next;

        public CustomerInfoMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context,ITransactionManager transactionManager)
        {
            context.Request.EnableBuffering();

            using (var reader = new StreamReader(
            context.Request.Body,
            encoding: Encoding.UTF8,
            detectEncodingFromByteOrderMarks: false,
            bufferSize: 1024,
            leaveOpen: true))
            {
                var body = await reader.ReadToEndAsync();
                var parsedBody = JsonConvert.DeserializeObject<MiddlewareRequest>(body);
                if (!string.IsNullOrEmpty(parsedBody.CustomerNo))
                {
                    try
                    {
                        var customerNo = Convert.ToInt64(parsedBody.CustomerNo);
                        await transactionManager.GetCustomerInfoByCustomerNo((ulong)customerNo);
                    }
                    catch (Exception ex)
                    {
                        transactionManager.LogError("Given Customer No is Not Valid. | "+ex.ToString());
                    }
                    
                }
                else
                {
                    if (parsedBody.Phone != null)
                    {
                        await transactionManager.GetCustomerInfoByPhone(parsedBody.Phone);
                    }
                    else
                    {
                        if (parsedBody.Email != null && !string.IsNullOrEmpty(parsedBody.Email))
                        {
                            await transactionManager.GetCustomerInfoByEmail(parsedBody.Email);
                        }
                    }
                }
                // Reset the request body stream position so the next middleware can read it
                context.Request.Body.Position = 0;
            }

            // Call the next delegate/middleware in the pipeline.
            await _next(context);
        }
    }

    public static class CustomerInfoMiddlewareExtensions
    {
        public static IApplicationBuilder UseCustomerInfoMiddleware(
            this IApplicationBuilder builder)
        {
            return builder.UseWhen(context => context.Request.Path.Value.IndexOf("/Messaging") != -1, builder =>
            {
                builder.UseMiddleware<CustomerInfoMiddleware>();
            });
        }
    }
}
