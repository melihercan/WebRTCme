using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WebRTCme;

namespace WebRTCme.Middleware
{
    public interface ILocalMediaStream
    {
        /// <summary>
        /// Raised when a capture device appears or disappears.
        /// </summary>
        /// <remarks>
        /// Forwarded from <see cref="IMediaDevices.OnDeviceChange"/>, which nothing in this
        /// repository could reach: the platform devices are held privately by the implementation,
        /// so an event declared on four platform classes had no route to a caller even once it
        /// was raised. This is that route.
        ///
        /// No payload, matching the web's <c>devicechange</c> - it says the set changed, not what
        /// changed. Ask <see cref="IMediaDevices.EnumerateDevices"/> if the answer matters.
        /// </remarks>
        event EventHandler OnDeviceChange;

        Task<IMediaStream> GetCameraMediaStreamAsync(CameraType cameraType = CameraType.Default,
            MediaStreamConstraints mediaStreamConstraints = null);

        Task<IMediaStream> GetDisplayMediaStreamAync(MediaStreamConstraints mediaStreamConstraints = null);
    }
}
