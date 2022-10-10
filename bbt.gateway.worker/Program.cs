using bbt.gateway.common;
using bbt.gateway.common.Repositories;
using bbt.gateway.worker;
using Elastic.Apm.NetCoreAll;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Refit;

IHost host = Host.CreateDefaultBuilder(args)
    .UseConsulSettings(typeof(Program))
    .UseSeriLog("entegrasyon")
    .ConfigureServices((context, services) =>
    {
        services.AddHostedService<TemplateWorker>();
        //services.AddHostedService<OtpWorker>();
        //services.AddHostedService<SmsWorker>();
        services.AddSingleton<DbContextOptions<DatabaseContext>>(new DbContextOptionsBuilder<DatabaseContext>()
                .UseSqlServer(context.Configuration.GetConnectionString("DefaultConnection"))
                .Options);
        services.AddSingleton<LogManager>();

        services.AddRefitClient<IMessagingGatewayApi>()
            .ConfigureHttpClient(c => c.BaseAddress = new Uri(context.Configuration["Api:ServiceUrl"]));
    })
    .UseAllElasticApm()
    .Build();

await host.RunAsync();
