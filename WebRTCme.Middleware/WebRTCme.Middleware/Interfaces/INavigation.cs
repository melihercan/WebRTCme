using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace WebRTCme.Middleware
{
    public interface INavigation
    {
        Task NavigateToPageAsync(string prefix, string uri);

        Task NavigateToPageAsync(string prefix, string url, string queryKey, string queryValue);
    }
}
