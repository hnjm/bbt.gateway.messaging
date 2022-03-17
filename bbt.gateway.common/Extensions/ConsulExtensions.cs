using bbt.gateway.common.Models.Settings;
using Consul;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;


namespace bbt.gateway.common.Models.Extensions
{
    public static class ConsulExtensions
    {
        public static IServiceCollection AddConsulConfig(this IServiceCollection services, ConsulSettings consulSettings)
        {
            services.AddSingleton<IConsulClient, ConsulClient>(p => new ConsulClient(consulConfig =>
            {
                consulConfig.Address = new Uri(consulSettings.ConsoleHost);
                consulConfig.Token = consulSettings.Token;
            }));
            return services;
        }

        public static IApplicationBuilder UseConsul(this IApplicationBuilder app, ConsulSettings consulSettings)
        {
            var consulClient = app.ApplicationServices.GetRequiredService<IConsulClient>();
            var logger = app.ApplicationServices.GetRequiredService<ILoggerFactory>().CreateLogger("AppExtensions");
            var lifetime = app.ApplicationServices.GetRequiredService<IApplicationLifetime>();

            IConfiguration configuration = app.ApplicationServices.GetRequiredService<IConfiguration>();

            var server = app.ApplicationServices.GetRequiredService<IServer>();
            var addressFeature = server.Features.Get<IServerAddressesFeature>();

            if (!(app.ServerFeatures is FeatureCollection features)) return app;

            var addresses = features.Get<IServerAddressesFeature>();
            var address = addresses.Addresses.First();
            Console.WriteLine("gelen adres : " +address);
            var uri = new Uri(address);


            AgentCheckRegistration httpCheck = new AgentCheckRegistration()
            {
                HTTP = $"{uri.Scheme}://{uri.Host}:{uri.Port}/{consulSettings.HealthyCheckURL}",

                Notes = "Checks hc",
                Timeout = TimeSpan.FromSeconds(3),
                Interval = TimeSpan.FromSeconds(10)
            };

            var registration = new AgentServiceRegistration()
            {
                ID = $"{consulSettings.ServiceName}-{uri.Port}",
                // servie name  
                Name = consulSettings.ServiceName,
                Address = uri.Host,
                Port = uri.Port,
                Checks = new[] { httpCheck }
            };

            logger.LogInformation("Registering with Consul");
            consulClient.Agent.ServiceDeregister(registration.ID).ConfigureAwait(true);
            consulClient.Agent.ServiceRegister(registration).ConfigureAwait(true);

            lifetime.ApplicationStopping.Register(() =>
            {
                logger.LogInformation("Unregistering from Consul");
                consulClient.Agent.ServiceDeregister(registration.ID).ConfigureAwait(true);
            });

            return app;
        }
    }
}
