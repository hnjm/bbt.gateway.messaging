using bbt.gateway.common.Models.v2;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Filters;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Linq;

namespace bbt.gateway.messaging
{
    public class ConfigureSwaggerOptions : IConfigureNamedOptions<SwaggerGenOptions>
    {
        private readonly IApiVersionDescriptionProvider provider;

        public ConfigureSwaggerOptions(IApiVersionDescriptionProvider provider)
        {
            this.provider = provider;
        }

        public void Configure(SwaggerGenOptions options)
        {
            // add swagger document for every API version discovered
            foreach (var description in provider.ApiVersionDescriptions)
            {
                options.SwaggerDoc(description.GroupName, CreateInfoForApiVersion(description));
            }

            options.DocumentFilter<OrderTagsDocumentFilter>();
        }

        public void Configure(string name, SwaggerGenOptions options)
        {
            Configure(options);
        }

        private OpenApiInfo CreateInfoForApiVersion(ApiVersionDescription description)
        {
            var info = new OpenApiInfo()
            {
                Title = "Bbt.Messaging.Gateway",
                Version = description.ApiVersion.ToString()
            };

            if (description.IsDeprecated)
            {
                info.Description += "This API version has been deprecated.";
            }

            return info;
        }
    }

    public class OrderTagsDocumentFilter : IDocumentFilter
    {
        public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            swaggerDoc.Tags = swaggerDoc.Tags
                 .OrderBy(x => x.Name).ToList();
        }
    }

    public class TemplatedSmsRequestExampleFilter : IExamplesProvider<object>
    {
        public object GetExamples()
        {
            return new TemplatedSmsRequest
            {
                Sender = common.Models.SenderType.AutoDetect,
                Phone = new Phone() { CountryCode = 90, Prefix = 553, Number = 9495258 },
                Template = "Test",
                TemplateParams = "{\"test\":\"Sercan\"}",
                Process = new Process() { Name = "Integration", Identity = "U05587" }
            };

        }
    }
}