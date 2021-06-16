using System;
using System.Collections.Generic;
using System.Text;
using WebRTCme.Middleware;
using Xamarin.Forms;

namespace WebRTCme.Middleware.Xamarin.Services
{
    public class RunOnUiThread : IRunOnUiThread
    {
        public void Invoke(Action action)
        {
            Device.BeginInvokeOnMainThread(action);
        }
    }
}
