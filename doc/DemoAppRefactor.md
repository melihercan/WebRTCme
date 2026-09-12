# Refactoring the demo apps

A plan for modernising the two demo applications - Blazor and MAUI - agreed 2026-09-12. Written
before any code changed, so the decisions are visible and can be argued with rather than
reverse-engineered from a diff later.

The goal in one line: **both demos should look like siblings, built this decade, and neither should
lose a behaviour that was put there to fix something.**

## What the survey found

Three things shape everything below, and none of them was obvious from outside.

**The video tile is library code, not demo code.** `Media` lives in `WebRTCme.Middleware`:
`Blazor/Media.razor` is a `<video>` element and a label, `Maui/Media.cs` is a control with
per-platform handlers. So making the tiles look better means changing what every consumer of this
library gets, not just what these two demos show. That is a scope decision and it is the first
question below.

**The size and orientation variance is the cameras, not the apps.** Nothing locks orientation
anywhere - iOS allows portrait and both landscapes, Android handles the configuration change - and
tiles size to whatever the stream is. A webcam sends 640x480 and a phone sends 480x640, so a call
between them shows one landscape tile and one portrait tile. "Make them the same size" therefore
means choosing a fixed tile aspect and deciding what happens to video that does not match it. It
is not an orientation setting.

**`Media.razor` renders `<p4>@Label</p4>`**, which is not an HTML element. Browsers treat unknown
tags as inline and unstyled, so the tile label has never been styled at all. Presumably a typo for
`<h4>`. Small, real, and fixed as part of this work.

## Phase 0 - decisions

**Answered 2026-09-12.** All three went to the recommendation; the reasoning is kept below because
the alternatives were real and someone may want to revisit them.

### 1. Packages — DECIDED: all three

The repository rule is that no NuGet package is added without asking, so:

| | why |
| --- | --- |
| `MudBlazor` | The Blazor component library, as requested. Ships Material Icons, so no separate icon dependency. |
| `Syncfusion.Maui.Toolkit` | The MAUI component library, as requested - the free open-source toolkit. |
| **Material Symbols** as a `MauiFont` | For MAUI icons. A font file rather than a package, and the *same glyph set MudBlazor uses*, so the two apps share an icon vocabulary rather than each having its own. |

`BlazorPro.Spinkit` and `Blazored.Modal` are already referenced and MudBlazor has equivalents for
both. **Decided: keep them for now.** Dropping is tidier and can be done later as its own change;
replacing working code during a UI migration mixes two kinds of risk in one diff.

### 2. How far into the library does this reach? — DECIDED: minimally

In `WebRTCme.Middleware`:

- fix the `<p4>`;
- give the tile an aspect-ratio container so a grid of tiles is a grid rather than a ragged edge;
- add a muted / speaking indicator, since the data is already there and every other video app has
  one.

Anything beyond that - overlaid controls, hover behaviour, per-tile menus - belongs in the demo
apps. The library should render a stream well and stay out of the way.

### 3. What happens to video that does not fit the tile? — DECIDED: cover remote, contain self-view

Fixed-aspect tiles need a policy for a 480x640 stream in a 16:9 box:

- `cover` fills the tile and crops the edges - looks right in a grid, hides part of the picture;
- `contain` letterboxes - shows everything, leaves black bars.

**Cover for remote tiles, contain for the self-view.** Cropping someone else is a cosmetic
choice; cropping yourself hides what you are actually sending, which is the one thing a self-view
exists to tell you.

## Phase 1 - shared groundwork

One design language both apps follow, written down before either is touched: palette, spacing
scale, and an agreed icon per action - microphone on/off, camera on/off, share screen, record,
restart ICE, layer, hang up. Same glyph, same meaning, both platforms.

This is a page of documentation, not code. It exists because the two demos have already drifted
into looking like different products, and nothing stops that happening again except writing the
shared vocabulary down.

## Phase 2 - Blazor, with MudBlazor

`MudThemeProvider` and layout, then the four pages: `ConnectionParametersPage`, `CallPage`,
`ChatPage`, `AboutPage`. `MudSelect` and `MudTextField` for the join form, `MudIconButton` with
tooltips for the call controls, `MudGrid` for the tiles. Retire Bootstrap once nothing references
it - not before, because a half-migrated page inherits from both and looks worse than either.

## Phase 3 - MAUI, with Syncfusion

The same four pages. A `ResourceDictionary` for the theme, so styling stops being inline.

Icon buttons replace the horizontally scrolling row of text buttons. Worth being explicit about
why that row scrolls today: the text buttons do not fit on a 1080px phone - the third was clipped
and the fourth was off the edge entirely, which made them unreachable rather than merely ugly.
Icons remove the cause rather than working around it, and the scroll can go.

## Phase 4 - sizing and orientation

Fixed-aspect tiles, and a responsive count per row: one on a phone, two or three on a tablet or
desktop. Orientation stays unlocked, because a video call should work whichever way the device is
held; the layout adapts instead.

## Phase 5 - verification

At least one two-party call on each of the five platforms. This does double duty: as of
2026-09-12 there are five commits - the Android wrapper-identity fix, three `async void` guards,
the silent-catch logging and the device-recovery thread-safety fix - which compile everywhere and
have run nowhere.

## Two behaviours that must survive

Both are recorded in comments on the call pages, both were real bugs with measurements attached,
and both are exactly what a UI rewrite quietly reintroduces.

**The status row has a fixed height.** It used to collapse when there was nothing to report, which
was fine while it only reported mutes. Voice activity made it toggle with every phrase, and each
toggle resized the video below it - measured at 1080x860 while a peer was speaking and 1080x897 a
second later, back and forth for the whole call. A reserved row costs one line of blank space.

**Every control must be reachable.** See Phase 3. Whatever the new layout is, check it on a phone
before believing it.

Preserve the behaviour, not the markup.
