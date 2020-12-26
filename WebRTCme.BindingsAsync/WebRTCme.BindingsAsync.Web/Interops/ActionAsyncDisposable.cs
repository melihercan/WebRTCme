using System;
using System.Threading.Tasks;

namespace WebRtcBindingsWeb.Interops
{
    internal class ActionAsyncDisposable : IAsyncDisposable
    {
        private readonly Func<ValueTask> _funcOnDispose;

        public ActionAsyncDisposable(Func<ValueTask> funcOnDispose)
        {
            _funcOnDispose = funcOnDispose;
        }

        public async ValueTask DisposeAsync()
        {
            await _funcOnDispose().ConfigureAwait(false);
        }
    }
}