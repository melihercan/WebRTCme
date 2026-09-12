namespace WebRTCme.DemoApp.Maui;

/// <summary>
/// The glyphs from the shared icon vocabulary in doc/DemoAppRefactor.md.
/// </summary>
/// <remarks>
/// <para>Material Icons rather than the Material Symbols the plan first named. The point of the
/// choice was to share a vocabulary with the Blazor demo, and MudBlazor ships Material Icons - so
/// this is the set that actually matches, and it is a 350KB static font rather than a 3.5MB
/// variable one.</para>
/// <para>Every codepoint below was read from <c>MaterialIcons-Regular.codepoints</c>, the file
/// Google ships beside the font, rather than written from memory. A wrong value here is not a
/// compile error and not a crash - it is a blank box on a device, which is a slow thing to
/// diagnose and an easy thing to avoid.</para>
/// <para>Written as escapes rather than as the characters themselves. The glyphs live in the
/// private use area, so pasted literally they are invisible in an editor, indistinguishable from
/// each other in a diff, and one file-encoding accident away from being lost.</para>
/// </remarks>
public static class Icons
{
    /// <summary>The font family name registered in <c>MauiProgram</c>.</summary>
    public const string Font = "MaterialIcons";

    public const string Mic = "\ue029";
    public const string MicOff = "\ue02b";
    public const string Videocam = "\ue04b";
    public const string VideocamOff = "\ue04c";
    public const string ScreenShare = "\ue0e2";
    public const string StopScreenShare = "\ue0e3";
    public const string FiberManualRecord = "\ue061";
    public const string StopCircle = "\uef71";
    public const string CallEnd = "\ue0b1";
    public const string MoreVert = "\ue5d4";
    public const string SyncProblem = "\ue629";
    public const string Layers = "\ue53b";
    public const string Send = "\ue163";
    public const string Chat = "\ue0b7";
    public const string Tune = "\ue429";
    public const string Info = "\ue88e";
}
