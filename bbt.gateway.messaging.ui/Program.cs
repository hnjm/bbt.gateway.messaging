using bbt.gateway.common;
using bbt.gateway.messaging.ui.Data;
using Refit;

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseConsulSettings(typeof(Program));

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddRefitClient<IMessagingGatewayService>()
               .ConfigureHttpClient(c => c.BaseAddress = new Uri("https://test-messaginggateway.burgan.com.tr"));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}


app.UseStaticFiles();

app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
