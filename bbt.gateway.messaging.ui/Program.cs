using bbt.gateway.common;
using bbt.gateway.messaging.ui.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components.Authorization;
using Radzen;
using Refit;
using Microsoft.AspNetCore.Authentication.Negotiate;
using System.Runtime.InteropServices;
var builder = WebApplication.CreateBuilder(args);
builder.Host.UseConsulSettings(typeof(Program));

// Add services to the container.

builder.Services.AddAuthentication(NegotiateDefaults.AuthenticationScheme)
        .AddNegotiate(options =>
        {
            options.EnableLdap(settings =>
            {
                settings.Domain = "ebt.bank";

            });

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
