//using BrowserInterop.Extensions;
//using BrowserInterop.Performance;
//using BrowserInterop.Screen;
//using BrowserInterop.Storage;
using Microsoft.JSInterop;
using System;
using System.Threading.Tasks;
using WebRtcJsInterop.Extensions;

namespace WebRtcJsInterop.Interops
{
    public class WindowInterop : JsObjectWrapperBase
    {
        internal static readonly object SerializationSpec = new
        {
            closed = true,
            innerHeight = true,
            innerWidth = true,
            isSecureContext = true,
            name = true,
            origin = true,
            outerHeight = true,
            outerWidth = true,
            screenX = true,
            screenY = true,
            scrollX = true,
            scrollY = true
        };

        //private Lazy<WindowHistory> historyInteropLazy;
        //private Lazy<WindowFramesArray> framesArrayInteropLazy;
        //private Lazy<WindowStorage> localStorageLazy;
        //private Lazy<WindowStorage> sessionStorageLazy;
        //private Lazy<WindowConsole> consoleInteropLazy;
        //private Lazy<WindowPerformance> performanceInteropLazy;
        //private Lazy<BarProp> locationBarLazy;
        //private Lazy<BarProp> menuBarLazy;
        //private Lazy<BarProp> personalBarLazy;
        //private Lazy<BarProp> scrollBarsLazy;
        //private Lazy<BarProp> statusBarLazy;
        //private Lazy<BarProp> toolBarLazy;

        internal override void SetJsRuntime(IJSRuntime jsRuntime, JsRuntimeObjectRef jsRuntimeObjectRef)
        {
            base.SetJsRuntime(jsRuntime, jsRuntimeObjectRef);
            //localStorageLazy =
            //    new Lazy<WindowStorage>(() => new WindowStorage(jsRuntime, jsRuntimeObjectRef, "localStorage"));
            //sessionStorageLazy =
            //    new Lazy<WindowStorage>(() => new WindowStorage(jsRuntime, jsRuntimeObjectRef, "sessionStorage"));
            //consoleInteropLazy = new Lazy<WindowConsole>(() => new WindowConsole(jsRuntime, jsRuntimeObjectRef));
            //historyInteropLazy = new Lazy<WindowHistory>(() => new WindowHistory(jsRuntime, jsRuntimeObjectRef));
            //performanceInteropLazy =
            //    new Lazy<WindowPerformance>(() => new WindowPerformance(jsRuntime, jsRuntimeObjectRef));
            //framesArrayInteropLazy =
            //    new Lazy<WindowFramesArray>(() => new WindowFramesArray(jsRuntimeObjectRef, jsRuntime));
            //personalBarLazy = new Lazy<BarProp>(() => new BarProp(jsRuntimeObjectRef, "personalbar", jsRuntime));
            //locationBarLazy = new Lazy<BarProp>(() => new BarProp(jsRuntimeObjectRef, "locationbar", jsRuntime));
            //menuBarLazy = new Lazy<BarProp>(() => new BarProp(jsRuntimeObjectRef, "menubar", jsRuntime));
            //scrollBarsLazy = new Lazy<BarProp>(() => new BarProp(jsRuntimeObjectRef, "scrollbars", jsRuntime));
            //statusBarLazy = new Lazy<BarProp>(() => new BarProp(jsRuntimeObjectRef, "statusbar", jsRuntime));
            //toolBarLazy = new Lazy<BarProp>(() => new BarProp(jsRuntimeObjectRef, "toolbar", jsRuntime));
        }


        /// <summary>
        /// Will return an instance of WindowConsole that will give access to window.console API
        /// </summary>
        /// <value></value>
        //public WindowConsole Console => consoleInteropLazy.Value;

        /// <summary>
        /// Indicates whether the referenced window is closed or not.
        /// </summary>
        /// <value></value>
        public bool Closed { get; set; }

        /// <summary>
        /// Will return an instance of WindowNavigator that will give access to window.navigator API
        /// </summary>
        /// <value></value>
        public async ValueTask<NavigatorInterop> Navigator()
        {
            return await JsRuntime.GetInstancePropertyWrapper<NavigatorInterop>(JsObjectRef, "navigator").ConfigureAwait(false);
        }

        /// <summary>
        /// Give access to the direct sub-frames of the current window.
        /// </summary>
        //public WindowFramesArray Frames => framesArrayInteropLazy.Value;

        /// <summary>
        /// reference to the History object, which provides an interface for manipulating the browser session history (pages visited in the tab or frame that the current page is loaded in).
        /// </summary>
        //public WindowHistory History => historyInteropLazy.Value;

        /// <summary>
        /// Gets the height of the content area of the browser window including, if rendered, the horizontal scrollbar.
        /// </summary>
        /// <value></value>
        public int InnerHeight { get; set; }

