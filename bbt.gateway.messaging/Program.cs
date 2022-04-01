
using Consul;
using Elasticsearch.Net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Sinks.Elasticsearch;
using System;
using Winton.Extensions.Configuration.Consul;

namespace bbt.gateway.messaging
{
    
    public class Program
    {
        public static void Main(string[] args)
        {
            Host.CreateDefaultBuilder(args)
            .ConfigureHostConfiguration((builder) => { builder.AddJsonFile("appsettings.Test.json", false, true)
                .AddUserSecrets(typeof(Program).Assembly).AddEnvironmentVariables(); })
            .ConfigureAppConfiguration((context, builder) =>
            {

                string consulHost = context.Configuration["ConsulHost"];
                string applicationName = context.HostingEnvironment.ApplicationName;
                string environmentName = context.HostingEnvironment.EnvironmentName;
                void ConsulConfig(ConsulClientConfiguration configuration)
                {
                    configuration.Token = context.Configuration["ConsulToken"];
                    configuration.Address = new Uri(consulHost);
                }

                builder.AddConsul($"{applicationName}/appsettings.json",
                    source =>
                    {
                        source.ReloadOnChange = true;
                        source.ConsulConfigurationOptions = ConsulConfig;
                    });
                builder.AddConsul($"{applicationName}/appsettings.{environmentName}.json",
                    source =>
                    {
                        source.Optional = true;
                        source.ConsulConfigurationOptions = ConsulConfig;
                    });

                IConfiguration configuration = builder.Build();
                ApiKeyAuthenticationCredentials k = new ApiKeyAuthenticationCredentials(configuration["ElasticSearch:ApiKey"]);
                Log.Logger = new LoggerConfiguration()
               .Enrich.FromLogContext()
               .Enrich.WithEnvironmentName()
               .Enrich.WithMachineName()
               .WriteTo.Console()
               .WriteTo.Debug()
               .WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri(configuration["ElasticSearch:Url"]))
               {
                   AutoRegisterTemplate = true,
                   ModifyConnectionSettings = x => x.ApiKeyAuthentication(k),
                   IndexFormat = $"nonprod-openshift-logstash-{DateTime.Now:yyyy-MM}",
               })
               .ReadFrom.Configuration(configuration)
               .CreateLogger();
            })
            .UseSerilog()
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>();
            }).Build().Run();
            
        }


    }
}
