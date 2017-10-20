using System;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using Survoicerium.Logging;

namespace Survoicerium.PacketAnalyzer.Analyzer
{
    public class PacketDataAnalyzer
    {
        private const int MaxIpLength = 4 * 4;
        private const int DefaultJoinIpPackageSize = 45;
        private const int IndexOfIp = 35;
        private const int JoinServerPacketIndex = 31;
        private const int TeamIndex = 54;
        private const int JoinServerPacketType = 130;

        private readonly PacketAnalyzerOptions _options;
        private readonly ILogger _logger;
        private readonly object _analyzePacket = new object();

        private string _gameServerIp = null;
        private int _team = -1;

        public PacketDataAnalyzer(ILogger logger, PacketAnalyzerOptions options)
        {
            _options = options;
            _logger = logger;
        }

        public void Analyze(IPPacket packet)
        {
            lock (_analyzePacket)
            {
                if (_options.FilterByIp.Any() && !_options.FilterByIp.Contains(packet.SourceAddress))
                {
                    // another fucking hack
                    if (_gameServerIp != null && packet.SourceAddress.ToString().StartsWith(_gameServerIp))
                    {
                        if (packet.Data[28] == 0 && packet.Data[29] == 0 && packet.Data[30] == 64 && packet.Data[31] == 220)
                        {
                            _logger.Log(Severity.Debug, $"Joined server: {packet.SourceAddress}:{packet.SourcePort} as team {_team}. Packet length {packet.Data.Length}. {packet.GetHexRepresentation()}");
                            _gameServerIp = null;
                            _team = -1;
                        }
                        else
                        {
                            _logger.Log(Severity.Debug, $"Packet doesn't fit 'join server criteria'. Packet length {packet.Data.Length}. {packet.GetHexRepresentation()}");
                        }
                    }

                    return;
                }

                //Console.WriteLine($"Received packet from {packet.SourceAddress}. Hex: {BitConverter.ToString(packet.Data).Replace("-", " ")}");
                if (packet.Data.Length >= DefaultJoinIpPackageSize && packet.Data.Length <= 144)
                {
                    if (packet.Data[JoinServerPacketIndex] != JoinServerPacketType)
                    {
                        _logger.Log(Severity.Debug, $"Packet is in valid range, but join server flag '{packet.Data[JoinServerPacketIndex]}' is invalid. {packet.GetHexRepresentation()}");
                        return;
                    }

                    _logger.Log(Severity.Debug, $"Packet is of type 'join server'. {packet.GetHexRepresentation()}");

                    _team = packet.Data[TeamIndex];
                    if (_team == 0x00 || _team == 0x01)
                    {
                        var assumedIpData = packet.Data.Skip(IndexOfIp).Take(MaxIpLength).ToArray();
                        var expectedIp = System.Text.ASCIIEncoding.ASCII.GetString(assumedIpData);
                        _gameServerIp = Regex.Match(expectedIp, @"([0-9]{1,3}(\.)?){3}", RegexOptions.Compiled).Value;
                        if (string.IsNullOrEmpty(_gameServerIp))
                        {
                            _logger.Log(Severity.Debug, $"failed to parse ip from string '{expectedIp}'");
                        }
                        else
                        {
                            _logger.Log(Severity.Debug, $"Game Server Ip: {_gameServerIp}*. Packet length {packet.Data.Length}");
                        }
                    }
                    else
                    {
                        _logger.Log(Severity.Debug, $"Unknown team id {_team}");
                    }
                }
            }
        }
    }
}
