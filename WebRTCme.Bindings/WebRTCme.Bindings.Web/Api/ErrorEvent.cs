using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WebRtcBindingsWeb.Interops;
using WebRtcBindingsWeb.Extensions;
using WebRTCme;

namespace WebRtcBindingsWeb.Api
{
    internal class ErrorEvent : ApiBase, IErrorEvent
    {
        public static IErrorEvent Create(IJSRuntime jsRuntime, JsObjectRef jsObjectRefRtcStatsReport) => 
            new ErrorEvent(jsRuntime, jsObjectRefRtcStatsReport);

        private ErrorEvent(IJSRuntime jsRuntime, JsObjectRef jsObjectRef) : base(jsRuntime, jsObjectRef) { }

        public string Message => GetNativeProperty<string>("message");

        public string FileName => GetNativeProperty<string>("fileName");

        public int LineNo => GetNativeProperty<int>("lineNo");

        public int ColNo => GetNativeProperty<int>("colNo");

        ////public object Error => throw new NotImplementedException();
    }
}
