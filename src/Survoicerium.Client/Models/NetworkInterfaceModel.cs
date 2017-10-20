using Survoicerium.PacketAnalyzer.Network;

namespace Survoicerium.Client.Models
{
    public class NetworkInterfaceModel
    {
        public NetworkInterfaceInfo Data { get; set; }

        public string Name { get { return Data?.Name; } }

        public NetworkInterfaceModel(NetworkInterfaceInfo info)
        {
            Data = info;
        }
    }
}
