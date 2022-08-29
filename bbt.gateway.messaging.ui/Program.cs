using bbt.gateway.common;
using bbt.gateway.messaging.ui.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components.Authorization;
using Radzen;
using Refit;
using Microsoft.AspNetCore.Authentication.Negotiate;
using System.Runtime.InteropServices;
using Microsoft.AspNetCore.Connections.Features;
using Microsoft.AspNetCore.Server.HttpSys;

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseConsulSettings(typeof(Program));

builder.Services.AddAuthentication(NegotiateDefaults.AuthenticationScheme)
        .AddNegotiate(options =>
        {

            options.PersistNtlmCredentials = true;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                options.EnableLdap("ebt.bank");
            }

            options.Events = new NegotiateEvents()
            {
                OnAuthenticationFailed = context =>
                {
                    if (context.Exception != null)
                        Console.WriteLine("Hata Events Mesajý :" + context.Exception.Message);

                    if (context.HttpContext != null && context.HttpContext.User != null && context.HttpContext.User.Identity != null)
                    {
                        Console.WriteLine("Identity Name:" + context.HttpContext.User.Identity.Name);
                    }
                    return Task.CompletedTask;
                }
            };


        });
builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = options.DefaultPolicy;
});
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddRefitClient<IMessagingGatewayService>()
               .ConfigureHttpClient(c => c.BaseAddress = new Uri(builder.Configuration["Api:MessagingGateway"]));
builder.Services.AddScoped<DialogService>();
builder.Services.AddAuthorizationCore();

var app = builder.Build();


// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}


app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
