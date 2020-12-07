using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WebRtcBindingsWeb.Interops;
using WebRTCme;

namespace WebRtcBindingsWeb.Api
{
    internal class ErrorEvent : ApiBase, IErrorEvent
    {
        public static IErrorEvent Create(IJSRuntime jsRuntime, JsObjectRef jsObjectRefRtcStatsReport) => 
            new ErrorEvent(jsRuntime, jsObjectRefRtcStatsReport);

        private ErrorEvent(IJSRuntime jsRuntime, JsObjectRef jsObjectRef) : base(jsRuntime, jsObjectRef) { }

        public string Message => throw new NotImplementedException();

        public string FileName => throw new NotImplementedException();

        public int LineNo => throw new NotImplementedException();

        public int ColNo => throw new NotImplementedException();

        public object Error => throw new NotImplementedException();



    }
}
