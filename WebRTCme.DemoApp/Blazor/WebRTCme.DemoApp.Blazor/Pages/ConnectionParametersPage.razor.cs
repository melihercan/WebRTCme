using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using WebRTCme.Middleware;

namespace WebRTCme.DemoApp.Blazor.Pages
{
    public partial class ConnectionParametersPage
    {
        [Inject]
        ConnectionParametersViewModel ConnectionParametersViewModel {get;set;}

        protected override async Task OnInitializedAsync()
        {
            await ConnectionParametersViewModel.OnPageAppearingAsync(ReRender);            
            await base.OnInitializedAsync();
        }

        private void ReRender()
        {
            StateHasChanged();
        }
    }
}
