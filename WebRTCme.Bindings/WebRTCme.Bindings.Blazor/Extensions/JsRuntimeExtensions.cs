using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using WebRTCme.Bindings.Blazor.Interops;

namespace WebRTCme.Bindings.Blazor.Extensions
{
    public static class JsRuntimeExtensions
    {
        /// <summary>
        /// The options every outbound argument is shaped with.
        /// </summary>
        /// <remarks>
        /// <para>
        /// These two settings, and no converter list, because every converter this API needs is
        /// already applied by attribute - <c>[JsonConverter]</c> on all 29 enums, and on each
        /// property that takes a ConstrainBoolean, ConstrainDouble, ConstrainULong or
        /// MediaStreamContraintsUnion. Attributes win over the options list anyway, so repeating
        /// them here would change nothing.
        /// </para>
        /// <para>
        /// Not <c>WebRTCme.JsonHelper.WebRtcJsonSerializerOptions</c>, which says the same thing,
        /// because of layering: this assembly is a binding and sits *below* WebRTCme, which
        /// references it and not the other way round. These are also exactly the two settings the
        /// demo app used to set globally by reflection, which is what this replaces.
        /// </para>
        /// <para>
        /// Built once. System.Text.Json caches type metadata per options instance, so a fresh
        /// instance per interop call would re-do that work on every call.
        /// </para>
        /// </remarks>
        private static readonly JsonSerializerOptions WebRtcOptions = new JsonSerializerOptions
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };

        /// <summary>
        /// Shapes one argument on its way to JavaScript, or leaves it alone.
        /// </summary>
        /// <remarks>
        /// <para>
        /// WebRTCme models optional dictionary members as properties that are simply left unset, and
        /// JSInterop writes an unset property as an explicit <c>null</c> rather than omitting it. The
        /// browser rejects that outright, because null is not a member of an enum:
        /// </para>
        /// <code>
        /// TypeError: Failed to construct 'RTCPeerConnection': Failed to read the 'bundlePolicy'
        /// property from 'RTCConfiguration': The provided value 'null' is not a valid enum value
        /// of type RTCBundlePolicy.
        /// </code>
        /// <para>
        /// So <c>new RTCConfiguration { IceServers = ... }</c> - which is what SignalingConnection
        /// builds on the mesh path - could not create a peer connection at all. Until now the only
        /// way round it was for the *application* to reach into JSRuntime's non-public
        /// JsonSerializerOptions by reflection and set the ignore condition globally, which is what
        /// WebRTCme.DemoApp.Blazor does and what every other Blazor consumer would have had to
        /// discover for itself.
        /// </para>
        /// <para>
        /// Serialising here instead makes that unnecessary. IJSRuntime.InvokeAsync takes no
        /// per-call serializer options, but it writes a JsonElement through verbatim - so an
        /// argument shaped with <see cref="JsonHelper.WebRtcJsonSerializerOptions"/> arrives exactly
        /// as those options describe, whatever the host's own settings are. Those options already
        /// said the right thing; nothing was using them on this path.
        /// </para>
        /// <para>
        /// Only the API's own model classes are converted - types in namespace <c>WebRTCme</c>
        /// exactly. Everything else is passed through untouched, and deliberately:
        /// </para>
        /// <list type="bullet">
        ///   <item><c>JsObjectRef</c> and <c>JsEventHandler</c> carry the <c>__jsObjectRefId</c> and
        ///   <c>__jsEventRefHandler</c> markers that the JS side revives, and JsEventHandler holds a
        ///   DotNetObjectReference that only JSInterop knows how to marshal. Both live in
        ///   <c>WebRTCme.Bindings.Blazor.Interops</c>, so the exact-namespace test excludes them.</item>
        ///   <item><c>byte[]</c> and the primitives have transports of their own in JSInterop.</item>
        ///   <item>Enums keep their [JsonConverter] attributes, which already produce the camelCase
        ///   strings the browser expects.</item>
        /// </list>
        /// <para>
        /// Nested models need no special handling: serialising the root carries them with it, under
        /// the same options.
        /// </para>
        /// </remarks>
        private static object MarshalArg(object arg)
        {
            if (arg is null) return null;

            var type = arg.GetType();

            // Exact namespace, not a prefix: WebRTCme.Bindings.Blazor.Interops must not match.
            if (!type.IsClass || type == typeof(string) || type.Namespace != "WebRTCme") return arg;

            return JsonSerializer.SerializeToElement(arg, type, WebRtcOptions);
        }