        /// <summary>
        /// Gets the width of the content area of the browser window including, if rendered, the vertical scrollbar.
        /// </summary>
        /// <value></value>
        public int InnerWidth { get; set; }

        /// <summary>
        /// Returns a boolean indicating whether the current context is secure (true) or not (false).
        /// </summary>
        /// <value></value>
        public bool IsSecureContext { get; set; }


        /// <summary>
        /// Returns the locationbar object, whose visibility can be checked.
        /// </summary>
        //public BarProp LocationBar => locationBarLazy.Value;

        /// <summary>
        /// Returns the menubar object, whose visibility can be checked.
        /// </summary>
        //public BarProp MenuBar => menuBarLazy.Value;

        /// <summary>
        /// reference to the local storage object used to store data that may only be accessed by the origin that created it.
        /// </summary>
        //public WindowStorage LocalStorage => localStorageLazy.Value;

        /// <summary>
        /// Returns a reference to the session storage object used to store data that may only be accessed by the origin that created it.
        /// </summary>
        //public WindowStorage SessionStorage => sessionStorageLazy.Value;

        /// <summary>
        /// Gets the name of the window.
        /// </summary>
        /// <value></value>
        public string Name { get; set; }

        /// <summary>
        /// Set the name of the window
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public async ValueTask SetName(string name)
        {
            await JsRuntime.SetInstanceProperty(JsObjectRef, "name", name).ConfigureAwait(false);
        }


        /// <summary>
        /// Returns the global object's origin, serialized as a string. 
        /// </summary>
        /// <value></value>
        public string Origin { get; set; }

        /// <summary>
        /// Returns a reference to the window that opened this current window.
        /// </summary>
        /// <returns></returns>
        public async ValueTask<WindowInterop> Opener()
        {
            return await JsRuntime.GetInstancePropertyWrapper<WindowInterop>(JsObjectRef, "opener", SerializationSpec).ConfigureAwait(false);
        }

        /// <summary>
        /// Gets the height of the outside of the browser window.
        /// </summary>
        /// <value></value>
        public int OuterHeight { get; set; }

        /// <summary>
        /// Gets the width of the outside of the browser window.
        /// </summary>
        /// <value></value>
        public int OuterWidth { get; set; }


        /// <summary>
        /// Returns a reference to the parent of the current window or child frame
        /// </summary>
        /// <returns></returns>
        public async ValueTask<WindowInterop> Parent()
        {
            return await JsRuntime.GetInstancePropertyWrapper<WindowInterop>(JsObjectRef, "parent", SerializationSpec).ConfigureAwait(false);
        }

        /// <summary>
        ///  can be used to gather performance information about the current document.
        /// </summary>
        //public WindowPerformance Performance => performanceInteropLazy.Value;

        /// <summary>
        /// Returns the personalbar object, whose visibility can be toggled in the window.
        /// </summary>
        //public BarProp PersonalBar => personalBarLazy.Value;

        /// <summary>
        /// Will return an instance of WindowNavigator that will give access to window.navigator API
        /// </summary>
        /// <value></value>
        //public async ValueTask<WindowScreen> Screen()
        //{
        //    return await JsRuntime.GetInstancePropertyWrapper<WindowScreen>(JsObjectRef, "screen").ConfigureAwait(false);
        //}

        /// <summary>
        /// return the horizontal distance from the left border of the user's browser viewport to the left side of the screen.
        /// </summary>
        /// <value></value>
        public int ScreenX { get; set; }

        /// <summary>
        ///  return the vertical distance from the top border of the user's browser viewport to the top side of the screen.
        /// </summary>
        /// <value></value>
        public int ScreenY { get; set; }

        /// <summary>
        /// Returns the scrollbars object, whose visibility can be toggled in the window.
        /// </summary>
        //public BarProp ScrollBars => scrollBarsLazy.Value;

        /// <summary>
        /// the number of pixels that the document has already been scrolled horizontally.
        /// </summary>
        /// <value></value>
        public decimal ScrollX { get; set; }

        /// <summary>
        /// Returns the number of pixels that the document has already been scrolled vertically.
        /// </summary>
        /// <value></value>
        public decimal ScrollY { get; set; }

        /// <summary>
        /// Returns the statusbar object, whose visibility can be toggled in the window.
        /// </summary>
        //public BarProp StatusBar => statusBarLazy.Value;


        /// <summary>
        /// Returns the toolbar object, whose visibility can be toggled in the window.
        /// </summary>
        //public BarProp ToolBar => toolBarLazy.Value;

        /// <summary>
        /// Returns a reference to the parent of the current window
        /// </summary>
        /// <returns></returns>
        public async ValueTask<WindowInterop> Top()
        {
            return await JsRuntime.GetInstancePropertyWrapper<WindowInterop>(JsObjectRef, "top", SerializationSpec).ConfigureAwait(false);
        }

