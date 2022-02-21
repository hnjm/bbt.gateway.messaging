using bbt.gateway.worker;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using bbt.gateway.common;
using bbt.gateway.common.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using Refit;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddHostedService<Worker>();
        services.AddDbContext<DatabaseContext>(o => o.UseNpgsql(Environment.GetEnvironmentVariable("SQL_CONNECTION")),ServiceLifetime.Singleton);
        services.AddSingleton<IRepositoryManager, RepositoryManager>();
        services.AddRefitClient<IMessagingGatewayApi>()
            .ConfigureHttpClient(c => c.BaseAddress = new Uri("https://localhost:5001"));
    })
    .Build();

await host.RunAsync();
