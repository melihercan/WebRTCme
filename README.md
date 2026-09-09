# 10 Feb 24 - V2.0.0 HAS BEEN RELEASED
This release brings major upgrades with breaking changes, including platform and tool changes. For further details and the project's history, please refer to the previous [README](https://github.com/melihercan/WebRTCme/blob/master/README_V1.md). 

Added: 
- .NET MAUI 
- .NET 8 

Dropped: 
- Xamarin 
- .NET 5, 6, and 7 

The Xamarin code has since been removed from the repo; the previous README above is the record of it. 

Special thanks and credits to [Gøran Yri](https://github.com/EagleDelux) for his major contributions to .NET MAUI porting. 

MediaSoup now works on Blazor, Android and iOS - verified with the three in one call. It is an SFU
alternative to the peer-to-peer signaling connection, and the server it talks to is versatica's
mediasoup-demo, which you deploy yourself; see
[WebRTCme.Connection/MediaSoup/README.md](WebRTCme.Connection/MediaSoup/README.md). Mac Catalyst is
compile-verified only. Screen sharing and mute are not wired up for this connection type yet.


![alt text](https://github.com/melihercan/WebRTCme/blob/master/non-code/docs/LibrariesAndPackages.png)

## Packages

There are two.

WebRTCme: [![NuGet](https://img.shields.io/nuget/v/WebRTCme.svg)](https://www.nuget.org/packages/WebRTCme)

WebRTCme.Middleware: [![NuGet](https://img.shields.io/nuget/v/WebRTCme.Middleware.svg)](https://www.nuget.org/packages/WebRTCme.Middleware)

### Use case 1 - Cross-platform library
- Use the **WebRTCme** package
- A single cross-platform API over the native WebRTC bindings, which it contains
- Build your own middleware or app on top of it

### Use case 2 - Middleware
- Use the **WebRTCme.Middleware** package, which brings in WebRTCme
- Adds view models, media elements and the connection layer (peer-to-peer signaling and mediasoup)
- Build your app directly on top of it

Both packages carry every target framework - `net10.0` for Blazor, and `net10.0-android`,
`net10.0-ios`, `net10.0-maccatalyst` and `net10.0-windows` for .NET MAUI - so you reference the
same package whatever you are building and NuGet picks the slice that matches.

## Upgrading from 2.0.0

Two things need editing in a project.

**Blazor apps: the JavaScript moved.** It ships with the WebRTCme package now rather than the
Blazor bindings package, so in `wwwroot/index.html`:

```diff
-<script src="_content/WebRTCme.Bindings.Blazor/JsInterop.js"></script>
+<script src="_content/WebRTCme/JsInterop.js"></script>
```

**Three packages are retired.** `WebRTCme.Api`, `WebRTCme.Bindings` and `WebRTCme.Bindings.Blazor`
are no longer published - their assemblies are inside the WebRTCme package. Remove any
`PackageReference` to them; referencing WebRTCme alone replaces all three.

`WebRTCme.Api` went further than the other two: it is no longer a separate assembly at all, its
types are compiled into `WebRTCme.dll`. They kept the `WebRTCme` namespace, so `using WebRTCme;`
and every type name are unchanged and no source edit is needed. Only something bound to the
*assembly* - a raw `<Reference Include="WebRTCme.Api" />`, or a pre-compiled third-party library
built against it - has to be rebuilt.

Nothing else moved. `WebRTCme.Middleware`, `WebRTCme.Middleware.Blazor` and
`WebRTCme.Middleware.Maui` became one assembly, but all three namespaces are preserved, so
existing `using` directives keep working. .NET MAUI XAML naming the assembly explicitly
(`assembly=WebRTCme.Middleware.Maui`) is better written `assembly=WebRTCme.Middleware`, though the
XAML compiler resolves the old spelling anyway by searching referenced assemblies.

Mac Catalyst users should take this release specifically. Every earlier package built on Windows
carried a broken `WebRTC.framework`: git cannot write symlinks on a Windows checkout without
`core.symlinks`, so the framework's binary arrived as a 23-byte text file holding a path, and that
is what was packed. The framework is stored flat now, and the native binary is really in it.


