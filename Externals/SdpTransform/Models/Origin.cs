namespace Utilme.SdpTransform
{
    public class Origin
    {
        public string UserName { get; set; }
        public const string DefaultUserName = "-";

        public ulong SessionId { get; set; }
        public uint SessionVersion { get; set; }
        public NetType NetType { get; set; }
        public string AddrType { get; set; }
        public string UnicastAddress { get; set; }
    }
}
