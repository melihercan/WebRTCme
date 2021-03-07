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

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
            await InitializingViewModel.OnPageAppearingAsync();
        }
    }
}