        /// <summary>Shapes every argument of an interop call. See <see cref="MarshalArg"/>.</summary>
        private static object[] MarshalArgs(object[] args) =>
            args is null ? args : Array.ConvertAll(args, MarshalArg);

        public static JsObjectRef CreateJsObject(this IJSRuntime jsRuntime, object parent, 
            string interface_, params object[] args)
        {
            var invokeParams = new object[]
            {
                parent,
                interface_
            };
            if (args != null)
            {
                ////args = args.Where(a => a is not null).ToArray();
                invokeParams = invokeParams.Concat(args).ToArray();
            }
            var jsObjectRef = jsRuntime.Invoke<JsObjectRef>("JsInterop.createObject", invokeParams);
            return jsObjectRef;
        }

        public static void DeleteJsObjectRef(this IJSRuntime jsRuntime,  int id)
        {
            jsRuntime.Invoke<object>(
                "JsInterop.deleteObjectRef",
                new object[]
                {
                    id,
                });
        }

        public static JsObjectRef GetJsPropertyObjectRef(this IJSRuntime jsRuntime, 
            object parent,  string property)
        {
            var jsObjectRef = jsRuntime.Invoke<JsObjectRef>(
                "JsInterop.getPropertyObjectRef",
                new object[]
                {
                    parent,
                    property//,
                    //null
                });
            return jsObjectRef;
        }

        public static ValueTask<JsObjectRef> GetJsPropertyObjectRefAsync(this IJSRuntime jsRuntime,
            object parent, string property)
        {
            var jsObjectRef = jsRuntime.InvokeAsync<JsObjectRef>(
                "JsInterop.getPropertyObjectRef",
                new object[]
                {
                    parent,
                    property
                });//.ConfigureAwait(false);
            return jsObjectRef;
        }


        public static T GetJsPropertyValue<T>(this IJSRuntime jsRuntime, object parent,
            string property, object valueSpec = null)
        {
            var content = jsRuntime.Invoke<T>(
                "JsInterop.getPropertyValue",
                new object[]
                {
                    parent,
                    property,
                    valueSpec
                });
            return content;
        }

        /// <summary>
        /// Calls a JS method and deserializes the result's <b>content</b>, not a reference to it.
        /// </summary>
        /// <remarks>
        /// Use this, not <see cref="CallJsMethod{T}"/>, whenever the result is a value object to be
        /// read rather than a thing to call further methods on. <c>callMethod</c> hands back an
        /// object reference for anything object-typed, and deserializing a reference into a model
        /// leaves every property null - which is silent, and looks exactly like an API that
        /// returned nothing.
        ///
        /// <paramref name="contentSpec"/> selects which members to copy, as for
        /// <see cref="GetJsPropertyValue{T}"/>; null takes everything.
        /// </remarks>
        public static T CallJsMethodWithContent<T>(this IJSRuntime jsRuntime, object parent,
            string method, object contentSpec = null, params object[] args)
        {
            var invokeParams = new object[]
            {
                parent,
                method,
                contentSpec
            };
            if (args != null)
            {
                invokeParams = invokeParams.Concat(args).ToArray();
            }
            return jsRuntime.Invoke<T>("JsInterop.callMethodWithContent", invokeParams);
        }

        public static IEnumerable<JsObjectRef> GetJsPropertyArray(this IJSRuntime jsRuntime,
            object parent, string property = null)
        {
            var jsObjectRefs = jsRuntime.Invoke<IEnumerable<JsObjectRef>>(
                "JsInterop.getPropertyArray",
                new object[]
                {
                    parent,
                    property,
                });
            return jsObjectRefs;
        }


        public static void SetJsProperty(this IJSRuntime jsRuntime, object parent,
            string property, object value)
        {
            jsRuntime.InvokeVoid(
                "JsInterop.setProperty",
                new object[]
                {
                    parent,
                    property,
                    value
                });
        }

        public static void CallJsMethodVoid(this IJSRuntime jsRuntime, object parent,
            string method, params object[] args)
        {
            var invokeParams = new object[]
            {
                parent,
                method
            };
            if (args != null)
            {
                invokeParams = invokeParams.Concat(args).ToArray();
            }
            jsRuntime.InvokeVoid("JsInterop.callMethod", invokeParams);
        }

        public static T CallJsMethod<T>(this IJSRuntime jsRuntime, object parent,
            string method, params object[] args)
        {
            var invokeParams = new object[]
            {
                parent,
                method
            };
            if (args != null)
            {
                invokeParams = invokeParams.Concat(args).ToArray();
            }
            var ret = jsRuntime.Invoke<T>("JsInterop.callMethod", invokeParams);
            return ret;
        }

