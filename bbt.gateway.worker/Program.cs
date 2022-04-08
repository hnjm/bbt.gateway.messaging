using bbt.gateway.worker;
using bbt.gateway.common;
using bbt.gateway.common.Repositories;
using Microsoft.EntityFrameworkCore;
using Refit;


IHost host = Host.CreateDefaultBuilder(args)
    .UseConsulSettings(typeof(Program))
    .UseSeriLog("entegrasyon")
    .ConfigureServices((context, services) =>
    {
        services.AddHostedService<Worker>();
        services.AddDbContext<DatabaseContext>(o => o.UseSqlServer(context.Configuration["ConnectionStrings:DefaultConnection"]), ServiceLifetime.Singleton);
        services.AddDbContext<DodgeDatabaseContext>(o => o.UseSqlServer(context.Configuration.GetConnectionString("DodgeConnection")), ServiceLifetime.Singleton);
        services.AddDbContext<SmsBankingDatabaseContext>(o => o.UseSqlServer(context.Configuration.GetConnectionString("SmsBankingConnection")), ServiceLifetime.Singleton);

        services.AddSingleton<IRepositoryManager, RepositoryManager>();

        services.AddRefitClient<IMessagingGatewayApi>()
            .ConfigureHttpClient(c => c.BaseAddress = new Uri(context.Configuration["Api:ServiceUrl"]));
    })
    .Build();


await host.RunAsync();
