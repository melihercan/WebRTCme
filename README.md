# UPDATE 08 Feb 24 (V2.0.0)
Added
- .NET 8 support
- Full MAUI support

Dropped
- .NET 6 & .NET 7
- Xamarin

Will release the new NuGet packages soon...

# UPDATE 14 Jan 24
Adding
- .NET 8 support
- Full MAUI support

Dropping
- .NET 6 & .NET 7
- Xamarin


# UPDATE 11 Feb 23
Started adding documentation to the [Wiki](https://github.com/melihercan/WebRTCme/wiki)

# DESKTOP UPDATE 14 Sep 22
Bindings and WebRTCme basic libraries will soon have support for desktop (Windows and MacOS initially).

# .NET MAUI UPDATE 14 Sep 22
.NET MAUI support have been added.


# MEDIASOUP UPDATE 27 Nov 21
Coming soon should have been faster than this but recently I am very busy with an ongoing project. Therefore I could spend time on this project only during weekends.
Today I verified that the MediaSoup Blazor is working :). Here is the screenshot of 6 clients connected. 
I would like to take more time to refactor the APIs considering that MediaSoup approach is different than peer to peer (mesh topology) WebRTC I already provided.
![alt text](https://github.com/melihercan/WebRTCme/blob/master/doc/MediaSoup_6Clients.JPEG)


# COMING SOON NEW NUGETS
I am actively working on this project to evolve this framework from demo level to production quality level. The upcoming improvements in the pipeline:
- Bug fixes
- Major rework of Middleware layer
- Adding file sharing, screen sharing and recording of call session or camera services
- Selecting audio/video sources
- Adding [mediasoup](https://mediasoup.org/) WebRTC media server support for group video conferencing using SFU
- Adding .NET 6 and MAUI support
- And more...

I will release the new NuGets once the MediaSoup implementation is completed.



## VS2019 16.9.x iOS Binding Issues
!!!IMPORTANT!!! vs2019 16.9.x is broken for iOS.Bindings projects on Windows.

See the workarounds:

[Building Xamarin iOS binding project not longer works in 16.9](https://developercommunity2.visualstudio.com/t/Building-Xamarin-iOS-binding-project-not/1361154?entry=problem&ref=native&refTime=1616432601108&refUserId=9fc44a6c-086f-4bc5-8dec-c2ffaa73501d)

[VS2019 Community (v16.9), iOS and Android bindings project templates are missing](https://developercommunity2.visualstudio.com/t/VS2019-Community-v169-iOS-and-Androi/1357564?entry=problem&ref=native&refTime=1616432820554&refUserId=9fc44a6c-086f-4bc5-8dec-c2ffaa73501d)

# WebRTCme
`WebRTCme` framework libraries provide [`WebRTC`](https://webrtc.org/) functionality to Blazor and Xamarin Forms applications with a single common API. [Here](https://melihercan.medium.com/webrtc-for-blazor-and-xamarin-forms-with-a-single-common-api-93ee0a2eca4b) is the link to the story published in Medium regarding this framework.

As stated in the [`WebRTC`](https://webrtc.org/) site: "With WebRTC (Real-Time Communication for the Web), you can add real-time communication capabilities to your application that works on top of an open standard. It supports video, voice, and generic data to be sent between peers, allowing developers to build powerful voice- and video-communication solutions. The technology is available on all modern browsers as well as on native clients for all major platforms. The technologies behind WebRTC are implemented as an open web standard and available as regular JavaScript APIs in all major browsers. For native clients, like Android and iOS applications, a library is available that provides the same functionality."

![alt text](https://github.com/melihercan/WebRTCme/blob/master/doc/LibsAndNugets.png)

As can be seen in the above figure, the framework provides several libraries and their corresponding NuGets:

WebRTCme: [![NuGet](https://img.shields.io/nuget/v/WebRTCme.svg)](https://www.nuget.org/packages/WebRTCme)

WebRTCme.Bindings: [![NuGet](https://img.shields.io/nuget/v/WebRTCme.Bindings.svg)](https://www.nuget.org/packages/WebRTCme.Bindings)

WebRTCme.Bindings.Blazor: [![NuGet](https://img.shields.io/nuget/v/WebRTCme.Bindings.Blazor.svg)](https://www.nuget.org/packages/WebRTCme.Bindings.Blazor)

WebRTCme.Middleware.Blazor: [![NuGet](https://img.shields.io/nuget/v/WebRTCme.Middleware.Blazor.svg)](https://www.nuget.org/packages/WebRTCme.Middleware.Blazor)

WebRTCme.Middleware.Xamarin: [![NuGet](https://img.shields.io/nuget/v/WebRTCme.Middleware.Xamarin.svg)](https://www.nuget.org/packages/WebRTCme.Middleware.Xamarin)

WebRTCme.SignallingServerProxy: [![NuGet](https://img.shields.io/nuget/v/WebRTCme.SignallingServerProxy.svg)](https://www.nuget.org/packages/WebRTCme.SignallingServerProxy)


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

## Usage of Libraries (NuGets)

TODO: ADD THIS

### WebRTCme.Bindings

### WebRTCme

### WebRTCme.Middleware.Blazor
* Check `NuGetTests` folder. Demo apps employing this NuGet are provided there as an example.
* 
### WebRTCme.Middleware.Xamarin
* Check `NuGetTests` folder. Demo apps employing this NuGet are provided there as an example.

### WebRTCme.SignallingServerProxy
* Check `NuGetTests` folder. Signalling server employing this NuGet is provided there as an example.

## Screenshots
Peer to Peer call between Blazor and Xamarin.Android
![alt text](https://github.com/melihercan/WebRTCme/blob/master/doc/Call.png)
 
Peer to Peer chat between Blazor and Xamarin.Android
![alt text](https://github.com/melihercan/WebRTCme/blob/master/doc/Chat.png)

3 Peers call: Xamarin.iOS, Blazor, Xamarin.Android 
![alt text](https://github.com/melihercan/WebRTCme/blob/master/doc/3Peers_Call.png)
 
3 Peers chat: Xamarin.iOS, Blazor, Xamarin.Android 
![alt text](https://github.com/melihercan/WebRTCme/blob/master/doc/3Peers_Chat.png)
