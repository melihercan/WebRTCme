using System;
using System.Collections.Generic;
using System.Text;
using WebRTCme.Middleware;
using Xamarin.Forms;

namespace WebRTCme.DemoApp.Xamarin.Services
{
    public class RunOnUiThreadService : IRunOnUiThreadService
    {
        public void Invoke(Action action)
        {
            Device.BeginInvokeOnMainThread(action);
        }
    }
}
