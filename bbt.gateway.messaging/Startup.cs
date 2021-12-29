using bbt.gateway.messaging.Api.TurkTelekom;
using bbt.gateway.messaging.Models;
using bbt.gateway.messaging.Workers;
using bbt.gateway.messaging.Workers.OperatorGateway;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

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
            services.AddControllers()
                    //.AddJsonOptions(opts =>
                    //{
                    //    var enumConverter = new JsonStringEnumConverter();
                    //    opts.JsonSerializerOptions.Converters.Add(enumConverter);
                    //    opts.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
                    //    opts.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
                    //});
                    .AddNewtonsoftJson();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "bbt.gateway.messaging", Version = "v1" });
                c.EnableAnnotations();
                //TODO: is process info came from header or body ? Decide
                //c.OperationFilter<AddRequiredHeaderParameter>();
            });

            services.AddTransient<OperatorTurkTelekom>();
            services.AddTransient<OperatorVodafone>();
            services.AddTransient<OperatorTurkcell>();
            services.AddTransient<OperatorIVN>();
            services.AddTransient<Func<OperatorType, IOperatorGateway>>(serviceProvider => key =>
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

            services.AddDbContext<DatabaseContext>(
                options => options.UseSqlServer(Environment.GetEnvironmentVariable("SQL_CONNECTION")),ServiceLifetime.Transient);

            services.AddScoped<OtpSender>();
            services.AddScoped<HeaderManager>();
            services.AddScoped<OperatorManager>();
            services.AddScoped<TurkTelekomApi>();
           
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "bbt.gateway.messaging v1");
                    c.RoutePrefix = "";
                    });
            }

            //app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
