using Auth0.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.JSInterop;
using Okta.AspNetCore;
using Microsoft.Extensions.Configuration;
using bbt.gateway.messaging.ui.Base;

namespace bbt.gateway.messaging.ui.Pages.Authorize
{
    public class LoginModel : PageModel
    {
        [Inject]
        bbt.gateway.messaging.ui.Data.HttpContextAccessor contextAccessor { get; set; }

        public async  Task OnGet(string redirectUri)
        {
           IConfiguration config= FrameworkDependencyHelper.Instance.Get<IConfiguration>();
          string path=  config.GetValue<string>("Base:path")+"/callback";
            //string redirect ="http://"+ HttpContext.Request.Host.Value+"/callback";
            //await JS.InvokeAsync<string>("console.log", redirect);
            var authenticationProperties = new LoginAuthenticationPropertiesBuilder()
                .WithRedirectUri(path)
                .Build();
           await HttpContext.ChallengeAsync(Auth0Constants.AuthenticationScheme, authenticationProperties);
        }
        
    }
}
