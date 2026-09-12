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

One design language both apps follow, written down before either is touched. It exists because the
two demos have already drifted into looking like different products, and nothing stops that
happening again except writing the shared vocabulary down. This section is documentation; Phases 2
and 3 implement it.

### The control set - DECIDED: required actions on the bar, debugging behind a menu

The call page has six controls today and they are not six of a kind. Four are what anybody in a
video call expects to find; two exist to exercise this library and mean nothing to someone
evaluating it. Mixing them is what made the row too wide to fit a phone in the first place.

**On the bar, as icons:**

| action | why it is primary |
| --- | --- |
| microphone on / off | the control people reach for most, and the one they need fastest |
| camera on / off | same |
| share screen | the feature the demo is mostly there to show |
| record | it writes a file; a user has to be able to stop it |
| leave call | see the note below - there is currently no way to leave except the back gesture |

**Behind an overflow menu, labelled Debug:**

| action | why it is not primary |
| --- | --- |
| restart ICE | a recovery for a connection fault the library is supposed to handle by itself. Useful when testing that handling; meaningless otherwise. |
| send bottom layer only | a simulcast switch. Does nothing visible without simulcast, and the peer-to-peer path refuses it outright - see `OnToggleSpatialLayerAsync`. |

The menu is a single `more_vert` button at the end of the bar. Its items keep their text labels:
they are rare, they need explaining, and an icon for "restart ICE" would be a puzzle rather than a
shortcut.

Two things the survey turned up that this phase has to settle:

**MAUI has no record button.** Blazor has all six controls; MAUI has five, missing `Record`, though
`RecordButtonText` and `OnRecordAsync` are on the shared view model and MAUI has a
`MediaRecorderFileStreamFactory` of its own. The two demos should offer the same controls, so this
adds the button rather than removing the feature.

**Neither demo has a leave-call control.** `MediaStreamParameters.Hangup` is set to `false` at all
three construction sites in `CallViewModel` and never set to `true` anywhere, so the per-tile
hangup it feeds has never fired. Leaving a call means navigating back. A demo of a calling library
should have a visible way to end a call, so one is added - a red `call_end` button that does what
the back navigation already does, via `OnPageDisappearingAsync`. Flagged explicitly because it is
an addition rather than a port: if it is not wanted, it is one row to delete from the bar.

### Icons

MudBlazor ships Material Icons; MAUI gets Material Symbols as a `MauiFont`. The two sets share
glyph names, which is the whole point of choosing them - one name means one picture on both
platforms.

| action | state | glyph |
| --- | --- | --- |
| microphone | live | `mic` |
| | muted | `mic_off` |
| camera | live | `videocam` |
| | muted | `videocam_off` |
| share screen | idle | `screen_share` |
| | sharing | `stop_screen_share` |
| record | idle | `fiber_manual_record` |
| | recording | `stop_circle` |
| leave call | - | `call_end` |
| debug menu | - | `more_vert` |
| restart ICE | menu item | `sync_problem` |
| bottom layer only | menu item | `layers` |

Icon-only buttons need their names said out loud somewhere: a `MudTooltip` on Blazor and
`SemanticProperties.Description` on MAUI, both taking the existing `*ButtonText` strings from the
view model. Those strings already read as labels - "Mute microphone", "Start sharing screen" - so
nothing new has to be written and the states stay in one place.

The MAUI glyph codepoints come from the `.codepoints` file shipped beside the font when it is added
in Phase 3. They are deliberately not written here: a hex value copied from memory is the kind of
thing that renders a blank box and takes an hour to explain.

### Palette

Dark, and only dark. Video reads better against a dark surface, every product in this category has
settled there, and a theme switcher is a feature the demo does not need. The names below are what
the MudBlazor theme and the MAUI `ResourceDictionary` both call these colours, so a change is made
once and read in two places.

