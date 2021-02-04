// onotseike@hotmail.comPaula Aliu
using System.Collections.Generic;
using Android.Graphics;
using Android.Opengl;
using Android.OS;
using Android.Views;
using Java.Lang;
using Webrtc = Org.Webrtc;

namespace WebRtcMiddlewareXamarin
{
    internal static class EglBaseHelper
    {
        private static int _openGlesVersion = 2;
        private static bool _hasAlphaChannel;
        private static bool _supportsPixelBuffer;
        private static bool _isRecordable;

        private static int[] CreateConfigAttributes()
        {
            var list = new List<int>();
            list.Add(EGL14.EglRedSize);
            list.Add(8);
            list.Add(EGL14.EglGreenSize);
            list.Add(8);
            list.Add(EGL14.EglBlueSize);
            list.Add(8);
            if (_hasAlphaChannel)
            {
                list.Add(EGL14.EglAlphaSize);
                list.Add(8);
            }

            if (_openGlesVersion == 2 || _openGlesVersion == 3)
            {
                list.Add(EGL14.EglRenderableType);
                list.Add(_openGlesVersion == 3 ? 64 : 4);
                //list.Add(4);
            }

            if (_supportsPixelBuffer)
            {
                list.Add(EGL14.EglSurfaceType);
                list.Add(EGL14.EglPbufferBit);
            }

            if (_isRecordable)
            {
                list.Add(12610);
                list.Add(1);
            }
            list.Add(EGL14.EglNone);

            return list.ToArray();
        }

        public static Webrtc.IEglBase Create()
        {
            return new EglBase14Impl(null, CreateConfigAttributes());
        }

        private class EglBase14Impl : Object, Webrtc.IEglBase
        {
            private EGLContext _eglContext;

            private EGLConfig _eglConfig;
            private EGLDisplay _eglDisplay;
            private EGLSurface _eglSurface;

            public EglBase14Impl(EGLContext sharedContext, int[] configAttributes)
            {
                _eglSurface = EGL14.EglNoSurface;
                _eglDisplay = GetEglDisplay();
                _eglConfig = GetEglConfig(_eglDisplay, configAttributes);

                int openGlesVersion = GetOpenGlesVersionFromConfig(configAttributes);
                Webrtc.Logging.D("EglBase14Impl", "Using OpenGL ES version " + openGlesVersion);
                _eglContext = CreateEglContext(sharedContext, _eglDisplay, _eglConfig, openGlesVersion);
            }


            public Webrtc.IEglBaseContext EglBaseContext => new ContextInternal(_eglContext);
            public bool HasSurface => _eglSurface != EGL14.EglNoSurface;

            public void CreateDummyPbufferSurface()
            {
                CreatePbufferSurface(1, 1);
            }

            public void CreatePbufferSurface(int width, int height)
            {
                CheckIsNotReleased();
                if (_eglSurface != EGL14.EglNoSurface)
                {
                    throw new RuntimeException("Already has an EGLSurface");
                }

                var surfaceAttribs = new[] { 12375, width, 12374, height, 12344 };
                _eglSurface = EGL14.EglCreatePbufferSurface(_eglDisplay, _eglConfig, surfaceAttribs, 0);
                if (_eglSurface == EGL14.EglNoSurface)
                {
                    throw new RuntimeException("Failed to create pixel buffer surface with size " + width + "x" +
                                               height + ": 0x" + Integer.ToHexString(EGL14.EglGetError()));
                }
            }

            public void CreateSurface(SurfaceTexture p0)
            {
                CreateSurfaceInternal(p0);
            }

            public void CreateSurface(Surface p0)
            {
                CreateSurfaceInternal(p0);
            }

            public void DetachCurrent()
            {
                lock (Webrtc.EglBase.Lock)
                {
                    if (!EGL14.EglMakeCurrent(_eglDisplay, EGL14.EglNoSurface, EGL14.EglNoSurface, EGL14.EglNoContext))
                    {
                        throw new RuntimeException("eglDetachCurrent failed: 0x" +
                                                   Integer.ToHexString(EGL14.EglGetError()));
                    }
                }
            }

            public void MakeCurrent()
            {
                CheckIsNotReleased();
                if (_eglSurface == EGL14.EglNoSurface)
                {
                    throw new RuntimeException("No EGLSurface - can't make current");
                }

                lock (Webrtc.EglBase.Lock)
                {
                    if (!EGL14.EglMakeCurrent(_eglDisplay, _eglSurface, _eglSurface, _eglContext))
                    {
                        throw new RuntimeException("eglMakeCurrent failed: 0x" +
                                                   Integer.ToHexString(EGL14.EglGetError()));
                    }
                }
            }

            public void Release()
            {
                CheckIsNotReleased();
                ReleaseSurface();
                DetachCurrent();
                EGL14.EglDestroyContext(_eglDisplay, _eglContext);
                EGL14.EglReleaseThread();
                EGL14.EglTerminate(_eglDisplay);
                _eglContext = EGL14.EglNoContext;
                _eglDisplay = EGL14.EglNoDisplay;
                _eglConfig = null;
            }

            public void ReleaseSurface()
            {
                if (_eglSurface != EGL14.EglNoSurface)
                {
                    EGL14.EglDestroySurface(_eglDisplay, _eglSurface);
                    _eglSurface = EGL14.EglNoSurface;
                }
            }

            public int SurfaceHeight()
            {
                var heightArray = new int[1];
                EGL14.EglQuerySurface(_eglDisplay, _eglSurface, 12374, heightArray, 0);
                return heightArray[0];
            }

            public int SurfaceWidth()
            {
                var widthArray = new int[1];
                EGL14.EglQuerySurface(_eglDisplay, _eglSurface, 12375, widthArray, 0);
                return widthArray[0];
            }

