using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebRTCme.Middleware;

namespace WebRTCme.Middleware.Blazor.Services
{
    public class RunOnUiThread : IRunOnUiThread
    {
        public void Invoke(Action action)
        {
            action();
        }
    }
}
