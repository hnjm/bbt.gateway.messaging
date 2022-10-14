using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using System.Globalization;


namespace bbt.gateway.messaging.ui.Shared
{
   
    public partial class MainLayout
    {
        [CascadingParameter]
        protected Task<AuthenticationState> AuthenticationState { get; set; }
        [Inject]
        public bbt.gateway.messaging.ui.Data.HttpContextAccessor httpContext { get; set; }
        [Inject]
        public NavigationManager navigationManager { get; set; }
        protected override async Task OnParametersSetAsync()
        {
            var user = (await AuthenticationState).User;
            string accessToken = await httpContext.Context.GetTokenAsync("access_token");
            string idToken = await httpContext.Context.GetTokenAsync("id_token");
            if (!string.IsNullOrEmpty(accessToken))
            {
                DateTime accessTokenExpiresAt = DateTime.Parse(
   await httpContext.Context.GetTokenAsync("expires_at"),
   CultureInfo.InvariantCulture,
   DateTimeStyles.RoundtripKind);
                string accessTokenExpiresAta =
         await httpContext.Context.GetTokenAsync("tckn");
                
            }
         

        }
        protected override void OnInitialized()
        {
            base.OnInitialized();
            httpContext.Context.Features.Get<HttpContext>();
        }
        public void LoginSite()
        {
            navigationManager.NavigateTo($"login?redirectUri=/", forceLoad: true);
        }

    }
}
