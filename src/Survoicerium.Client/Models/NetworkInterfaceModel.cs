using Survoicerium.PacketAnalyzer.Network;

namespace Survoicerium.Client.Models
{
    public class NetworkInterfaceModel
    {
        public NetworkInterfaceInfo Data { get; set; }

        public string Name { get { return $"{Data?.Name} - {Data?.IPAddress} - {Data?.Id}"; } }

        public NetworkInterfaceModel(NetworkInterfaceInfo info)
        {
            Data = info;
        }
    }
}
