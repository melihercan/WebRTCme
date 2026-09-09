PACKAGING
=========
WebRTCme ships as two NuGet packages, both produced by `dotnet pack` on an ordinary csproj. There
are no hand-written .nuspec files any more, and packages are never built by a local build
(`GeneratePackageOnBuild` is false everywhere) - CI builds and pushes them.

  WebRTCme              WebRTCme.Api + WebRTCme + the five bindings, and their native halves
    +-- WebRTCme.Middleware   the middleware + the connection layer (signaling and mediasoup)

Build them with:

  dotnet pack WebRTCme/WebRTCme.csproj -c Release -o <dir>
  dotnet pack WebRTCme.Middleware/WebRTCme.Middleware/WebRTCme.Middleware.csproj -c Release -o <dir>


WHY THERE ARE NO .nuspec FILES ANY MORE
=======================================
There used to be, and this file used to explain that `dotnet pack` cannot fold a referenced
project's output into your package, so the only way to ship one package built from several
projects was to merge the generated .nuspec files by hand and run `nuget pack` on the result.

That is no longer true, and the two projects above do it in MSBuild instead:

- `TargetsForTfmSpecificBuildOutput` adds the referenced projects' assemblies to
  `BuildOutputInPackage`, which puts them in lib/<tfm>/ next to the packing project's own.
- Each folded project is referenced with `PrivateAssets="all"` so pack does not record it as a
  package dependency. `IsPackable=false` does *not* do this - it stops a project being packed on
  its own, but a reference to it still becomes a dependency, at version 1.0.0 if the project never
  sets one.
- Because `PrivateAssets="all"` also stops the types flowing to anything downstream, projects in
  this repo that use WebRTCme.Api types name it themselves. A package consumer never has to: the
  assembly is right there in lib/.
- The packages a folded project needs have to be declared again on the packing project, since the
  folded project is invisible to consumers.

The one thing the old note got right is that static web assets are special. wwwroot/JsInterop.js
now lives in WebRTCme itself rather than in WebRTCme.Bindings.Blazor, which is why the Blazor
binding no longer needs a package of its own - a Razor SDK project can only pack the web assets it
owns. Consumers load it from `_content/WebRTCme/JsInterop.js`.


NATIVE PAYLOADS
===============
Pack drops anything in the build output folder that is not on
`AllowedOutputExtensionsInPackageBuildOutputFolder`, silently, so .aar and .zip are added to it.

  Android       lib/<tfm>/libwebrtc.aar          the Java classes.jar and the four .so
  iOS/MacCat    lib/<tfm>/*.resources.zip        WebRTC.xcframework / WebRTC.framework
  Windows       runtimes/win-x64/native/*.dll    WebRtcInterop.dll and its import closure

The .aar the Android SDK builds for the project itself is removed before packing: it re-emits the
same four .so and nothing else, so it is 23MB of duplication and a second .aar in one lib/ folder
carrying the same .so files.


VERIFYING A CHANGE
==================
Building is not enough - pack drops things quietly. Read the package:

  unzip -l <package>.nupkg
  unzip -p <package>.nupkg <id>.nuspec        # check the dependency groups per TFM

Then consume it for real. Point a nuget.config at the output directory, replace the demo apps'
ProjectReference with a PackageReference at a version that only exists locally (`-p:Version=
2.0.0-local`, so nuget.org's 2.0.0 cannot satisfy it), and build them. That is what catches a
missing native payload: it is a link or runtime failure, not a compile error.
