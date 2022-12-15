using bbt.gateway.common;
using bbt.gateway.common.Api.MessagingGateway;
using bbt.gateway.common.Helpers;
using bbt.gateway.worker.SmsReports;
using Elastic.Apm.NetCoreAll;
using Microsoft.EntityFrameworkCore;
using Refit;
using System.Text.Json;

IHost host = Host.CreateDefaultBuilder(args)
    .UseConsulSettings(typeof(Program),"bbt.gateway.worker")
    .UseSeriLog("entegrasyon")
    .ConfigureServices((context, services) =>
    {        
        services.AddRefitClient<IMessagingGatewayApi>()
            .ConfigureHttpClient(c => c.BaseAddress = new Uri(context.Configuration["Api:ServiceUrl"]));
        services.AddHostedService<SmsWorker>();
        services.AddSingleton<DbContextOptions<DatabaseContext>>(new DbContextOptionsBuilder<DatabaseContext>()
                .UseSqlServer(context.Configuration.GetConnectionString("DefaultConnection")).UseLoggerFactory(LoggerFactory.Create(b => b.AddConsole()))
                .Options);
        services.AddSingleton<LogManager>();

    })
    .UseAllElasticApm()
    .Build();

await host.RunAsync();

