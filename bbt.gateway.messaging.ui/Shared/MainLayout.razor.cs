using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace bbt.gateway.messaging.ui.Shared
{
    public partial class MainLayout
    {
        [CascadingParameter]
        protected Task<AuthenticationState> AuthenticationState { get; set; }
        protected override async Task OnParametersSetAsync()
        {
            var user = (await AuthenticationState).User;
           

        }
    }
}
