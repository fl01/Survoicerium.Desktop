using System;
using System.Net;

namespace Survoicerium.PacketAnalyzer.Analyzer
{
    public class IPPacket
    {
        public int Version { get; private set; }
        public int HeaderLength { get; private set; }
        public int Protocol { get; private set; }
        public IPAddress SourceAddress { get; private set; }
        public IPAddress DestAddress { get; private set; }
        public int SourcePort { get; private set; }

        public int DestPort { get; private set; }

        public byte[] Data { get; private set; }

        public IPPacket(byte[] data)
        {
            var versionAndLength = data[0];
            Version = versionAndLength >> 4;

            // Only parse IPv4 packets for now
            if (this.Version == 4)
            {
                HeaderLength = (versionAndLength & 0x0F) << 2;

                Protocol = Convert.ToInt32(data[9]);
                SourceAddress = new IPAddress(BitConverter.ToUInt32(data, 12));
                DestAddress = new IPAddress(BitConverter.ToUInt32(data, 16));

                if (Enum.IsDefined(typeof(ProtocolsWithPort), this.Protocol))
                {
                    // TODO : fix crappy hack
                    var rawSourcePort = new byte[] { data[HeaderLength], data[HeaderLength + 1] };
                    var rawDestPort = new byte[] { data[HeaderLength + 2], data[HeaderLength + 3] };
                    if (BitConverter.IsLittleEndian)
                    {
                        Array.Reverse(rawSourcePort);
                        Array.Reverse(rawDestPort);
                        DestPort = BitConverter.ToUInt16(rawDestPort, 0);
                    }
                    else
                    {
                        DestPort = BitConverter.ToInt16(rawDestPort, 0);
                    }

                    SourcePort = BitConverter.ToInt16(rawSourcePort, 0);
                }

                Data = data;
            }
        }
    }
}
