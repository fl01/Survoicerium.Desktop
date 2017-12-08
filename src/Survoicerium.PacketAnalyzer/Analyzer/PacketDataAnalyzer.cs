using System;
using System.Linq;
using System.Text.RegularExpressions;
using Survoicerium.Logging;

namespace Survoicerium.PacketAnalyzer.Analyzer
{
    /// <summary>
    /// TODO : rework this class, too much of hacks
    /// </summary>
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

        private string _gameServerIp = null;
        private int _team = -1;

        public EventHandler<JoinedGameArgs> OnJoinedGame;

        public PacketDataAnalyzer(ILogger logger, PacketAnalyzerOptions options)
        {
            _options = options;
            _logger = logger;
        }

        public void Analyze(IPPacket packet)
        {
            if (_options.FilterByIp.Any() && !_options.FilterByIp.Contains(packet.SourceAddress))
            {
                // another fucking hack
                if (_gameServerIp != null && packet.SourceAddress.ToString().StartsWith(_gameServerIp))
                {
                    if (packet.Data[28] == 0 && packet.Data[29] == 0 && packet.Data[30] == 64 && packet.Data[31] == 220)
                    {
                        _logger.Log(Severity.Info, $"Joined server: {packet.SourceAddress}:{packet.SourcePort} as team {_team}. Packet length {packet.Data.Length}. {packet.GetHexRepresentation()}");
                        OnJoinedGame?.Invoke(this, new JoinedGameArgs(packet.SourceAddress.ToString(), packet.SourcePort, _team));
                        _gameServerIp = null;
                        _team = -1;
                    }
                }

                return;
            }

            if (packet.Data.Length >= DefaultJoinIpPackageSize && packet.Data.Length <= 144)
            {
                if (packet.Data[JoinServerPacketIndex] != JoinServerPacketType)
                {
                    _logger.Log(Severity.Debug, $"Packet is in valid range, but join server flag '{packet.Data[JoinServerPacketIndex]}' is invalid. {packet.GetHexRepresentation()}");
                    return;
                }

                _logger.Log(Severity.Debug, $"Packet is of type 'join server'. {packet.GetHexRepresentation()}");

                // just a hack, nothing special
                _team = packet.Data.Length >= TeamIndex ? packet.Data[TeamIndex] : -1;
                if (_team == 0x00 || _team == 0x01)
                {
                    var assumedIpData = packet.Data.Skip(IndexOfIp).Take(MaxIpLength).ToArray();
                    var expectedIp = System.Text.ASCIIEncoding.ASCII.GetString(assumedIpData);
                    _gameServerIp = Regex.Match(expectedIp, @"([0-9]{1,3}(\.)?){3}", RegexOptions.Compiled).Value;
                    if (string.IsNullOrEmpty(_gameServerIp))
                    {
                        _logger.Log(Severity.Info, $"failed to parse ip from string '{expectedIp}'");
                    }
                    else
                    {
                        _logger.Log(Severity.Info, $"Game Server Ip: {_gameServerIp}*. Packet length {packet.Data.Length}");
                    }
                }
                else
                {
                    _logger.Log(Severity.Info, $"Unknown team id {_team}");
                }
            }
        }
    }
}