| name | value | used for |
| --- | --- | --- |
| `Surface` | `#121316` | page background |
| `SurfaceRaised` | `#1C1E22` | control bar, cards, the join form |
| `SurfaceSunken` | `#2A2D34` | tile background behind letterboxed video, dividers |
| `Primary` | `#4C8DFF` | join, and the active state of a toggle |
| `Danger` | `#E5484D` | leave call, and recording while it is running |
| `Speaking` | `#30A46C` | the speaking indicator on a tile |
| `Warning` | `#F5A524` | the muted indicator on a tile |
| `TextPrimary` | `#F2F3F5` | labels, values |
| `TextSecondary` | `#A0A4AB` | the peer media status line, hints |
| `TextDisabled` | `#6E7279` | controls that cannot act yet |

An enabled control is `TextPrimary` on `SurfaceRaised`; a toggle that is *off* - muted microphone,
camera off - is `Danger`, because the state worth noticing is the one where you are not sending
anything.

### Spacing and size

A 4px base, and nothing between the steps: `4, 8, 12, 16, 24, 32`. Inline padding, ad-hoc margins
and one-off `HeightRequest`s are what the current pages are made of, and they are why the two look
unrelated.

| | value | why |
| --- | --- | --- |
| icon button | 48x48 | above both minimum touch targets - 44 on iOS, 48 on Android |
| icon | 24 | Material's own metric |
| gap between controls | 8 | |
| control bar padding | 12 | |
| corner radius, controls | 24 | a pill, at that height |
| corner radius, tiles | 12 | |
| tile gap | 8 | |
| status row | one line, reserved | see "Two behaviours that must survive" |

Five controls at 48 plus a menu button, with 8 between them, is 344px. The narrowest target is a
1080px phone at roughly 360 device-independent pixels, so the bar fits without scrolling - which is
the measurement that justifies removing the `ScrollView` in Phase 3 rather than assuming it.

## Phase 2 - Blazor, with MudBlazor - DONE

`MudThemeProvider` and layout, then the four pages: `ConnectionParametersPage`, `CallPage`,
`ChatPage`, `AboutPage`. `MudSelect` and `MudTextField` for the join form, `MudIconButton` with
tooltips for the call controls, a CSS grid for the tiles. Bootstrap retired once nothing
referenced it - not before, because a half-migrated page inherits from both and looks worse than
either.

Verified in Chrome against the running app: all four pages, the debug menu, the leave button, and
the control bar at phone width - five controls plus the menu on one row with no scrolling, which
is the measurement Phase 1 used to justify dropping the MAUI `ScrollView` in Phase 3.

### What it turned up

Three faults, none of them introduced here:

- **`SelectedConnectionTypeName` was never initialised.** `JoinCall` matches it against each
  connection type name, so choosing nothing left `ConnectionParameters.ConnectionType` at its
  default. The old `<InputSelect>` showed the first option as though it were selected while the
  bound value was still null - and the default happened to be that same first option, so it
  always did the right thing for the wrong reason. `MudSelect` showed an empty box, which is the
  truth. Fixed by preselecting the first name.
- **The About image has never loaded.** It pointed at `_content/WebRTCme.DemoApp.Blazor/me.png`,
  which is the path for a file shipped by a *razor class library*; this one is in the app's own
  `wwwroot`.
- **`BlazorPro.Spinkit` and `Blazored.Modal` are both dead.** Spinkit has a `@using` and no
  component; `Blazored.Modal` has not even that. The middleware's popups are `BlazorDialog`.
  Phase 0 decided to keep them "for now" on the strength of my note that MudBlazor had
  equivalents for both - which implied they were in use. They are not, so the argument for
  keeping them (don't replace working code mid-migration) does not apply. Left referenced rather
  than removed quietly; **removing them is a one-line change whenever you want it.**

### Two things worth knowing before Phase 3

- **The peer-state properties are observable on purpose.** `IMediaStreamManager.Update` removes a
  tile and re-inserts it - it has to, because `BindableLayout` ignores a `Replace` - and every
  such round trip rebuilds the platform video renderer. Speaking toggles with every phrase, so
  the indicator cannot go through the manager. MAUI must follow the same rule.
- **Debug builds of this app are hard to load.** A Blazor WASM debug build fetches a `.pdb`
  beside every assembly, and on this machine one of the ~250 parallel requests reliably fails
  with a status-0 `TypeError: Failed to fetch` - a different asset each time, while `curl` serves
  every one of them. The Release build has no `.pdb` to fetch and loads first time. If the loading
  ring sticks at 98%, that is what it is, not the application.

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
