namespace WebRTCme.Middleware
{
    public static class MauiSupport
    {
        public static async Task SetCameraAndMicPermissionsAsync()
        {
            var camStatus = await Permissions.RequestAsync<Permissions.Camera>();
            if (camStatus != PermissionStatus.Granted)
            {
                throw new Exception("No Video permission was granted");
            }
            var micStatus = await Permissions.RequestAsync<Permissions.Microphone>();
            if (micStatus != PermissionStatus.Granted)
            {
                throw new Exception("No Mic permission was granted");
            }
        }

    }
}
