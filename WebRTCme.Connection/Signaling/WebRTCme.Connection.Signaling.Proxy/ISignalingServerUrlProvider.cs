namespace WebRTCme.Connection.Signaling.Proxy
{
    /// <summary>
    /// Where the signalling server is.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The address used to come straight out of <c>IConfiguration</c> in the stub's constructor,
    /// which is fine when it is known at build time and useless when it is not - a server chosen by
    /// the person using the app, one per tenant, one discovered at sign-in. Configuration can be
    /// rebuilt at runtime on some platforms and not others, which made this a platform quirk rather
    /// than a decision. Issue #9.
    /// </para>
    /// <para>
    /// Asked once, when the connection is first needed rather than when the stub is constructed, so
    /// an implementation is free to return nothing until the address is known. Register your own to
    /// replace the configuration-backed default:
    /// </para>
    /// <code>
    /// services.AddSignaling();
    /// services.AddSingleton&lt;ISignalingServerUrlProvider, MyUrlProvider&gt;();
    /// </code>
    /// </remarks>
    public interface ISignalingServerUrlProvider
    {
        /// <summary>
        /// The server's base URL, without the hub path - <c>https://example.com</c>, not
        /// <c>https://example.com/roomhub</c>. Null or empty means "not known yet", and the stub
        /// will say so rather than connecting to nowhere.
        /// </summary>
        string BaseUrl { get; }
    }
}
