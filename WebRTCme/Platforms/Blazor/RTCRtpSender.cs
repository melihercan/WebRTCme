using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WebRTCme.Bindings.Blazor.Interops;
using WebRTCme.Bindings.Blazor.Extensions;
using WebRTCme;
using System.Linq;
using WebRTCme.Platforms.Blazor.Custom;

namespace WebRTCme.Blazor
{
    internal class RTCRtpSender : NativeBase, IRTCRtpSender
    {
        public RTCRtpSender(IJSRuntime jsRuntime, JsObjectRef jsObjectRef) : base(jsRuntime, jsObjectRef) { }

        public IRTCDTMFSender Dtmf =>
            new RTCDTMFSender(JsRuntime, JsRuntime.GetJsPropertyObjectRef(NativeObject, "dtmf"));

        public IMediaStreamTrack Track =>
            new MediaStreamTrack(JsRuntime, JsRuntime.GetJsPropertyObjectRef(NativeObject, "track"));

        public IRTCDtlsTransport Transport =>
            new RTCDtlsTransport(JsRuntime, JsRuntime.GetJsPropertyObjectRef(NativeObject, "transport"));

        /// <summary>
        /// The sender's current parameters, contents and all.
        /// </summary>
        /// <remarks>
        /// Content, not an object reference. This used to call <c>CallJsMethod</c>, which returns a
        /// reference for any object result, so every property of what came back was null - and
        /// nothing said so. Any caller reading <c>Encodings</c> got a null array: layer control
        /// threw <see cref="ArgumentNullException"/> from deep inside a LINQ call, and setting
        /// encoding parameters silently did nothing.
        /// </remarks>
        public RTCRtpSendParameters GetParameters() =>
            JsRuntime.CallJsMethodWithContent<RTCRtpSendParameters>(NativeObject, "getParameters");

        public async Task<IRTCStatsReport> GetStats() =>
            (await JsRuntime.GetJsStatsAsync(NativeObject)).ToStatsReport();

        public Task SetParameters(RTCRtpSendParameters parameters) =>
            JsRuntime.CallJsMethodVoidAsync(NativeObject, "setParameters", parameters).AsTask();

        public void SetStreams(IMediaStream[] mediaStreams) =>
            JsRuntime.CallJsMethodVoid(
                NativeObject, "setStreams", mediaStreams.Select(stream => ((MediaStream)stream).NativeObject).ToArray());

        public Task ReplaceTrack(IMediaStreamTrack newTrack = null) =>
            JsRuntime.CallJsMethodVoidAsync(NativeObject, "replaceTrack", ((MediaStreamTrack)newTrack)?.NativeObject).AsTask();

        /*static*/
        public RTCRtpCapabilities GetCapabilities(string kind) =>
            JsRuntime.CallJsMethodWithContent<RTCRtpCapabilities>("RTCRtpSender", "getCapabilities", null, kind);
    }
}
