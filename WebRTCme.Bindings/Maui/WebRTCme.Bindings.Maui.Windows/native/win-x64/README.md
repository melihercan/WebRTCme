# Native binaries

These come from the `webrtc-interop-windows-x64-*` artifact of the
**WebRtcNativeInteropWindows** workflow in
[WebRTCnative](https://github.com/melihercan/WebRTCnative/actions), the same way
`libwebrtc.aar` and `WebRTC.framework` are taken for Android and iOS.

Take only the files below, not the whole artifact. The workflow collects GN's
`runtime_deps`, which drags in the build machine's debugger tooling
(`dbghelp.dll`, `dbgcore.dll`, `msdia140.dll`, `symsrv.dll`) and an unreferenced
`atomic.dll`. None of those are imported by anything here — `dbghelp` is
referenced by abseil's symbolizer, and the copy in `System32` serves.

`WebRtcInterop.dll.lib` is for C++ consumers and is not needed for P/Invoke.
`WebRtcInterop.dll.pdb` is optional and large (~126 MB); take it only when
debugging a native crash. `webrtc.dll` is **not** used — the interop DLL absorbs
the WebRTC code it needs and does not depend on it.

## What belongs here

The set below is the import closure of `WebRtcInterop.dll`, read off the import
tables rather than guessed:

```
WebRtcInterop.dll                                         the shim itself
third_party_abseil-cpp_absl.dll                           component-build dependencies
third_party_boringssl.dll                                   of the shim; it is not
third_party_protobuf_protobuf_full_and_lite_library.dll     statically linked
libc++.dll                                                the clang C++ runtime
```

`libc++.dll` in turn imports Microsoft's `MSVCP140.dll`, and everything imports
`VCRUNTIME140.dll`, so the redistributables travel with them:

```
msvcp140.dll
msvcp140_atomic_wait.dll
vcruntime140.dll
vcruntime140_1.dll
```

Those four are here so the binding works on a machine without the Visual C++
runtime installed — app-local deployment, which the redistributable license
permits. They are the one part of this folder that is a deployment choice rather
than a fact: if the apps consuming this are packaged as MSIX with the VC++
runtime as a framework dependency, or the runtime is a stated prerequisite, they
can be deleted and nothing else changes.

The csproj copies `*.dll` from this folder to the output directory and the items
flow on to referencing projects, so refreshing the binaries needs no other edit.
