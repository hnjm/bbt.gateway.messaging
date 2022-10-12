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
using Auth0.AspNetCore.Authentication;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Okta.AspNetCore;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Logging;
using System.Text;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using bbt.gateway.messaging.ui.Pages.Authorize;
using bbt.gateway.messaging.ui.Base.Administration;
using bbt.gateway.messaging.ui.Base.Token;
using bbt.gateway.messaging.ui.Base;

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseConsulSettings(typeof(Program));


builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<bbt.gateway.messaging.ui.Data.HttpContextAccessor>();
//builder.Services.Configure<CookiePolicyOptions>(options =>
//{

//    // this lambda determines whether user consent for non-essential cookies is needed for a given request.
//    options.CheckConsentNeeded = context => true;
//    options.MinimumSameSitePolicy = SameSiteMode.None;

//});

IdentityModelEventSource.ShowPII = true;

builder.Services.AddAuthentication(options =>
{

    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
})
   .AddCookie()

    .AddOpenIdConnect("Auth0", options =>
    {


        options.NonceCookie.SameSite = SameSiteMode.Unspecified;
        options.CorrelationCookie.SameSite = SameSiteMode.Unspecified;

        options.Authority = $"{builder.Configuration["Okta:OktaDomain"]}";
        options.SaveTokens = true;
        // Configure the Auth0 Client ID and Client Secret
        options.ClientId = builder.Configuration["Okta:ClientId"];
        options.ClientSecret = builder.Configuration["Okta:ClientSecret"];
        options.AuthenticationMethod = OpenIdConnectRedirectBehavior.RedirectGet;

        options.Scope.Clear();
        options.Scope.Add("openid");
        options.Scope.Add("profile");
        options.Scope.Add("offline_access");
        options.Scope.Add("tckn");
        options.Scope.Add("phone");
        options.RequireHttpsMetadata = false;
        options.GetClaimsFromUserInfoEndpoint = true;
        // Use the authorization code flow.
        options.ResponseType = OpenIdConnectResponseType.Code;
       
        options.CallbackPath = new PathString("/authorization-code/callback");
        options.SignedOutCallbackPath = new PathString("/authorization-code/signout/callback");
        // Configure the Claims Issuer to be Auth0
        options.ClaimsIssuer = "Auth0";

        options.Events = new OpenIdConnectEvents

        {

            OnRedirectToIdentityProvider = context =>
            {
                var builder = new UriBuilder(context.ProtocolMessage.RedirectUri);
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    builder.Scheme = "https";
                    builder.Port = -1;
                }

                context.ProtocolMessage.RedirectUri = builder.ToString();
                return Task.FromResult(0);
            },
            OnTokenValidated = context =>
            {

                try
                {
                    if (context is not null && context.Principal is not null && context.Principal.Identity is not null)
                    {
                        var identity = (ClaimsIdentity)context.Principal.Identity;
                        List<Claim> addToken = new();
                        if (context?.TokenEndpointResponse is not null && context?.TokenEndpointResponse?.AccessToken is not null)
                        {
                            addToken.Add(new Claim("access_token", context?.TokenEndpointResponse?.AccessToken));
                        }
                        if (context?.TokenEndpointResponse is not null && context?.TokenEndpointResponse?.RefreshToken is not null)
                        {
                            addToken.Add(new Claim("refresh_token", context?.TokenEndpointResponse?.RefreshToken));
                        }

                        if (addToken.Count > 0)
                        {
                            identity.AddClaims(addToken);
                        }
                                    // so that we don't issue a session cookie but one with a fixed expiration
                                    context.Properties.IsPersistent = true;
                        context.Properties.AllowRefresh = true;

                                    // align expiration of the cookie with expiration of the

                                    var accessToken = new JwtSecurityToken(context.TokenEndpointResponse.AccessToken);
                       
                    }
                    else
                    {
                                    //hk todo 
                                    //redirect
                                }
                }
                catch
                {

                }

                return Task.CompletedTask;
            },
            OnTicketReceived = context =>
            {
                            // If your authentication logic is based on users then add your logic here
                            return Task.CompletedTask;
            },

            //HK save for later
            OnSignedOutCallbackRedirect = context =>
            {
                context.Response.Redirect("~/");
                context.HandleResponse();

                return Task.CompletedTask;
            },
            OnUserInformationReceived = context =>
            {
                            //IHttpContextAccessor httpContextAccessor;
                            //   RegisterUser(context);

                            return Task.CompletedTask;
            },
        };
    });
//);
builder.Services.Configure<OktaSettings>(builder.Configuration.GetSection("Okta"));

builder.Services.AddSingleton<HttpClient>();
builder.Services.AddHttpClient();
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddRefitClient<IMessagingGatewayService>()
               .ConfigureHttpClient(c => c.BaseAddress = new Uri(builder.Configuration["Api:MessagingGateway"]));
builder.Services.AddScoped<DialogService>();
builder.Services.AddScoped<AdministrationService>();

builder.Services.AddScoped<ITokenService,OktaTokenService>();
FrameworkDependencyHelper.Instance.LoadServiceCollection(builder.Services);
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
app.MapControllers();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();