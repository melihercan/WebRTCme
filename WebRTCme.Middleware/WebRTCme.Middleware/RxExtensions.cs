using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Text;
using System.Threading.Tasks;

namespace WebRTCme.Middleware
{
    public static class RxExtensions
    {
        public static IDisposable SubscribeAsync<T>(this IObservable<T> source, Func<T, Task> onNext, 
            Action<Exception> onError, Action onCompleted)
        {
            return source
                .Select(e => Observable.Defer(() => onNext(e).ToObservable()))
                .Concat()
                .Subscribe(
                e => { }, // empty
                onError,
                onCompleted);
        }
    }
}
