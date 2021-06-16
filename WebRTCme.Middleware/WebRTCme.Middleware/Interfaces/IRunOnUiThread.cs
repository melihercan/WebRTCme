using System;
using System.Collections.Generic;
using System.Text;

namespace WebRTCme.Middleware
{
    public interface IRunOnUiThread
    {
        void Invoke(Action action);
    }
}
