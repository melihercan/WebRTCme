# Diagnostics

Two of the smallest programs that can be written, kept because between them they answer a question
four rounds of guessing could not.

`Tests/WebRTCme.DeviceTests` runs in about two seconds on real Windows and never starts at all on a
`windows-latest` GitHub runner. It loads its assembly - a module initializer proves it - and then
produces nothing: no xUnit banner, no test, and nothing even when asked only to print its help.

That leaves two candidates, and the failing project differs from the passing unit tier in two ways
at once, so neither can be blamed from the evidence there:

- the Windows platform target framework and `win-x64` runtime identifier, or
- xUnit v3 / Microsoft.Testing.Platform on such a project.

These two probes separate them. Both target `net10.0-windows10.0.22621.0` with `win-x64`, exactly
as the failing project does, and neither references WebRTCme, a native payload, or anything else.

| `PlainConsole` | `MinimalXunit` | verdict |
| --- | --- | --- |
| hangs | - | the target framework or runtime identifier on that image; xUnit is innocent |
| runs | hangs | xUnit v3 on a Windows platform target framework |
| runs | runs | something this repository's test project adds on top - bisect from there |

Run by the `windows-diagnostics` job in `.github/workflows/ci.yml`, which is why they are committed
rather than written and thrown away: the next person to see this hang should find the experiment,
not have to invent it.
