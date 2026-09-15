using ObjCRuntime;
using UIKit;

namespace WebRTCme.DeviceTests.Runner;

public class Program
{
    /// <summary>Environment variable carrying the scenario filter. See <see cref="Main"/>.</summary>
    const string FilterVariable = "WEBRTCME_FILTER";

    // The filter arrives by environment variable first, command line second - the Apple equivalent
    // of the intent extra the Android runner takes.
    //
    // The environment comes first because the command line does not survive the trip to a phone.
    // devicectl documents trailing <command-line-arguments>, and they simply never reach Main:
    // passed plainly or after a -- separator, the app still reported all five scenarios instead of
    // the one asked for. That failure is silent and it matters, because an unfiltered "isolated" run
    // is the full run again - and the device-enumeration scenario means nothing in a process where a
    // call has already happened, which is the whole reason the harness runs it twice.
    //
    // devicectl forwards any variable prefixed DEVICECTL_CHILD_, so the harness sets
    // DEVICECTL_CHILD_WEBRTCME_FILTER and the app sees WEBRTCME_FILTER. Mac Catalyst is launched
    // directly and can use either; the command-line form is kept because it works there and is
    // easier to type by hand.
    static void Main(string[] args)
    {
        App.Filter = Environment.GetEnvironmentVariable(FilterVariable) is { Length: > 0 } fromEnvironment
            ? fromEnvironment
            : args.FirstOrDefault(a => a.StartsWith("--filter="))?["--filter=".Length..];
        UIApplication.Main(args, null, typeof(AppDelegate));
    }
}
