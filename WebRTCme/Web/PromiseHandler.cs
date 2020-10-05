//
// PromiseHandler.cs - The promise handler class to be passed to JavaScript.  The
// methods of this class are invoked by the Blazor JavaScript interop to pass information
// back to the C# application.
//
// Todd Littlejohn, 21 July 2019

using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json;

namespace WebRtc.Web
{
    public class PromiseHandler
    {
        /// <summary>
        /// The TaskCompletionSource object.  This object can be awaited within the C# application
        /// to asynchronously process JavaScript promise results.
        /// </summary>
        public TaskCompletionSource</*string*/object> tcs { get; set;}

        /// <summary>
        /// Set the TaskCompletionSource's result value.  The supplied result is the 
        /// result of a successfully resolved JavaScript promise.  Note this method is 
        /// invokable from Javascript.
        /// </summary>
        /// <param name="json">The JSON result from JavaScript</param>
        [JSInvokable]
        public void SetResult(string json)
        {
            // Deserialize the JSON result to a string.  Once JSInterop supports generic
            // classes this class and associated methods can be made generic.  Then objects
            // of this class can simply be instantiated with the desired result type.
            var result = JsonSerializer.Deserialize<string>(json);

            // Set the results in the TaskCompletionSource
            tcs.SetResult(result);
        }

        /// <summary>
        /// Set/raise a TaskCompletionSource excpetion with the error (i.e., message) returned 
        /// from JavaScript.  The supplied error is the result of a rejected JavaScript promise.
        /// Note this method is invokable from JavaScript.
        /// </summary>
        /// <param name="error">The error message return from JavaScript</param>
        [JSInvokable]
        public void SetError(string error)
        {
            // Create the exception from the returned error string
            tcs.SetException(new Exception(error));
        }
    }
}
