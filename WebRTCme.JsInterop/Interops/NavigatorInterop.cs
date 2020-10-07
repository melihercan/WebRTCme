//using BrowserInterop.Extensions;
//using BrowserInterop.Geolocation;
//using BrowserInterop.Storage;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebRtcJsInterop.Extensions;

namespace WebRtcJsInterop.Interops
{
    public class NavigatorInterop : JsObjectWrapperBase
    {
        internal override void SetJsRuntime(IJSRuntime jsRuntime, JsRuntimeObjectRef navigatorRef)
        {
            base.SetJsRuntime(jsRuntime, navigatorRef);
            //Geolocation = new WindowNavigatorGeolocation(jsRuntime);
            //Storage = new WindowStorageManager(jsRuntime);
            //Connection?.SetJsRuntime(jsRuntime, navigatorRef);
        }


        /// <summary>
        /// Returns the internal "code" name of the current browser. Do not rely on this property to return the correct value.
        /// </summary>
        /// <returns></returns>
        public string AppCodeName { get; set; }

        /// <summary>
        /// Returns  the official name of the browser. Do not rely on this property to return the correct value.
        /// </summary>
        /// <returns></returns>
        public string AppName { get; set; }

        /// <summary>
        /// Returns  the official name of the browser. Do not rely on this property to return the correct value.
        /// </summary>
        /// <returns></returns>
        public string AppVersion { get; set; }


        /// <summary>
        ///  provides information about the system's battery
        /// </summary>
        /// <returns></returns>
        //public async ValueTask<WindowNavigatorBattery> GetBattery()
        //{
        //    return await JsRuntime.InvokeAsync<WindowNavigatorBattery>("webRtcInterop.navigator.getBattery").ConfigureAwait(false);
        //}


        /// <summary>
        /// Return a JS Interop wrapper for getting information about the network connection of a device.
        /// </summary>
        /// <returns></returns>
        //public WindowNavigatorConnection Connection { get; set; }

        /// <summary>
        /// Returns false if setting a cookie will be ignored and true otherwise.
        /// </summary>
        /// <returns></returns>
        public bool CookieEnabled { get; set; }

        /// <summary>
        /// Returns the number of logical processor cores available.
        /// </summary>
        /// <returns></returns>
        public int HardwareConcurrency { get; set; }

        //public WindowNavigatorGeolocation Geolocation { get; private set; }

        /// <summary>
        /// Returns false if the browser enables java
        /// </summary>
        /// <returns></returns>
        public async ValueTask<bool> JavaEnabled()
        {
            return await JsRuntime.InvokeInstanceMethod<bool>(JsObjectRef, "javaEnabled").ConfigureAwait(false);
        }

        /// <summary>
        /// The user preferred language
        /// </summary>
        /// <returns></returns>
        public string Language { get; set; }

        /// <summary>
        /// Return all the user set languages
        /// </summary>
        /// <returns></returns>
        public string[] Languages { get; set; }

        /// <summary>
        /// Returns the maximum number of touch point supported by the current device
        /// </summary>
        /// <returns></returns>
        public int MaxTouchPoints { get; set; }

        /// <summary>
        /// Returns the mime types supported by the browser
        /// </summary>
        /// <returns></returns>
        //public NavigatorMimeTypes[] MimeTypes { get; set; }

        /// <summary>
        /// Returns true if the user is online
        /// </summary>
        /// <returns></returns>
        public bool Online { get; set; }

        /// <summary>
        /// Returns a string representing the platform of the browser.
        /// </summary>
        /// <value></value>
        public string Platform { get; set; }


        /// <summary>
        /// Returns the plugins installed in this browser
        /// </summary>
        /// <returns></returns>

        //public NavigatorPlugin[] Plugins { get; set; }

        /// <summary>
        /// Provides an interface for managing persistence permissions and estimating available storage
        /// </summary>
        /// <value></value>
        //public WindowStorageManager Storage { get; private set; }

        /// <summary>
        /// Return the user agent string for the browser
        /// </summary>
        /// <value></value>
        public string UserAgent { get; set; }

