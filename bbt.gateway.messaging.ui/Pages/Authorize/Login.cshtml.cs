using Auth0.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Okta.AspNetCore;
namespace bbt.gateway.messaging.ui.Pages.Authorize
{
    public class LoginModel : PageModel
    {
        [Inject]
        bbt.gateway.messaging.ui.Data.HttpContextAccessor contextAccessor { get; set; }
        public async  Task OnGet(string redirectUri)
        {

            string redirect ="http://"+ HttpContext.Request.Host.Value+"/callback";

            var authenticationProperties = new LoginAuthenticationPropertiesBuilder()
                .WithRedirectUri(redirect)
                .Build();
            //var test = Challenge(OktaDefaults.MvcAuthenticationScheme);
            //ChallengeResult challengeResult = Challenge(OktaDefaults.MvcAuthenticationScheme);
            //return Task.FromResult(challengeResult);
           await HttpContext.ChallengeAsync(Auth0Constants.AuthenticationScheme, authenticationProperties);
            //HttpContext.ChallengeAsync(Auth0Constants.AuthenticationScheme, authenticationProperties);
        }
    }
}