        /// <summary>
        ///  represents the visual viewport for a given window. For a page containing iframes, each iframe, as well as the containing page, will have a unique window object. Each window on a page will have a unique VisualViewport representing the properties associated with that window.
        /// </summary>
        /// <returns></returns>
        //public async ValueTask<WindowVisualViewPort> VisualViewport()
        //{
        //    return await JsRuntime.GetInstancePropertyWrapper<WindowVisualViewPort>(JsObjectRef, "visualViewport").ConfigureAwait(false);
        //}



        /// <summary>
        /// A string you want to display in the alert dialog
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public async ValueTask Alert(string message)
        {
            await JsRuntime.InvokeInstanceMethod(JsObjectRef, "alert", message).ConfigureAwait(false);
        }

        /// <summary>
        /// Shifts focus away from the window.
        /// </summary>
        /// <returns></returns>
        public async ValueTask Blur()
        {
            await JsRuntime.InvokeInstanceMethod(JsObjectRef, "blur").ConfigureAwait(false);
        }

        /// <summary>
        /// Closes the current window.
        /// </summary>
        /// <returns></returns>
        public async ValueTask Close()
        {
            await JsRuntime.InvokeInstanceMethod(JsObjectRef, "close").ConfigureAwait(false);
        }

        /// <summary>
        /// The Window.confirm() method displays a modal dialog with an optional message and two buttons: OK and Cancel.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public async ValueTask<bool> Confirm(string message)
        {
            return await JsRuntime.InvokeInstanceMethod<bool>(JsObjectRef, "confirm", message).ConfigureAwait(false);
        }

        /// <summary>
        /// Makes a request to bring the window to the front. It may fail due to user settings and the window isn't guaranteed to be frontmost before this method returns.
        /// </summary>
        /// <returns></returns>
        public async ValueTask Focus()
        {
            await JsRuntime.InvokeInstanceMethod(JsObjectRef, "focus").ConfigureAwait(false);
        }

        /// <summary>
        /// moves the current window by a specified amount.
        /// </summary>
        /// <param name="deltaX">the amount of pixels to move the window horizontally. Positive values are to the right, while negative values are to the left.</param>
        /// <param name="deltaY">the amount of pixels to move the window vertically. Positive values are down, while negative values are up.</param>
        /// <returns></returns>
        public async ValueTask MoveBy(int deltaX, int deltaY)
        {
            await JsRuntime.InvokeInstanceMethod(JsObjectRef, "moveBy", deltaX, deltaY).ConfigureAwait(false);
        }

        /// <summary>
        /// moves the current window to the specified coordinates.
        /// </summary>
        /// <param name="x">the horizontal coordinate to be moved to.</param>
        /// <param name="y">the vertical coordinate to be moved to.</param>
        /// <returns></returns>
        public async ValueTask MoveTo(int x, int y)
        {
            await JsRuntime.InvokeInstanceMethod(JsObjectRef, "moveTo", x, y).ConfigureAwait(false);
        }

        /// <summary>
        /// loads the specified resource into the browsing context (window, &lt;iframe&gt; or tab) with the specified name. If the name doesn't exist, then a new window is opened and the specified resource is loaded into its browsing context.
        /// </summary>
        /// <param name="url">URL of the resource to be loaded. This can be a path or URL to an HTML page, image file, or any other resource which is supported by the browser. If the empty string ("") is specified as url, a blank page is opened into the targeted browsing context.</param>
        /// <param name="windowName">the name of the browsing context (window,  &lt;iframe&gt; or tab) into which to load the specified resource; if the name doesn't indicate an existing context, a new window is created and is given the name specified by windowName.</param>
        /// <param name="windowFeature">comma-separated list of window features given with their corresponding values in the form "name=value". These features include options such as the window's default size and position, whether or not to include scroll bars, and so forth. There must be no whitespace in the string.</param>
        /// <returns></returns>
        //public async ValueTask<WindowInterop> Open(Uri url, string windowName = null,
        //    WindowFeature windowFeature = null)
        //{
        //    var windowOpenRef = await JsRuntime.InvokeInstanceMethodGetRef(JsObjectRef, "open", url, windowName,
        //        windowFeature?.GetOpenString()).ConfigureAwait(false);
        //    var windowInterop = await JsRuntime.GetInstanceContent<WindowInterop>(windowOpenRef, SerializationSpec).ConfigureAwait(false);
        //    windowInterop.SetJsRuntime(JsRuntime, windowOpenRef);
        //    return windowInterop;
        //}

        /// <summary>
        /// Safely enables cross-origin communication between Window objects; e.g., between a page and a pop-up that it spawned, or between a page and an iframe embedded within it.
        /// </summary>
        /// <param name="message">The object that will be send to the other window </param>
        /// <param name="targetOrigin">Specifies what the origin of targetWindow must be for the event to be dispatched, either as the literal string "*" (indicating no preference) or as a URI. If at the time the event is scheduled to be dispatched the scheme, hostname, or port of targetWindow's document does not match that provided in targetOrigin, the event will not be dispatched; only if all three match will the event be dispatched.  </param>
        /// <returns></returns>
        public async ValueTask PostMessage(object message, string targetOrigin)
        {
            await JsRuntime.InvokeInstanceMethod(JsObjectRef, "postMessage", message, targetOrigin).ConfigureAwait(false);
        }

