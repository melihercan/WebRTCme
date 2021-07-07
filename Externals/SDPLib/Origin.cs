namespace SDPLib
{
    public class Origin
    {
        public string UserName { get; set; }
        public ulong SessionId { get; set; }
        public ulong SessionVersion { get; set; }
        public string Nettype { get; set; }
        public string AddrType { get; set; }
        public string UnicastAddress { get; set; }
    }
}