        public static ValueTask CallJsMethodVoidAsync(this IJSRuntime jsRuntime, object parent,
            string method, params object[] args)
        {
            var invokeParams = new object[]
            {
                parent,
                method
            };
            if (args != null)
            {
                invokeParams = invokeParams.Concat(args).ToArray();
            }
            return jsRuntime.InvokeVoidAsync("JsInterop.callMethodAsync", MarshalArgs(invokeParams));
                //.ConfigureAwait(false);
        }

        public static async ValueTask<T> CallJsMethodAsync<T>(this IJSRuntime jsRuntime, 
            object parent, string method, params object[] args)
        {
            var invokeParams = new object[]
            {
                parent,
                method
            };
            if(args != null)
            {
                invokeParams = invokeParams.Concat(args).ToArray();
            }
            var ret = await jsRuntime.InvokeAsync<T>("JsInterop.callMethodAsync", MarshalArgs(invokeParams))
                .ConfigureAwait(false);
            return ret;
        }

        public static async ValueTask<Dictionary<string, Dictionary<string, JsonElement>>>
            GetJsStatsAsync(this IJSRuntime jsRuntime, object parent, params object[] args)
        {
            var invokeParams = new object[] { parent };
            if (args != null)
            {
                invokeParams = invokeParams.Concat(args).ToArray();
            }
            return await jsRuntime
                .InvokeAsync<Dictionary<string, Dictionary<string, JsonElement>>>(
                    "JsInterop.getStats", MarshalArgs(invokeParams))
                .ConfigureAwait(false);
        }

        public static string[] GetJsRemoteCertificates(this IJSRuntime jsRuntime, object parent) =>
            jsRuntime.Invoke<string[]>("JsInterop.getRemoteCertificates", parent);

        public static IDisposable AddJsEventListener(this IJSRuntime jsRuntime,
            JsObjectRef jsObjectRef, string property, string event_, JsEventHandler callBack)
        {
            var listenerId = jsRuntime.Invoke<int>("JsInterop.addEventListener", jsObjectRef,
                property, event_, callBack);

            return new DisposableAction(() =>
                jsRuntime.InvokeVoid("JsInterop.removeEventListener", jsObjectRef, property,
                    event_, listenerId));
        }

        //public static async ValueTask<IAsyncDisposable> AddJsEventListener(this IJSRuntime jsRuntime,
        //    JsObjectRef jsObjectRef, string property, string event_, JsEventHandler callBack)
        //{
        //    var listenerId = await jsRuntime.InvokeAsync<int>("JsInterop.addEventListener", jsObjectRef,
        //        property, event_, callBack).ConfigureAwait(false);

        //    return new ActionAsyncDisposable(async () =>
        //        await jsRuntime.InvokeVoidAsync("JsInterop.removeEventListener", jsObjectRef, property,
        //            event_, listenerId).ConfigureAwait(false));
        //}



        public static TValue Invoke<TValue>(this IJSRuntime jsRuntime, string identifier, params object[] args)
        {
            var isWasm = jsRuntime is IJSInProcessRuntime;

            if (isWasm)
            {
                return ((IJSInProcessRuntime)jsRuntime).Invoke<TValue>(identifier, MarshalArgs(args));
            }
            else
            {
                // Sync call to JSInterop is not possible.
                // Blocking current thread with any kind of Wait throws:
                //   Exception thrown: 'System.Threading.Tasks.TaskCanceledException' in System.Private.CoreLib.dll
                // Async API is required.
                throw new NotImplementedException();
            }
        }

        //private static async void Sync<TValue>(IJSRuntime jsRuntime, string identifier, params object[] args)
        //{
        //    TValue value = default(TValue);
        //    value = await jsRuntime.InvokeAsync<TValue>(identifier, args).Wait();
        //}

        public static void InvokeVoid(this IJSRuntime jsRuntime, string identifier, params object[] args)
        {
            var isWasm = jsRuntime is IJSInProcessRuntime;

            if (isWasm)
            {
                _ = (IJSInProcessRuntime)jsRuntime.Invoke<object>(identifier, MarshalArgs(args));
            }
            else
            {
                // Sync call to JSInterop is not possible.
                // Blocking current thread with any kind of Wait throws:
                //   Exception thrown: 'System.Threading.Tasks.TaskCanceledException' in System.Private.CoreLib.dll
                // Async API is required.
                throw new NotImplementedException();
            }
        }
    }
}