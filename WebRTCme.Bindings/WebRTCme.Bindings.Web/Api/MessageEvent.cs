using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WebRtcBindingsWeb.Interops;
using WebRTCme;

namespace WebRtcBindingsWeb.Api
{
    internal class MessageEvent : ApiBase, IMessageEvent
    {
        public static IMessageEvent Create(IJSRuntime jsRuntime, JsObjectRef jsObjectRefRtcStatsReport) => 
            new MessageEvent(jsRuntime, jsObjectRefRtcStatsReport);

        private MessageEvent(IJSRuntime jsRuntime, JsObjectRef jsObjectRef) : base(jsRuntime, jsObjectRef) { }

        public object Data => throw new NotImplementedException();

        public string Origin => throw new NotImplementedException();

        public string LastEventId => throw new NotImplementedException();

        public object Source => throw new NotImplementedException();

        public object[] Ports => throw new NotImplementedException();



    }
}
