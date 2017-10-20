using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace Survoicerium.PacketAnalyzer.Network
{
    public static class NetworkInterfaces
    {
        [DllImport("iphlpapi.dll", SetLastError = true)]
        private static extern int GetBestInterface(uint destAddr, out uint bestIfIndex);

        public static IList<NetworkInterfaceInfo> GetInterfaces()
        {
            return NetworkInterface.GetAllNetworkInterfaces().SelectMany(FromNativeInterface).ToList();
        }

        private static IList<NetworkInterfaceInfo> FromNativeInterface(NetworkInterface nic)
        {
            var nicInfos = new List<NetworkInterfaceInfo>();
            var ipAddresses = nic.GetIPProperties().UnicastAddresses.Where(x =>
                    x.Address != null && x.Address.AddressFamily == AddressFamily.InterNetwork);

            foreach (var ipAddress in ipAddresses)
            {
                var nicInfo = new NetworkInterfaceInfo
                {
                    Id = nic.Id,
                    Name = nic.Name,
                    IPAddress = ipAddress.Address
                };

                nicInfos.Add(nicInfo);
            }

            return nicInfos;
        }

        public static IList<NetworkInterfaceInfo> GetBestInterface(IPAddress address)
        {
            UInt32 bestInterfaceIndex;
            UInt32 ipv4AddressAsUInt32 = BitConverter.ToUInt32(address.GetAddressBytes(), 0);
            int result = GetBestInterface(ipv4AddressAsUInt32, out bestInterfaceIndex);
            if (result != 0)
            {
                throw new Win32Exception(result);
            }

            var nic = GetNetworkInterfaceByIndex(bestInterfaceIndex);

            return FromNativeInterface(nic);
        }

        private static NetworkInterface GetNetworkInterfaceByIndex(uint index)
        {
            // Search in all network interfaces that support IPv4.
            NetworkInterface ipv4Interface = (from thisInterface in NetworkInterface.GetAllNetworkInterfaces()
                                              where thisInterface.Supports(NetworkInterfaceComponent.IPv4)
                                              let ipv4Properties = thisInterface.GetIPProperties().GetIPv4Properties()
                                              where ipv4Properties != null && ipv4Properties.Index == index
                                              select thisInterface).SingleOrDefault();
            if (ipv4Interface != null)
            {
                return ipv4Interface;
            }

            // Search in all network interfaces that support IPv6.
            NetworkInterface ipv6Interface = (from thisInterface in NetworkInterface.GetAllNetworkInterfaces()
                                              where thisInterface.Supports(NetworkInterfaceComponent.IPv6)
                                              let ipv6Properties = thisInterface.GetIPProperties().GetIPv6Properties()
                                              where ipv6Properties != null && ipv6Properties.Index == index
                                              select thisInterface).SingleOrDefault();

            return ipv6Interface;
        }
    }
}
