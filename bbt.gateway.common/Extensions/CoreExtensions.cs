using Consul;
using Elasticsearch.Net;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Sinks.Elasticsearch;
using Winton.Extensions.Configuration.Consul;
using Dapr.Client;
using Dapr.Extensions.Configuration;
using System.Text;
using VaultSharp.V1.AuthMethods;
using VaultSharp.V1.AuthMethods.Token;
using VaultSharp;

namespace bbt.gateway.common
{
    public static class CoreExtensions
    {
        /// <summary>
        /// Read Appsettings From Consule<br />
        /// ConsulHost and ConsulToken have to be set in appsettings
        /// </summary>
        /// <param name="host"></param>
        /// <param name="type">Type of Main | usage : typeof(Program)</param>
        /// <returns></returns>
        public static IHostBuilder UseConsulSettings(this IHostBuilder host,Type type,string fullPath = null)
        {
            

            return host.ConfigureAppConfiguration((context, builder) =>
            {
                string applicationName = fullPath ?? context.HostingEnvironment.ApplicationName;
                string environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";

                builder.AddJsonFile($"appsettings.{environmentName}.json", false, true)
                .AddUserSecrets(type.Assembly).AddEnvironmentVariables();

                if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Mock")
                    return;

                context.Configuration = builder.Build();

                string consulHost = context.Configuration["ConsulHost"];
                
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
                
            });

        }

        /// <summary>
        /// Set Serilog Configuration To Logging Elastic Search<br />
        /// ElasticSearch:ApiKey and ElasticSearch:Url have to be set in appsettings
        /// </summary>
        /// <param name="host"></param>
        /// <param name="indexFormat">Index Format for Elastic Search</param>
        /// <returns></returns>
        public static IHostBuilder UseSeriLog(this IHostBuilder host, string indexFormat)
        {
            if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Mock")
                return host;

            return host.ConfigureAppConfiguration((context, builder) =>
            {
                string environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
                var configuration = builder.Build();
                Serilog.Debugging.SelfLog.Enable(msg => Console.WriteLine(msg));
                ApiKeyAuthenticationCredentials k = new ApiKeyAuthenticationCredentials(configuration["ElasticSearch:ApiKey"]);
                indexFormat = (environmentName != "Prod" ? "nonprod-" : "prod-") + indexFormat;
                Log.Logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .Enrich.WithEnvironmentName()
                .Enrich.WithMachineName()
                .WriteTo.Console()
                .WriteTo.Debug()
                .WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri(configuration["ElasticSearch:Url"])) { 
                    IndexFormat = indexFormat+"-{0:yyyy-MM}",
                    ModifyConnectionSettings = c => c.ApiKeyAuthentication(k)
                })
                .ReadFrom.Configuration(configuration)
                .CreateLogger();
            }).UseSerilog();

        }

        /// <summary>
        /// Read Appsettings/Secret From Vault<br />
        /// </summary>
        /// <param name="builder">Configuration Builder</param>
        /// <param name="secretStoreName">Dapr Secret Store Component Name</param>
        /// <param name="secretPath">Vault Secret Path</param>
        /// <param name="key">Secret Key | Default : appsettings</param>
        /// <returns></returns>
        public static async Task<IConfigurationBuilder> UseVaultSecretsAsync(this IConfigurationBuilder builder, string secretStoreName,string secretPath, string key = "appsettings")
        {
           
            var daprClient = new DaprClientBuilder().Build();

            var secret = await daprClient.GetSecretAsync(secretStoreName, secretPath);

            builder.AddJsonStream(new MemoryStream(Encoding.UTF8.GetBytes(secret[key])));

            return builder;

        }
    }
}
