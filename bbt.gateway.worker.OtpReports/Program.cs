using bbt.gateway.common;
using bbt.gateway.common.Api.MessagingGateway;
using bbt.gateway.common.Helpers;
using bbt.gateway.worker.OtpReports;
using Elastic.Apm.NetCoreAll;
using Microsoft.EntityFrameworkCore;
using Refit;

IHost host = Host.CreateDefaultBuilder(args)
    .UseConsulSettings(typeof(Program))
    .UseSeriLog("entegrasyon")
    .ConfigureServices((context, services) =>
    {
        services.AddRefitClient<IMessagingGatewayApi>()
            .ConfigureHttpClient(c => c.BaseAddress = new Uri(context.Configuration["Api:ServiceUrl"]));
        services.AddHostedService<OtpWorker>();
        services.AddSingleton<DbContextOptions<DatabaseContext>>(new DbContextOptionsBuilder<DatabaseContext>()
                .UseSqlServer(context.Configuration.GetConnectionString("DefaultConnection"))
                .Options);
        services.AddSingleton<LogManager>();

    })
    .UseAllElasticApm()
    .Build();

await host.RunAsync();
