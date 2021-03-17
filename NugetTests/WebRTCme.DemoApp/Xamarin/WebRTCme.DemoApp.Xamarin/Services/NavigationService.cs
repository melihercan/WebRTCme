using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WebRTCme.Middleware;
using Xamarin.Forms;

namespace WebRTCme.DemoApp.Xamarin.Services
{
    public class NavigationService : INavigationService
    {
        public async Task NavigateToPageAsync(string prefix, string uri)
        {
            await Shell.Current.GoToAsync($"{prefix}{uri}");
        }

        public async Task NavigateToPageAsync(string prefix, string uri, string queryKey, string queryValue)
        {
            await Shell.Current.GoToAsync($"{prefix}{uri}?{queryKey}={queryValue}");
        }
    }

}