        /// <summary>
        ///  listen for dispatched messages send by PostMessage
        /// </summary>
        /// <param name="todo"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        //public async ValueTask<IAsyncDisposable> OnMessage<T>(Func<MessageEvent<T>, ValueTask> todo)
        //{
        //    return await JsRuntime.AddEventListener(
        //        JsObjectRef,
        //        "",
        //        "message",
        //        CallBackInteropWrapper.Create<JsRuntimeObjectRef>(
        //            async payload =>
        //            {
        //                var eventPayload = new MessageEvent<T>
        //                {
        //                    Data = await JsRuntime.GetInstanceProperty<T>(payload, "data").ConfigureAwait(false),
        //                    Origin = await JsRuntime.GetInstanceProperty<string>(payload, "origin")
        //                        .ConfigureAwait(false),
        //                    Source = await JsRuntime.GetInstanceProperty<WindowInterop>(payload, "source",
        //                        SerializationSpec).ConfigureAwait(false)
        //                };
        //                eventPayload.Source.SetJsRuntime(JsRuntime,
        //                    await JsRuntime.GetInstancePropertyRef(payload, "source").ConfigureAwait(false));

        //                await todo.Invoke(eventPayload).ConfigureAwait(false);
        //            },
        //            getJsObjectRef: true
        //        )).ConfigureAwait(false);
        //}

        /// <summary>
        /// Opens the Print Dialog to print the current document.
        /// </summary>
        /// <returns></returns>
        public async ValueTask Print()
        {
            await JsRuntime.InvokeInstanceMethod(JsObjectRef, "print").ConfigureAwait(false);
        }

        /// <summary>
        /// displays a dialog with an optional message prompting the user to input some text.
        /// </summary>
        /// <param name="message">A string of text to display to the user. Can be omitted if there is nothing to show in the prompt window.</param>
        /// <param name="defaultValue">A string containing the default value displayed in the text input field. </param>
        /// <returns></returns>
        public async ValueTask<string> Prompt(string message, string defaultValue = null)
        {
            return await JsRuntime.InvokeInstanceMethod<string>(JsObjectRef, "prompt", message, defaultValue).ConfigureAwait(false);
        }

        /// <summary>
        /// Tells the browser that you wish to perform an animation and requests that the browser calls a specified function to update an animation before the next repaint. The method takes a callback as an argument to be invoked before the repaint.
        /// </summary>
        /// <param name="callback">ells the browser that you wish to perform an animation and requests that the browser calls a specified function to update an animation before the next repaint. The method takes a callback as an argument to be invoked before the repaint.</param>
        /// <returns>The request ID, can be used for cancelling it</returns>
        //public async ValueTask<int> RequestAnimationFrame(Func<double, ValueTask> callback)
        //{
        //    return await JsRuntime.InvokeInstanceMethod<int>(JsObjectRef, "requestAnimationFrame",
        //        CallBackInteropWrapper.Create(callback)).ConfigureAwait(false);
        //}

        /// <summary>
        /// cancels an animation frame request previously scheduled through a call to RequestAnimationFrame().
        /// </summary>
        /// <param name="id">Id returned by RequestAnimationFrame</param>
        /// <returns></returns>
        public async ValueTask CancelAnimationFrame(int id)
        {
            await JsRuntime.InvokeInstanceMethod(JsObjectRef, "cancelAnimationFrame", id).ConfigureAwait(false);
        }

        /// <summary>
        /// queues a function to be called during a browser's idle periods. This enables developers to perform background and low priority work on the main event loop, without impacting latency-critical events such as animation and input response.
        /// </summary>
        /// <param name="callback">ells the browser that you wish to perform an animation and requests that the browser calls a specified function to update an animation before the next repaint. The method takes a callback as an argument to be invoked before the repaint.</param>
        /// <param name="options">Contains optional configuration parameters. Currently only one property is defined : timeout. If timeout is specified and has a positive value, and the callback has not already been called by the time timeout milliseconds have passed, the callback will be called during the next idle period, even if doing so risks causing a negative performance impact.
        /// </param>
        /// <returns>The request ID, can be used for cancelling it</returns>
        //public async ValueTask<int> RequestIdleCallback(Func<IdleDeadline, ValueTask> callback,
        //    RequestIdleCallbackOptions options = null)
        //{
        //    return await JsRuntime.InvokeInstanceMethod<int>(JsObjectRef, "requestIdleCallback",
        //        CallBackInteropWrapper.Create<JsRuntimeObjectRef>(async jsRef =>
        //        {
        //            var idleDeadline = await JsRuntime.GetInstanceContent<IdleDeadline>(jsRef, true).ConfigureAwait(false);
        //            idleDeadline.SetJsRuntime(JsRuntime, jsRef);
        //            await callback.Invoke(idleDeadline).ConfigureAwait(false);
        //        }, getJsObjectRef: true), options).ConfigureAwait(false);
        //}

