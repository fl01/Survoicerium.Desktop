using System.Net;

namespace Survoicerium.PacketAnalyzer.Network
{
    public class NetworkInterfaceInfo
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public IPAddress IPAddress { get; set; }
    }
}
