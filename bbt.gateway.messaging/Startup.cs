using bbt.gateway.common;
using bbt.gateway.common.Models;
using bbt.gateway.common.Repositories;
using bbt.gateway.messaging.Api.dEngage;
using bbt.gateway.messaging.Api.Pusula;
using bbt.gateway.messaging.Api.Turkcell;
using bbt.gateway.messaging.Api.TurkTelekom;
using bbt.gateway.messaging.Api.Vodafone;
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
using System;
using System.Collections.Generic;

namespace bbt.gateway.messaging
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHealthChecks();

            services.AddControllers()
                    .AddNewtonsoftJson(opts =>
                    {
                        opts.SerializerSettings.Converters.Add(new StringEnumConverter());
                        opts.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                    });

            JsonSerializerSettings settings = new();
            settings.Converters.Add(new StringEnumConverter());
            settings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            JsonConvert.DefaultSettings = () => settings;

            services.AddApiVersioning(v =>
            {
                v.DefaultApiVersion = new Microsoft.AspNetCore.Mvc.ApiVersion(1, 0);
                v.AssumeDefaultVersionWhenUnspecified = true;
            });

            services.AddVersionedApiExplorer(setup =>
            {
                setup.GroupNameFormat = "'v'VVV";
                setup.SubstituteApiVersionInUrl = true;
            });

            services.AddSwaggerGen(c =>
            {
                //c.SwaggerDoc("v1", new OpenApiInfo { Title = "bbt.gateway.messaging", Version = "v1" });
                c.EnableAnnotations();
                //TODO: is process info came from header or body ? Decide
                //c.OperationFilter<AddRequiredHeaderParameter>();
            });
            services.AddSwaggerGenNewtonsoftSupport();
            services.ConfigureOptions<ConfigureSwaggerOptions>();

            services.AddDbContext<DatabaseContext>(o => o.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"), b => b.MigrationsAssembly("bbt.gateway.messaging")));
            //services.AddDbContext<DodgeDatabaseContext>(o => o.UseSqlServer(Configuration.GetConnectionString("DodgeConnection")));
            services.AddDbContext<SmsBankingDatabaseContext>(o => o.UseSqlServer(Configuration.GetConnectionString("SmsBankingConnection")));

            services.AddScoped<IRepositoryManager, RepositoryManager>();


            services.AddRefitClient<IdEngageClient>(new RefitSettings
            {
                ContentSerializer = new NewtonsoftJsonContentSerializer(
                        new JsonSerializerSettings()
                        {
                            NullValueHandling = NullValueHandling.Ignore,
                        }
                )
            })
               .ConfigureHttpClient(c => c.BaseAddress = new Uri(Configuration["Api:dEngage:BaseAddress"]));

            services.AddScoped<ITransactionManager, TransactionManager>();
            services.AddScoped<OperatorTurkTelekom>();
            services.AddScoped<OperatorVodafone>();
            services.AddScoped<OperatorTurkcell>();
            services.AddScoped<OperatorIVN>();
            services.AddScoped<OperatordEngage>();
            services.AddScoped<Func<OperatorType, IOperatorGateway>>(serviceProvider => key =>
            {
                switch (key)
                {
                    case OperatorType.Turkcell:
                        return serviceProvider.GetService<OperatorTurkcell>();
                    case OperatorType.Vodafone:
                        return serviceProvider.GetService<OperatorVodafone>();
                    case OperatorType.TurkTelekom:
                        return serviceProvider.GetService<OperatorTurkTelekom>();
                    case OperatorType.IVN:
                        return serviceProvider.GetService<OperatorIVN>();
                    default:
                        throw new KeyNotFoundException();
                }
            });

            services.AddScoped<ITurkTelekomApi, TurkTelekomApi>();
            services.AddScoped<IVodafoneApi, VodafoneApi>();
            services.AddScoped<ITurkcellApi, TurkcellApi>();

            services.AddScoped<OtpSender>();
            services.AddScoped<dEngageSender>();
            services.AddScoped<HeaderManager>();
            services.AddScoped<OperatorManager>();
            services.AddScoped<OperatorIVN>();
            services.AddScoped<PusulaClient>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IApiVersionDescriptionProvider provider)
        {
            app.UseCustomerInfoMiddleware();
            app.UseRequestInfoMiddleware();

            app.UseSwagger();

            app.UseSwaggerUI(options =>
            {
                foreach (var description in provider.ApiVersionDescriptions)
                {
                    options.SwaggerEndpoint(
                        $"/swagger/{description.GroupName}/swagger.json",
                        description.GroupName.ToUpperInvariant());
                    options.RoutePrefix = "";
                }
            });


            app.UseHealthChecks("/hc", new HealthCheckOptions()
            {
                ResultStatusCodes =
            {
                [Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Healthy] = StatusCodes.Status200OK,
                [Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Degraded] = StatusCodes.Status503ServiceUnavailable,
                [Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable
            }
            });

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            app.UseAllElasticApm(Configuration);

        }
    }

}