        /// <summary>
        /// cancels a callback previously scheduled with RequestIdleCallback()
        /// </summary>
        /// <param name="id">Id returned by RequestIdleCallback</param>
        /// <returns></returns>
        public async ValueTask CancelIdleCallback(int id)
        {
            await JsRuntime.InvokeInstanceMethod(JsObjectRef, "cancelIdleCallback", id).ConfigureAwait(false);
        }

        /// <summary>
        ///  resizes the current window by a specified amount.
        /// </summary>
        /// <param name="xDelta">the number of pixels to grow the window horizontally.</param>
        /// <param name="yDelta"> the number of pixels to grow the window vertically.</param>
        /// <returns></returns>
        public async ValueTask ResizeBy(int xDelta, int yDelta)
        {
            await JsRuntime.InvokeInstanceMethod(JsObjectRef, "resizeBy", xDelta, yDelta).ConfigureAwait(false);
        }

        /// <summary>
        /// dynamically resizes the window.
        /// </summary>
        /// <param name="width">An integer representing the new outerWidth in pixels (including scroll bars, title bars, etc).</param>
        /// <param name="height">An integer value representing the new outerHeight in pixels (including scroll bars, title bars, etc).</param>
        /// <returns></returns>
        public async ValueTask ResizeTo(int width, int height)
        {
            await JsRuntime.InvokeInstanceMethod(JsObjectRef, "resizeTo", width, height).ConfigureAwait(false);
        }

        /// <summary>
        /// scrolls the window to a particular place in the document.
        /// </summary>
        /// <param name="xCoord">the pixel along the horizontal axis of the document that you want displayed in the upper left.</param>
        /// <param name="yCoord">the pixel along the vertical axis of the document that you want displayed in the upper left.</param>
        /// <returns></returns>
        public async ValueTask Scroll(int xCoord, int yCoord)
        {
            await JsRuntime.InvokeInstanceMethod(JsObjectRef, "scroll", xCoord, yCoord).ConfigureAwait(false);
        }

        /// <summary>
        /// scrolls the window to a particular place in the document.
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        //public async ValueTask Scroll(ScrollToOptions options)
        //{
        //    await JsRuntime.InvokeInstanceMethod(JsObjectRef, "scroll", options).ConfigureAwait(false);
        //}

        /// <summary>
        /// scrolls the document in the window by the given amount.
        /// </summary>
        /// <param name="xCoord">the pixel along the horizontal axis of the document that you want displayed in the upper left.</param>
        /// <param name="yCoord">the pixel along the vertical axis of the document that you want displayed in the upper left.</param>
        /// <returns></returns>
        public async ValueTask ScrollBy(int xCoord, int yCoord)
        {
            await JsRuntime.InvokeInstanceMethod(JsObjectRef, "scrollBy", xCoord, yCoord).ConfigureAwait(false);
        }

        /// <summary>
        /// scrolls the document in the window by the given amount.
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        //public async ValueTask ScrollBy(ScrollToOptions options)
        //{
        //    await JsRuntime.InvokeInstanceMethod(JsObjectRef, "scrollBy", options).ConfigureAwait(false);
        //}

        /// <summary>
        ///  stops further resource loading in the current browsing context, equivalent to the stop button in the browser.
        /// </summary>
        /// <returns></returns>
        public async ValueTask Stop()
        {
            await JsRuntime.InvokeInstanceMethod(JsObjectRef, "stop").ConfigureAwait(false);
        }

        /// <summary>
        /// Called when the page is installed as a webapp.
        /// </summary>
        /// <param name="callback"></param>
        /// <returns></returns>
        //public async ValueTask<IAsyncDisposable> OnAppInstalled(Func<ValueTask> callback)
        //{
        //    return await JsRuntime.AddEventListener(JsObjectRef, "", "appinstalled",
        //        CallBackInteropWrapper.Create(callback, false)).ConfigureAwait(false);
        //}

        /// <summary>
        /// Fired when a resource failed to load, or can't be used. For example, if a script has an execution error or an image can't be found or is invalid.
        /// </summary>
        /// <param name="callback"></param>
        /// <returns></returns>
        //public async ValueTask<IAsyncDisposable> OnError(Func<ValueTask> callback)
        //{
        //    return await JsRuntime.AddEventListener(JsObjectRef, "", "error",
        //        CallBackInteropWrapper.Create(callback, false)).ConfigureAwait(false);
        //}

