using MudBlazor;

namespace WebRTCme.DemoApp.Blazor.Theme;

/// <summary>
/// The palette and metrics from doc/DemoAppRefactor.md, as a MudBlazor theme.
/// </summary>
/// <remarks>
/// <para>Dark only, and there is no light palette to switch to. Video reads better against a dark
/// surface, every product in this category has settled there, and a theme switcher is a control
/// the demo would have to explain without teaching anybody anything about WebRTC.</para>
/// <para>The same names and the same values appear in the MAUI demo's resource dictionary. They
/// are written twice because the two stacks have no way to share a file, which is exactly why the
/// values live in the document rather than only here.</para>
/// </remarks>
internal static class DemoTheme
{
    internal const string Surface = "#121316";
    internal const string SurfaceRaised = "#1C1E22";
    internal const string SurfaceSunken = "#2A2D34";
    internal const string Primary = "#4C8DFF";
    internal const string Danger = "#E5484D";
    internal const string Speaking = "#30A46C";
    internal const string Warning = "#F5A524";
    internal const string TextPrimary = "#F2F3F5";
    internal const string TextSecondary = "#A0A4AB";
    internal const string TextDisabled = "#6E7279";

    internal static readonly MudTheme Instance = new()
    {
        PaletteDark = new PaletteDark
        {
            Black = Surface,
            Background = Surface,
            Surface = SurfaceRaised,
            AppbarBackground = SurfaceRaised,
            AppbarText = TextPrimary,
            DrawerBackground = SurfaceRaised,
            DrawerText = TextPrimary,
            DrawerIcon = TextSecondary,

            Primary = Primary,
            // Nothing in the demo asks for a second accent; keep it legible rather than
            // near-invisible, since Color.Secondary resolves here and not to muted text.
            Secondary = TextSecondary,
            Success = Speaking,
            Warning = Warning,
            Error = Danger,
            Info = Primary,

            TextPrimary = TextPrimary,
            TextSecondary = TextSecondary,
            TextDisabled = TextDisabled,

            ActionDefault = TextSecondary,
            ActionDisabled = TextDisabled,
            Divider = SurfaceSunken,
            DividerLight = SurfaceSunken,
            LinesDefault = SurfaceSunken,
            TableLines = SurfaceSunken,
        },

        // The 4px scale from the plan. MudBlazor takes one radius; 12 is the tile, and the pill
        // shape the control buttons use comes from MudIconButton being round already.
        LayoutProperties = new LayoutProperties
        {
            DefaultBorderRadius = "12px",
            DrawerWidthLeft = "260px",
        },

        Typography = new Typography
        {
            Default = new DefaultTypography
            {
                FontFamily = ["Roboto", "Helvetica", "Arial", "sans-serif"],
            },
        },
    };
}
