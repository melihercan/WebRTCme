using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace WebRTCme.Middleware
{
    public interface INavigationService
    {
        Task NavigateToPageAsync(string url);

        Task NavigateToPageAsync(string url, string queryKey, string queryValue);
    }
}
