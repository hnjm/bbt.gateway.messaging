
using bbt.gateway.common;
using bbt.gateway.common.Models;
using bbt.gateway.common.Models.v1;
using bbt.gateway.common.Repositories;
using bbt.gateway.common.Api.dEngage;
using bbt.gateway.messaging.Api.Fora;
using bbt.gateway.messaging.Api.Pusula;
using bbt.gateway.messaging.Api.Turkcell;
using bbt.gateway.messaging.Api.TurkTelekom;
using bbt.gateway.messaging.Api.Vodafone;
using bbt.gateway.messaging.Helpers;
using bbt.gateway.messaging.Middlewares;
using bbt.gateway.messaging.Workers;
using bbt.gateway.messaging.Workers.OperatorGateway;
using Elastic.Apm.NetCoreAll;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Refit;
using Swashbuckle.AspNetCore.Filters;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace bbt.gateway.messaging
{
    
    public class Program
    {

        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Host.UseConsulSettings(typeof(Program));
            var startup = new Startup(builder.Configuration);
            startup.ConfigureServices(builder.Services);
            var app = builder.Build();
            startup.Configure(app, builder.Environment,app.Services.GetService<IApiVersionDescriptionProvider>());
            app.Run();
        }

    }
}
