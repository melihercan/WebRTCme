using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebRTCme.Middleware;

namespace WebRTCme.DemoApp.Blazor.Services
{
    public class RunOnUiThreadService : IRunOnUiThreadService
    {
        public void Invoke(Action action)
        {
            action();
        }
    }
}