        /// <summary>
        /// The languagechange event is fired at the global scope object when the user's preferred language changes.
        /// </summary>
        /// <param name="callback"></param>
        /// <returns></returns>
        //public async ValueTask<IAsyncDisposable> OnLanguageCHange(Func<ValueTask> callback)
        //{
        //    return await JsRuntime.AddEventListener(JsObjectRef, "", "languagechange",
        //        CallBackInteropWrapper.Create(callback, false)).ConfigureAwait(false);
        //}

        /// <summary>
        /// The orientationchange event is fired when the orientation of the device has changed.
        /// </summary>
        /// <param name="callback"></param>
        /// <returns></returns>
        //public async ValueTask<IAsyncDisposable> OnOrientationChange(Func<ValueTask> callback)
        //{
        //    return await JsRuntime.AddEventListener(JsObjectRef, "", "orientationchange",
        //        CallBackInteropWrapper.Create(callback, false)).ConfigureAwait(false);
        //}

        /// <summary>
        /// dispatched on devices when a user is about to be prompted to "install" a web application. Its associated event may be saved for later and used to prompt the user at a more suitable time. 
        /// </summary>
        /// <param name="callback"></param>
        /// <returns></returns>
        //public async ValueTask<IAsyncDisposable> OnBeforeInstallPrompt(
        //    Func<BeforeInstallPromptEvent, ValueTask> callback)
        //{
        //    return await JsRuntime.AddEventListener(
        //        JsObjectRef, "",
        //        "beforeinstallprompt",
        //        CallBackInteropWrapper.Create<JsRuntimeObjectRef>(
        //            async jsObjectRef =>
        //            {
        //                var beforeInstallPromptEvent = new BeforeInstallPromptEvent
        //                {
        //                    Platforms = await JsRuntime.GetInstanceProperty<string[]>(jsObjectRef, "platforms").ConfigureAwait(false)
        //                };
        //                beforeInstallPromptEvent.SetJsRuntime(JsRuntime, jsObjectRef);
        //                await callback.Invoke(beforeInstallPromptEvent).ConfigureAwait(false);
        //            },
        //            getJsObjectRef: true,
        //            serializationSpec: false
        //        )
        //    ).ConfigureAwait(false);
        //}

        /// <summary>
        /// fires when a window's hash changes
        /// </summary>
        /// <param name="callback"></param>
        /// <returns></returns>
        //public async ValueTask<IAsyncDisposable> OnHashChange(Func<ValueTask> callback)
        //{
        //    return await JsRuntime.AddEventListener(
        //        JsObjectRef, "",
        //        "hashchange",
        //        CallBackInteropWrapper.Create(
        //            async () => { await callback.Invoke().ConfigureAwait(false); },
        //            getJsObjectRef: false,
        //            serializationSpec: false
        //        )
        //    ).ConfigureAwait(false);
        //}

        /// <summary>
        /// fired at a regular interval and indicates the amount of physical force of acceleration the device is receiving at that time. It also provides information about the rate of rotation, if available.
        /// </summary>
        /// <param name="callback"></param>
        /// <returns></returns>
        //public async ValueTask<IAsyncDisposable> OnDeviceMotion(Func<DeviceMotionEvent, ValueTask> callback)
        //{
        //    return await JsRuntime.AddEventListener(
        //        JsObjectRef, "",
        //        "devicemotion",
        //        CallBackInteropWrapper.Create<DeviceMotionEvent>(
        //            async e => { await callback.Invoke(e).ConfigureAwait(false); },
        //            getJsObjectRef: false,
        //            serializationSpec: new
        //            {
        //                acceleration = "*",
        //                accelerationIncludingGravity = "*",
        //                rotationRate = "*",
        //                interval = "*"
        //            })
        //    ).ConfigureAwait(false);
        //}

        /// <summary>
        /// fired when fresh data is available from an orientation sensor about the current orientation of the device as compared to the Earth coordinate frame. This data is gathered from a magnetometer inside the device
        /// </summary>
        /// <param name="callback"></param>
        /// <returns></returns>
        //public async ValueTask<IAsyncDisposable> OnDeviceOrientation(Func<DeviceOrientationEvent, ValueTask> callback)
        //{
        //    return await JsRuntime.AddEventListener(
        //        JsObjectRef, "",
        //        "deviceorientation",
        //        CallBackInteropWrapper.Create<DeviceOrientationEvent>(
        //            async e => { await callback.Invoke(e).ConfigureAwait(false); },
        //            getJsObjectRef: false,
        //            serializationSpec: new
        //            {
        //                alpha = "*",
        //                beta = "*",
        //                gamma = "*",
        //                absolute = "*"
        //            })
        //    ).ConfigureAwait(false);
        //}


        /// <summary>
        /// raised after the user prints
        /// </summary>
        /// <param name="callback"></param>
        /// <returns></returns>
        //public async ValueTask<IAsyncDisposable> OnAfterPrint(Func<ValueTask> callback)
        //{
        //    return await JsRuntime.AddEventListener(JsObjectRef, "", "afterprint",
        //        CallBackInteropWrapper.Create(callback, false)).ConfigureAwait(false);
        //}


