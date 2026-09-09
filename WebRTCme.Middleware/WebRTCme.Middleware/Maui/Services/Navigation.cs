namespace WebRTCme.Middleware.Maui.Services
{
    public class Navigation : INavigation
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
