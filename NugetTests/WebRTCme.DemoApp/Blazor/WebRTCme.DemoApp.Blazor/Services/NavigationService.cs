using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebRTCme.Middleware;

namespace WebRTCme.DemoApp.Blazor.Services
{
    public class NavigationService : INavigationService
    {
        private readonly NavigationManager _navigationManager;

        public NavigationService(NavigationManager navigationManager)
        {
            _navigationManager = navigationManager;
        }

        public Task NavigateToPageAsync(string prefix, string uri)
        {
            _navigationManager.NavigateTo($"/{uri}");
            return Task.CompletedTask;
        }

        public Task NavigateToPageAsync(string prefix, string uri, string queryKey, string queryValue)
        {
            _navigationManager.NavigateTo($"/{uri}/{queryValue}");
            return Task.CompletedTask;
        }
    }

}
