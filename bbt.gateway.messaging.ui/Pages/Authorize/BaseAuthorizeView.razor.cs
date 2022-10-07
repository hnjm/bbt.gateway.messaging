using Microsoft.AspNetCore.Components;
//using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Auth0.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Components.Authorization;
using Okta.AspNetCore;

namespace bbt.gateway.messaging.ui.Pages.Authorize
{
    public partial class BaseAuthorizeView
    {
        [Parameter]
        public RenderFragment AuthorizedControl { get; set; }
        [Parameter]
        public RenderFragment NotAuthorizedControl { get; set; }

        public RenderFragment Display { get; set; }

        public bool IsAuthorized { get; set; }=false;
        [CascadingParameter]
        Task<AuthenticationState> AuthenticationStated { get; set; }
        [Inject]
        public NavigationManager navigationManager { get; set; }
        protected override async  Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                try
                {
                  
                    //Challenge(OktaDefaults.MvcAuthenticationScheme);
                    //var authste = await AuthenticationStated;
                    //var user = authste.User;
                    //string Name = user.Identity.Name;
                    //var res = await MessagingGateway.GetUserControl(Name);
                    //if (res != null && res)
                    //{
                    //    IsAuthorized = true;
                    //}
                    //else
                    //{
                    //    IsAuthorized = false;
                    //}
                    //if (IsAuthorized)
                    //    Display = AuthorizedControl;
                    //else
                    //    Display = NotAuthorizedControl;
                    //StateHasChanged();



                }
                catch (Exception ex)
                {
                    Display = NotAuthorizedControl;
                    //StateHasChanged();
                }

            }


            await base.OnAfterRenderAsync(firstRender);
        }

    }
}
