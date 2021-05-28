using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebRTCme.Middleware;
using Blazored.Modal.Services;
using WebRTCme.DemoApp.Blazor.Components;

namespace WebRTCme.DemoApp.Blazor.Pages
{
    public partial class InitializingPage
    {
        [Inject]
        InitializingViewModel InitializingViewModel { get; set; }

        // Navigation failes on server side.
        //protected override async Task OnInitializedAsync()
        //{
        //    await base.OnInitializedAsync();
        //    await InitializingViewModel.OnPageAppearingAsync();
        //}

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await base.OnAfterRenderAsync(firstRender);
            if (firstRender)
                await InitializingViewModel.OnPageAppearingAsync();

        }
    }
}
