namespace WebRTCme.Middleware.Maui.Services
{
    public class RunOnUiThread : IRunOnUiThread
    {
        public void Invoke(Action action)
        {
            MainThread.BeginInvokeOnMainThread(action);
        }
    }
}
