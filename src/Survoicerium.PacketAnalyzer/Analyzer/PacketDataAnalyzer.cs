using System;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;

namespace Survoicerium.PacketAnalyzer.Analyzer
{
    public class PacketDataAnalyzer
    {
        private readonly PacketAnalyzerOptions _options;
        private const int MaxIpLength = 4 * 4;
        private const int DefaultJoinIpPackageSize = 64;
        private const int IndexOfIp = 35;
        private const int JoinServerPacketIndex = 31;
        private const int TeamIndex = 54;
        private const int JoinServerPacketType = 130;

        private IPAddress GameServerIp = null;
        private int team = -1;
        public PacketDataAnalyzer(PacketAnalyzerOptions options)
        {
            _options = options;
        }

        public void Analyze(IPPacket packet)
        {
            if (_options.FilterByIp.Any() && !_options.FilterByIp.Contains(packet.SourceAddress))
            {
                if (GameServerIp != null && GameServerIp.Equals(packet.SourceAddress))
                {
                    Console.WriteLine($"Joined server: {packet.SourceAddress}:{packet.SourcePort} as team {team}");
                    GameServerIp = null;
                    team = -1;
                }

                return;
            }

            //Console.WriteLine($"Received packet from {packet.SourceAddress}. Hex: {BitConverter.ToString(packet.Data).Replace("-", " ")}");
            if (packet.Data.Length >= DefaultJoinIpPackageSize && packet.Data.Length <= 144 && packet.Data[JoinServerPacketIndex] == JoinServerPacketType)
            {
                team = packet.Data[TeamIndex];
                Console.WriteLine($"Team: {team}");
                if (team == 0x00 || team == 0x01)
                {
                    var assumedIpData = packet.Data.Skip(IndexOfIp).Take(MaxIpLength).ToArray();
                    var expectedIp = System.Text.ASCIIEncoding.ASCII.GetString(assumedIpData);
                    string ip = Regex.Match(expectedIp, @"([0-9]{1,3}(\.)?){4}", RegexOptions.Compiled).Value;
                    Console.WriteLine(ip);
                    if (string.IsNullOrEmpty(ip) || !IPAddress.TryParse(ip, out IPAddress address))
                    {
                        Console.WriteLine($"failed to parse ip from string '{expectedIp}'");
                    }
                    else
                    {
                        GameServerIp = address;
                    }
                }
                else
                {
                    Console.WriteLine($"Unknown team id {team}");
                }
            }
        }
    }
}
