using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebRTCme.Middleware;
using Xamarin.Forms;

namespace WebRTCme.DemoApp.Blazor.Wasm.Services
{
    public class NavigationService : INavigationService
    {
        public async Task NavigateToPageAsync(string uri)
        {
            await Shell.Current.GoToAsync($"/{uri}");
        }

        public async Task NavigateToPageAsync(string uri, string queryKey, string queryValue)
        {
            await Shell.Current.GoToAsync($"/{uri}?{queryKey}={queryValue}");
        }
    }
}
