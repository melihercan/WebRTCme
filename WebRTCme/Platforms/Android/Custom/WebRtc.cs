using Android.Content;
using Android.Util;
using Microsoft.JSInterop;
using Org.Webrtc.Audio;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WebRTCme.Android;
using Webrtc = Org.Webrtc;
////#if! (NET6_0 || NET7_0 || NET8_0)
////using Xamarin.Essentials;
////#endif


namespace WebRTCme
{
    //// FOR TESTING, REMOVE THIS LATER
    public static class ConnFact
    {
        public static Webrtc.PeerConnectionFactory PeerConnectionFactory { get; set; }
    }

    internal class WebRtc : IWebRtc
    {
        public static Webrtc.PeerConnectionFactory NativePeerConnectionFactory { get; private set; }
        public static Webrtc.IEglBase NativeEglBase { get; private set; }

        private static int _id = 1000;
        public static int Id => Interlocked.Increment(ref _id);

        public static IWebRtc Create() => new WebRtc();

        private WebRtc()
        {
            //var context = Platform.CurrentActivity.ApplicationContext;
            var context = Platform.AppContext;

            var options = Webrtc.PeerConnectionFactory.InitializationOptions
                    .InvokeBuilder(context)
                    //.SetFieldTrials("")
                    //.SetEnableInternalTracer(true)
                    .CreateInitializationOptions();
            Webrtc.PeerConnectionFactory.Initialize(options);
     Webrtc.Logging.EnableLogToDebugOutput(Webrtc.Logging.Severity./*LsInfo*/LsError/*LsWarning*//*LsVerbose*/);

            ///// TODO: INVESTIGATE WHY Webrtc.EglBase.Create() FAILS
            NativeEglBase = Webrtc.EglBase.Create();// EglBaseHelper.Create();

            var eglBaseContext = NativeEglBase.EglBaseContext;
            //var adm = CreateJavaAudioDevice(context);
            var encoderFactory = new Webrtc.DefaultVideoEncoderFactory(eglBaseContext, true, true);
            //var encoderFactory = new Webrtc.SoftwareVideoEncoderFactory();
            var decoderFactory = new Webrtc.DefaultVideoDecoderFactory(eglBaseContext);
            //var decoderFactory = new Webrtc.SoftwareVideoDecoderFactory();
            NativePeerConnectionFactory = Webrtc.PeerConnectionFactory.InvokeBuilder()
                //.SetAudioDeviceModule(adm)
                .SetVideoEncoderFactory(encoderFactory)
                .SetVideoDecoderFactory(decoderFactory)
                .SetOptions(new Webrtc.PeerConnectionFactory.Options())
                .CreatePeerConnectionFactory();
 ConnFact.PeerConnectionFactory = NativePeerConnectionFactory;
            //adm.Release();
        }

        public IWindow Window(IJSRuntime jsRuntime) => new global::WebRTCme.Android.Window();

        private static IAudioDeviceModule CreateJavaAudioDevice(Context context)
        {
            var audioErrorCallbacks = new AudioErrorCallbacks();
            return JavaAudioDeviceModule.InvokeBuilder(context)
                .SetAudioRecordErrorCallback(audioErrorCallbacks)
                .SetAudioRecordStateCallback(audioErrorCallbacks)
                .SetAudioTrackErrorCallback(audioErrorCallbacks)
                .SetAudioTrackStateCallback(audioErrorCallbacks)
                .CreateAudioDeviceModule();
        }

        public void Dispose()
        {
        }

        private class AudioErrorCallbacks : Java.Lang.Object,
            JavaAudioDeviceModule.IAudioRecordErrorCallback,
            JavaAudioDeviceModule.IAudioRecordStateCallback,
            JavaAudioDeviceModule.IAudioTrackErrorCallback,
            JavaAudioDeviceModule.IAudioTrackStateCallback

        {
            private const string TAG = nameof(NativePeerConnectionFactory);

            public void OnWebRtcAudioRecordError(string p0)
            {
                Log.Error(TAG, $"OnWebRtcAudioRecordError: {p0}");
            }

            public void OnWebRtcAudioRecordInitError(string p0)
            {
                Log.Error(TAG, $"OnWebRtcAudioRecordInitError: {p0}");
            }

            public void OnWebRtcAudioRecordStartError(JavaAudioDeviceModule.AudioRecordStartErrorCode p0, string p1)
            {
                Log.Error(TAG, $"OnWebRtcAudioRecordStartError: errorCode {p0} {p1}");
            }

            public void OnWebRtcAudioTrackError(string p0)
            {
                Log.Error(TAG, $"OnWebRtcAudioTrackError: errorCode {p0}");
            }

            public void OnWebRtcAudioTrackInitError(string p0)
            {
                Log.Error(TAG, $"OnWebRtcAudioTrackInitError: errorCode {p0}");
            }

            public void OnWebRtcAudioTrackStartError(JavaAudioDeviceModule.AudioTrackStartErrorCode p0, string p1)
            {
                Log.Error(TAG, $"OnWebRtcAudioTrackStartError: errorCode {p0} {p1}");
            }

            public void OnWebRtcAudioRecordStart()
            {
                Log.Info(TAG, "Audio recording starts");
            }

            public void OnWebRtcAudioRecordStop()
            {
                Log.Info(TAG, "Audio recording stops");
            }

            public void OnWebRtcAudioTrackStart()
            {
                Log.Info(TAG, "Audio playout starts");
            }

            public void OnWebRtcAudioTrackStop()
            {
                Log.Info(TAG, "Audio playout stops");
            }
        }



    }
}