        /// <summary>
        /// Returns true if a call to Share() would succeed.
        /// Returns false if it would fail or sharing is not supported
        /// </summary>
        /// <returns></returns>
        //public async ValueTask<bool> CanShare(ShareData shareData)
        //{
        //    return await JsRuntime.HasProperty(JsObjectRef, "canShare").ConfigureAwait(false) &&
        //           await JsRuntime.InvokeInstanceMethod<bool>(JsObjectRef, "canShare", shareData).ConfigureAwait(false);
        //}

        /// <summary>
        /// Lets web sites register their ability to open or handle particular URL schemes (aka protocols).
        /// 
        /// For example, this API lets webmail sites open mailto: URLs, or VoIP sites open tel: URLs.
        /// </summary>
        /// <param name="protocol">A string containing the protocol the site wishes to handle. For example, you can register to handle SMS text message links by passing the "sms" scheme.</param>
        /// <param name="urlPattern">A string containing the URL of the handler. This URL must include %s, as a placeholder that will be replaced with the escaped URL to be handled.</param>
        /// <param name="title">A human-readable title string for the handler. This will be displayed to the user, such as prompting “Allow this site to handle [scheme] links?” or listing registered handlers in the browser’s settings.</param>
        /// <returns></returns>
#pragma warning disable CA1054 
        public async ValueTask RegisterProtocolHandler(string protocol, string urlPattern, string title)
#pragma warning restore CA1054 
        {
            if (string.IsNullOrEmpty(protocol)) throw new ArgumentNullException(nameof(protocol));

            if (string.IsNullOrEmpty(urlPattern)) throw new ArgumentNullException(nameof(urlPattern));

            if (string.IsNullOrEmpty(title)) throw new ArgumentNullException(nameof(title));

            if (await JsRuntime.HasProperty(JsObjectRef, "registerProtocolHandler").ConfigureAwait(false))
                await JsRuntime.InvokeInstanceMethod(JsObjectRef, "registerProtocolHandler", protocol, urlPattern,
                    title).ConfigureAwait(false);
        }

        /// <summary>
        /// Sends a small amount of data over HTTP to a web server. Returns true if the method is supported and succeeds
        /// 
        /// This method is for analytics and diagnostics that send data to a server before the document is unloaded, where sending the data any sooner may miss some possible data collection. For example, which link the user clicked before navigating away and unloading the page.
        /// Ensuring that data has been sent during the unloading of a document has traditionally been difficult, because user agents typically ignore asynchronous XMLHttpRequests made in an unload handler.
        /// See https://developer.mozilla.org/en-US/docs/Web/API/Navigator/sendBeacon
        /// </summary>
        /// <param name="url"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public async ValueTask<bool> SendBeacon(Uri url, object data)
        {
            return await JsRuntime.HasProperty(JsObjectRef, "sendBeacon").ConfigureAwait(false) &&
                   await JsRuntime.InvokeInstanceMethod<bool>(JsObjectRef, "sendBeacon", url, data).ConfigureAwait(false);
        }

        /// <summary>
        /// Invokes the native sharing mechanism of the device.
        /// Use CanShare to check if this is allowed
        /// </summary>
        /// <returns></returns>
        //public async ValueTask Share(ShareData shareData)
        //{
        //    await JsRuntime.InvokeInstanceMethod<bool>(JsObjectRef, "share", shareData).ConfigureAwait(false);
        //}

        /// <summary>
        /// Pulses the vibration hardware on the device, if such hardware exists. If the device doesn't support vibration, this method has no effect. If a vibration pattern is already in progress when this method is called, the previous pattern is halted and the new one begins instead.
        /// </summary>
        /// <param name="pattern">Each value indicates a number of milliseconds to vibrate or pause, in alternation. An array of values to alternately vibrate, pause, then vibrate again.</param>
        /// <returns></returns>
        public async ValueTask Vibrate(IEnumerable<TimeSpan> pattern)
        {
            await JsRuntime.InvokeInstanceMethod<bool>(JsObjectRef, "vibrate",
                pattern.Select(t => t.TotalMilliseconds).ToArray()).ConfigureAwait(false);
        }
    }
}