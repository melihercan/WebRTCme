# WebRTCme
`WebRTCme` framework libraries provide [`WebRTC`](https://webrtc.org/) functionality to Blazor and Xamarin Forms applications with a single common API. [Here](https://mediumxxx) is the link to the story published in Medium regarding this framework.

As stated in the [`WebRTC`](https://webrtc.org/) site: "With WebRTC (Real-Time Communication for the Web), you can add real-time communication capabilities to your application that works on top of an open standard. It supports video, voice, and generic data to be sent between peers, allowing developers to build powerful voice- and video-communication solutions. The technology is available on all modern browsers as well as on native clients for all major platforms. The technologies behind WebRTC are implemented as an open web standard and available as regular JavaScript APIs in all major browsers. For native clients, like Android and iOS applications, a library is available that provides the same functionality."

![alt text](https://github.com/melihercan/WebRTCme/blob/master/doc/LibsAndNugets.png)

As can be seen in the above figure, the framework provides several libraries and their corresponsing NuGets.

## Bindings and API
[WebRTC](https://webrtc.googlesource.com/src) is implemented in C++. The platform wrappers are added on top of this as:
* Web Browsers: JS code is embedded with most of the browsers.
* Android: Java SDK.
* iOS: ObjC SDK.

Each platform has its own native API. [Browser API](https://developer.mozilla.org/en-US/docs/Web/API/WebRTC_API) is developed by `IETF WebRTC Working Group`. The first task of `WebRTCme` framework is to map and bring these SDKs into .NET world so that Blazor and Xamarin components can consume them:
* For Blazor: RCL library has been implemented that uses JSInterop and employs the above described `Browser API`. That API has been defined in `WebRTCme.Api` library as interfaces, models and enums. 
* For Xamarin.Android: Xamarin Android Java bindings library has been implemented.
* For Xamarin.iOS: Xamarin iOS ObjC binding library has been implemented.

Note that Xamarin libraries still have their own native APIs mapped to .NET. 

## WebRTCme Plug-in
Binding layer provides 3 separate APIs for each platform: Blazor, Xamarin Android and iOS. I decided to wrap these APIs and provide a single common API to simplify development process for consumers of this library. `WebRTCme.Api` has been implemented for all 3 platforms as a plug-in by using bait and switch PLC trick.

## Middleware
This is the services layer between applications and the `WebRTCme` plug-in. This layer provides four functionalities:
1.) Xamarin custom renderer and Blazor component to support `Media` tag. This requires platform specific implementations, hence `WebRTCme.Middleware.Blazor` and `WebRTCme.Xamarin` libraries provided on top of shared `WebRTCme.Middleware` library. 
2.) Media stream service.
3.) Blazor and Xamarin shared connection handling with signalling server through a proxy.
4.) Blazor and Xamarin shared view models to handle call and chat functionalities.

## Signalling Server Proxy
This tiny library is a bridge between middleware and the signalling server. It exposes two APIs as C# interfaces; one for the middleware and the other for the signalling server.

## Screenshots
Peer to Peer call between Blazor and Xamarin.Android
![alt text](https://github.com/melihercan/WebRTCme/blob/master/doc/Call.png)
 
Peer to Peer chat between Blazor and Xamarin.Android
![alt text](https://github.com/melihercan/WebRTCme/blob/master/doc/Chat.png)

3 Peers call: Xamarin.iOS, Blazor, Xamarin.Android 
![alt text](https://github.com/melihercan/WebRTCme/blob/master/doc/3Peers_Call.png)
 
3 Peers chat: Xamarin.iOS, Blazor, Xamarin.Android 
![alt text](https://github.com/melihercan/WebRTCme/blob/master/doc/3Peers_Chat.png)