            public void SwapBuffers()
            {
                CheckIsNotReleased();
                if (_eglSurface == EGL14.EglNoSurface)
                {
                    throw new RuntimeException("No EGLSurface - can't swap buffers");
                }

                lock (Webrtc.EglBase.Lock)
                {
                    EGL14.EglSwapBuffers(_eglDisplay, _eglSurface);
                }
            }

            public void SwapBuffers(long timeStampNs)
            {
                CheckIsNotReleased();
                if (_eglSurface == EGL14.EglNoSurface)
                {
                    throw new RuntimeException("No EGLSurface - can't swap buffers");
                }

                lock (Webrtc.EglBase.Lock)
                {
                    EGLExt.EglPresentationTimeANDROID(_eglDisplay, _eglSurface, timeStampNs);
                    EGL14.EglSwapBuffers(_eglDisplay, _eglSurface);
                }
            }

            private void CreateSurfaceInternal(Java.Lang.Object surface)
            {
                CheckIsNotReleased();
                if (_eglSurface != EGL14.EglNoSurface)
                {
                    throw new RuntimeException("Already has an EGLSurface");
                }
                else
                {
                    int[] surfaceAttribs = new int[] { 12344 };
                    _eglSurface = EGL14.EglCreateWindowSurface(_eglDisplay, _eglConfig, surface, surfaceAttribs, 0);
                    if (_eglSurface == EGL14.EglNoSurface)
                    {
                        throw new RuntimeException("Failed to create window surface: 0x" +
                                                   Integer.ToHexString(EGL14.EglGetError()));
                    }
                }
            }

            private void CheckIsNotReleased()
            {
                if (_eglDisplay == EGL14.EglNoDisplay || _eglContext == EGL14.EglNoContext || _eglConfig == null)
                {
                    throw new RuntimeException("This object has been released");
                }
            }

            private static EGLDisplay GetEglDisplay()
            {
                var eglDisplay = EGL14.EglGetDisplay(0);
                if (eglDisplay == EGL14.EglNoDisplay)
                {
                    throw new RuntimeException("Unable to get EGL14 display: 0x" +
                                               Integer.ToHexString(EGL14.EglGetError()));
                }

                var version = new int[2];
                if (!EGL14.EglInitialize(eglDisplay, version, 0, version, 1))
                {
                    throw new RuntimeException("Unable to initialize EGL14: 0x" +
                                               Integer.ToHexString(EGL14.EglGetError()));
                }

                return eglDisplay;
            }

            private static EGLConfig GetEglConfig(EGLDisplay eglDisplay, int[] configAttributes)
            {
                var configs = new EGLConfig[1];
                var numConfigs = new int[1];
                if (!EGL14.EglChooseConfig(eglDisplay, configAttributes, 0, configs, 0, configs.Length, numConfigs, 0))

#if false
                    int[] _attribs_config = new int[]{
                        EGL14.EglRenderableType, EGL14.EglOpenglEs2Bit,
                        EGL14.EglRedSize, 8,
                        EGL14.EglGreenSize, 8,
                        EGL14.EglBlueSize, 8,
                        EGL14.EglAlphaSize, 8,
                        EGL14.EglDepthSize, 0,
                        EGL14.EglStencilSize, 0,
                        EGL14.EglNone
                    };
                if (!EGL14.EglChooseConfig(eglDisplay, _attribs_config, 0, configs, 0, configs.Length, numConfigs, 0))
#endif
                {
                    throw new RuntimeException("eglChooseConfig failed: 0x" + Integer.ToHexString(EGL14.EglGetError()));
                }

                if (numConfigs[0] <= 0)
                {
                    throw new RuntimeException("Unable to find any matching EGL config");
                }

                EGLConfig eglConfig = configs[0];
                if (eglConfig == null)
                {
                    throw new RuntimeException("eglChooseConfig returned null");
                }

                return eglConfig;
            }

            private static EGLContext CreateEglContext(EGLContext sharedContext, EGLDisplay eglDisplay,
                EGLConfig eglConfig, in int openGlesVersion)
            {
                if (sharedContext != null && sharedContext == EGL14.EglNoContext)
                {
                    throw new RuntimeException("Invalid sharedContext");
                }

                int[] contextAttributes = new int[] { 12440, openGlesVersion, 12344 };
                var rootContext = sharedContext ?? EGL14.EglNoContext;
                EGLContext eglContext;

                lock (Webrtc.EglBase.Lock)
                {
                    eglContext = EGL14.EglCreateContext(eglDisplay, eglConfig, rootContext, contextAttributes, 0);
                }

                if (eglContext == EGL14.EglNoContext)
                {
                    throw new RuntimeException("Failed to create EGL context: 0x" +
                                               Integer.ToHexString(EGL14.EglGetError()));
                }

                return eglContext;
            }

            private static int GetOpenGlesVersionFromConfig(int[] configAttributes)
            {
                for (int i = 0; i < configAttributes.Length - 1; ++i)
                {
                    if (configAttributes[i] == 12352)
                    {
                        switch (configAttributes[i + 1])
                        {
                            case 4:
                                return 2;
                            case 64:
                                return 3;
                            default:
                                return 1;
                        }
                    }
                }

                return 1;
            }

            private class ContextInternal : Java.Lang.Object, Webrtc.IEglBase14Context
            {
                public ContextInternal(EGLContext eglContext)
                {
                    RawContext = eglContext;
                }

                public long NativeEglContext
                {
                    get
                    {
                        return Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop
                            ? RawContext.NativeHandle
#pragma warning disable 618
                            : RawContext.GetHandle();
#pragma warning restore 618
                    }
                }

                public EGLContext RawContext { get; }
            }
        }
    }
}