        /// <summary>
        /// raised before the user prints
        /// </summary>
        /// <param name="callback"></param>
        /// <returns></returns>
        //public async ValueTask<IAsyncDisposable> OnBeforePrint(Func<ValueTask> callback)
        //{
        //    return await JsRuntime.AddEventListener(JsObjectRef, "", "beforeprint",
        //        CallBackInteropWrapper.Create(callback, false)).ConfigureAwait(false);
        //}

        /// <summary>
        ///  fire when a window is about to unload its resources. At this point, the document is still visible and the event is still cancelable.
        /// </summary>
        /// <param name="callback"></param>
        /// <returns></returns>
        //public async ValueTask<IAsyncDisposable> OnBeforeUnload(Func<BeforeUnloadEvent, ValueTask> callback)
        //{
        //    return await JsRuntime.AddEventListener(
        //        JsObjectRef, "",
        //        "beforeunload",
        //        CallBackInteropWrapper.Create<JsRuntimeObjectRef>(
        //            async e => { await callback.Invoke(new BeforeUnloadEvent(JsRuntime, e)).ConfigureAwait(false); },
        //            getJsObjectRef: true)
        //    ).ConfigureAwait(false);
        //}

        /// <summary>
        /// raised when the window losses focus
        /// </summary>
        /// <param name="callback"></param>
        /// <returns></returns>
        //public async ValueTask<IAsyncDisposable> OnBlur(Func<ValueTask> callback)
        //{
        //    return await JsRuntime.AddEventListener(JsObjectRef, "", "blur",
        //        CallBackInteropWrapper.Create(callback, false)).ConfigureAwait(false);
        //}

        ///// <summary>
        ///// raised when the window is closed
        ///// </summary>
        ///// <param name="callback"></param>
        ///// <returns></returns>
        //public async ValueTask<IAsyncDisposable> OnClose(Func<ValueTask> callback)
        //{
        //    return await JsRuntime.AddEventListener(JsObjectRef, "", "close",
        //        CallBackInteropWrapper.Create(callback, false)).ConfigureAwait(false);
        //}


        ///// <summary>
        ///// raised when the window is closed
        ///// </summary>
        ///// <param name="callback"></param>
        ///// <returns></returns>
        //public async ValueTask<IAsyncDisposable> OnContextMenu(Func<CancellableEvent, ValueTask> callback)
        //{
        //    return await JsRuntime.AddEventListener(
        //        JsObjectRef, "",
        //        "contextmenu",
        //        CallBackInteropWrapper.Create<JsRuntimeObjectRef>(
        //            async e => { await callback.Invoke(new CancellableEvent(JsRuntime, e)).ConfigureAwait(false); },
        //            getJsObjectRef: true)
        //    ).ConfigureAwait(false);
        //}

        ///// <summary>
        ///// raised when the window gains focus
        ///// </summary>
        ///// <param name="callback"></param>
        ///// <returns></returns>
        //public async ValueTask<IAsyncDisposable> OnFocus(Func<ValueTask> callback)
        //{
        //    return await JsRuntime.AddEventListener(JsObjectRef, "", "focus",
        //        CallBackInteropWrapper.Create(callback, false)).ConfigureAwait(false);
        //}

        ///// <summary>
        ///// fires at the end of the document loading process. At this point, all of the objects in the document are in the DOM, and all the images, scripts, links and sub-frames have finished loading.
        ///// </summary>
        ///// <param name="callback"></param>
        ///// <returns></returns>
        //public async ValueTask<IAsyncDisposable> OnLoad(Func<ValueTask> callback)
        //{
        //    return await JsRuntime.AddEventListener(JsObjectRef, "", "load",
        //        CallBackInteropWrapper.Create(callback, false)).ConfigureAwait(false);
        //}

        ///// <summary>
        ///// fired when the browser has lost access to the network and the value of Navigator.onLine switches to false.
        ///// </summary>
        ///// <param name="callback"></param>
        ///// <returns></returns>
        //public async ValueTask<IAsyncDisposable> OnOffline(Func<ValueTask> callback)
        //{
        //    return await JsRuntime.AddEventListener(JsObjectRef, "", "offline",
        //        CallBackInteropWrapper.Create(callback, false)).ConfigureAwait(false);
        //}

        ///// <summary>
        ///// fired when the browser has gained access to the network and the value of Navigator.onLine switches to true.
        ///// </summary>
        ///// <param name="callback"></param>
        ///// <returns></returns>
        //public async ValueTask<IAsyncDisposable> OnOnline(Func<ValueTask> callback)
        //{
        //    return await JsRuntime.AddEventListener(JsObjectRef, "", "online",
        //        CallBackInteropWrapper.Create(callback, false)).ConfigureAwait(false);
        //}

