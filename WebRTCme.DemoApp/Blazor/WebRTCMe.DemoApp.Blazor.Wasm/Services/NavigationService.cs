using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebRTCme.Middleware;

namespace WebRTCme.DemoApp.Blazor.Wasm.Services
{
    public class NavigationService : INavigationService
    {
        private readonly NavigationManager _navigationManager;

        public NavigationService(NavigationManager navigationManager)
        {
            _navigationManager = navigationManager;
        }

        public Task NavigateToPageAsync(string url)
        {
            _navigationManager.NavigateTo($"/{url}");
            return Task.CompletedTask;
        }

        public Task NavigateToPageAsync(string url, string queryKey, string queryValue)
        {
            _navigationManager.NavigateTo($"/{url}/{queryValue}");
            return Task.CompletedTask;
        }
    }

}
