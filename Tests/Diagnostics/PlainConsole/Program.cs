// Prints one line and leaves. If even this does not finish, the fault is below anything this
// repository wrote - the target framework, the runtime identifier, or the apphost on that machine.
//
// stderr as well as stdout, because that is the pair the tier 3 watchdog captures and prints when it
// has to kill a process, and the whole point here is to be readable when something does not finish.

Console.Out.WriteLine("PLAINCONSOLE | alive on " + Environment.OSVersion);
Console.Out.Flush();

Console.Error.WriteLine("PLAINCONSOLE | stderr works too");
Console.Error.Flush();

return 0;
