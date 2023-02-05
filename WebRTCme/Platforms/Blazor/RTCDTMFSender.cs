using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WebRTCme.Bindings.Blazor.Interops;
using WebRTCme.Bindings.Blazor.Extensions;
using WebRTCme;
using WebRTCme.Platforms.Blazor.Custom;

namespace WebRTCme.Blazor
{
    internal class RTCDTMFSender : NativeBase, IRTCDTMFSender
    {
        public RTCDTMFSender(IJSRuntime jsRuntime, JsObjectRef jsObjectRef) : base(jsRuntime, jsObjectRef) 
        {
            AddNativeEventListener("tonechange", (s, e) => OnToneChange?.Invoke(s, e));
        }

        public string ToneBuffer => GetNativeProperty<string>("toneBuffer");

        public event EventHandler OnToneChange;

        public void InsertDTMF(string tones, ulong duration = 100, ulong interToneGap = 70) =>
            JsRuntime.CallJsMethodVoid(NativeObject, "insertDTMF", tones, duration, interToneGap);
    }
}
