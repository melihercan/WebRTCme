Native WebRTC artifacts: where they come from
=============================================

This file used to name two WebRTC commits from January 2021, one for Android and one
for iOS. Both were wrong by 2026-09-12 and had been for a long time: the file itself
was last touched in October 2022, while the artifacts it claimed to describe were
rebuilt in September 2026. A version record that is quietly four years stale is worse
than none, because it gets believed - it was believed during an investigation on
2026-09-12 and sent it looking at the wrong tree.

So this file no longer records version numbers. It records where the answer lives,
because that cannot drift.


Where the artifacts come from
-----------------------------

All of them are built by the workflows in the WebRTCnative repository, one per
platform - WebRtcNativeAndroidLib, WebRtcNativeIosLib, WebRtcNativeMacCatalystLib,
WebRtcNativeWindows*, and so on.

None of them pins a WebRTC version here. Each run resolves one, through
.github/actions/resolve-webrtc-branch: given a branch number it uses that, and given
nothing it asks the Chromium dashboard for the newest milestone that has reached
stable and takes that milestone's WebRTC branch-head. Two consequences worth knowing:

  - "Which WebRTC is this?" is a property of the build that produced the artifact,
    not of this repository. The workflow answers it - the uploaded artifact is named
    webrtc-<platform>-m<milestone>-<branch>, so the milestone and branch-head are in
    the artifact name of the run the binary came from.

  - Two artifacts built on different days can be different WebRTC versions even from
    identical inputs, because "newest stable milestone" moves.


What is actually in this repository
-----------------------------------

Dates are when each artifact was committed here, which is the closest thing to a
version this repository honestly knows:

  Android          Jars/libwebrtc.aar                2026-09-08
  iOS              WebRTC.xcframework                2026-09-07
  Mac Catalyst     WebRTC.framework                  2026-09-09
  Windows          native/win-x64/ (WebRtcInterop)   see the Windows workflow

Find the corresponding workflow run in WebRTCnative for the exact branch-head.


One thing the source tree does not contain
------------------------------------------

Checked on 2026-09-12, against main and against branch-heads 8054 and 7977: WebRTC
has no Java simulcast classes at all. Simulcast upstream is entirely C++
(media/engine/simulcast_encoder_adapter, modules/video_coding/utility/
simulcast_rate_allocator). SimulcastVideoEncoderFactory - the Java factory that would
expose it to PeerConnectionFactory - is added by the LiveKit and webrtc-sdk forks and
is not something these builds can include by configuration.

Recorded here because the opposite was assumed first, and the assumption was only
expensive because nobody could look it up.


If you are about to trust a number in here again
------------------------------------------------

Don't add one by hand. If a pinned version becomes necessary, have the workflow write
the resolved branch into the artifact as it builds - it already knows it, since it
puts the milestone and branch in the artifact name. A version recorded by hand beside
a binary that CI replaces will go stale the first time somebody forgets, and nothing
will complain.
