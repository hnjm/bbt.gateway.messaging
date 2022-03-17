using bbt.gateway.worker;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using bbt.gateway.common;
using bbt.gateway.common.Repositories;
using Microsoft.EntityFrameworkCore;
using Refit;
using Consul;
using Winton.Extensions.Configuration.Consul;
using System;
using Microsoft.Extensions.Configuration;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureHostConfiguration(builder => { builder.AddJsonFile("appsettings.Test.json", false, true); })
    //.ConfigureAppConfiguration((context, builder) => {
    //    string consulHost = context.Configuration["ConsulHost"];
    //    string applicationName = context.HostingEnvironment.ApplicationName;
    //    string environmentName = context.HostingEnvironment.EnvironmentName;
    //    void ConsulConfig(ConsulClientConfiguration configuration)
    //    {
    //        configuration.Address = new Uri(consulHost);
    //    }

    //    builder.AddConsul($"{applicationName}/appsettings.json",
    //        source =>
    //        {
    //            source.ReloadOnChange = true;
    //            source.ConsulConfigurationOptions = ConsulConfig;
    //        });
    //    builder.AddConsul($"{applicationName}/appsettings.{environmentName}.json",
    //        source =>
    //        {
    //            source.Optional = true;
    //            source.ConsulConfigurationOptions = ConsulConfig;
    //        });
    //    configuration = builder.Build();
    //})
    .ConfigureServices((context, services) =>
    {
        services.AddHostedService<Worker>();
        services.AddDbContext<DatabaseContext>(o => o.UseNpgsql(context.Configuration["ConnectionStrings:DefaultConnection"]), ServiceLifetime.Singleton);
        services.AddSingleton<IRepositoryManager, RepositoryManager>();
        services.AddRefitClient<IMessagingGatewayApi>()
            .ConfigureHttpClient(c => c.BaseAddress = new Uri(context.Configuration["Api:ServiceUrl"]));
    })
    .Build();


await host.RunAsync();
