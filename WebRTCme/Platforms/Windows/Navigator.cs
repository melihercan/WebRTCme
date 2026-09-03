namespace WebRTCme.Windows
{
    internal class Navigator : INavigator
    {
        public static INavigator Create() => new Navigator();

        public IMediaDevices MediaDevices { get; } = new MediaDevices();

        public void Dispose() => MediaDevices.Dispose();
    }
}
