using System.Reflection;

[assembly:XamlCompilation(XamlCompilationOptions.Compile)]

namespace WebRTCme.DemoApp.Maui
{
    public partial class App
    {
        public App()
        {
            InitializeComponent();

            MainPage = new AppShell();
        }

        /// <summary>
        /// Opens the desktop window landscape.
        /// </summary>
        /// <remarks>
        /// <para>Windows and Mac Catalyst have no orientation to lock - a window is whatever shape
        /// it was left - so the equivalent of "tablets are landscape" is to open wide. 1280x800
        /// fits three tiles across with room for the controls, and the user is free to resize
        /// afterwards: the tile grid follows the width.</para>
        /// <para>Only a starting size. Setting a minimum as well would fight the user over a
        /// window they are entitled to make any shape they like.</para>
        /// </remarks>
        protected override Window CreateWindow(IActivationState activationState)
        {
            var window = base.CreateWindow(activationState);

#if WINDOWS || MACCATALYST
            window.Width = 1280;
            window.Height = 800;
#endif

            return window;
        }

        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }

        protected override /*async*/ void CleanUp()
        {
            ////            WebRtcMiddleware.Cleanup();
            ///
            //await SignallingServerService.DisposeAsync();
            //WebRtcMiddleware.Dispose();

            base.CleanUp();
        }
    }
}