        ///// <summary>
        ///// The pagehide event is sent to a Window when the browser hides the current page in the process of presenting a different page from the session's history.
        ///// </summary>
        ///// <param name="callback"></param>
        ///// <returns></returns>
        //public async ValueTask<IAsyncDisposable> OnPageHide(Func<PageTransitionEvent, ValueTask> callback)
        //{
        //    return await JsRuntime.AddEventListener(JsObjectRef, "", "pagehide",
        //        CallBackInteropWrapper.Create(callback, new {persisted = true})).ConfigureAwait(false);
        //}


        ///// <summary>
        ///// The pageshow event is sent to a Window when the browser displays the window's document due to navigation
        ///// </summary>
        ///// <param name="callback"></param>
        ///// <returns></returns>
        //public async ValueTask<IAsyncDisposable> OnPageShow(Func<PageTransitionEvent, ValueTask> callback)
        //{
        //    return await JsRuntime.AddEventListener(JsObjectRef, "", "pageshow",
        //        CallBackInteropWrapper.Create(callback, new {persisted = true})).ConfigureAwait(false);
        //}

        ///// <summary>
        ///// A popstate event is dispatched to the window every time the active history entry changes between two history entries for the same document. If the history entry being activated was created by a call to history.pushState() or was affected by a call to history.replaceState(), the popstate event's state property contains a copy of the history entry's state object.
        ///// </summary>
        ///// <param name="callback"></param>
        ///// <returns></returns>
        //public async ValueTask<IAsyncDisposable> OnPopState<T>(Func<T, ValueTask> callback)
        //{
        //    return await JsRuntime.AddEventListener(JsObjectRef, "", "popstate",
        //        CallBackInteropWrapper.Create<PopStateEvent<T>>(async p => await callback(p.State).ConfigureAwait(false), new {state = "*"})).ConfigureAwait(false);
        //}


        ///// <summary>
        ///// The resize event fires when the document view (window) has been resized.
        ///// </summary>
        ///// <param name="callback"></param>
        ///// <returns></returns>
        //public async ValueTask<IAsyncDisposable> OnResize(Func<ValueTask> callback)
        //{
        //    return await JsRuntime.AddEventListener(JsObjectRef, "", "resize",
        //        CallBackInteropWrapper.Create(callback, false)).ConfigureAwait(false);
        //}

        ///// <summary>
        ///// The scroll event fires when the document view or an element has been scrolled, whether by the user, a Web API, or the user agent.
        ///// </summary>
        ///// <param name="callback"></param>
        ///// <returns></returns>
        //public async ValueTask<IAsyncDisposable> OnScroll(Func<ValueTask> callback)
        //{
        //    return await JsRuntime.AddEventListener(JsObjectRef, "", "scroll",
        //        CallBackInteropWrapper.Create(callback, false)).ConfigureAwait(false);
        //}

        ///// <summary>
        ///// The scroll event fires when the document view or an element has been scrolled, whether by the user, a Web API, or the user agent.
        ///// </summary>
        ///// <param name="callback"></param>
        ///// <returns></returns>
        //public async ValueTask<IAsyncDisposable> OnWheel(Func<WheelEvent, ValueTask> callback)
        //{
        //    return await JsRuntime.AddEventListener(JsObjectRef, "", "wheel",
        //        CallBackInteropWrapper.Create(callback,
        //            new {deltaX = true, deltaY = true, deltaZ = true, deltaMode = true})).ConfigureAwait(false);
        //}

        //public async ValueTask<IAsyncDisposable> OnStorage(Func<StorageEvent, ValueTask> callback)
        //{
        //    return await JsRuntime.AddEventListener(JsObjectRef, "", "storage",
        //        CallBackInteropWrapper.Create<JsRuntimeObjectRef>(
        //            async jsObject =>
        //            {
        //                var eventContent = await JsRuntime.GetInstanceContent<StorageEvent>(jsObject,
        //                    new {key = true, oldValue = true, newValue = true, url = true}).ConfigureAwait(false);
        //                eventContent.Storage = new WindowStorage(JsRuntime,
        //                    await JsRuntime.GetInstancePropertyRef(jsObject, "storageArea").ConfigureAwait(false));
        //                await callback(eventContent).ConfigureAwait(false);
        //            },
        //            getJsObjectRef: true
        //        )
        //    ).ConfigureAwait(false);
        //}

        ///// <summary>
        ///// The unload event is fired when the document or a child resource is being unloaded.
        ///// </summary>
        ///// <param name="callback"></param>
        ///// <returns></returns>
        //public async ValueTask<IAsyncDisposable> OnUnload(Func<ValueTask> callback)
        //{
        //    return await JsRuntime.AddEventListener(JsObjectRef, "", "unload",
        //        CallBackInteropWrapper.Create(callback, false)).ConfigureAwait(false);
        //}
    }
}