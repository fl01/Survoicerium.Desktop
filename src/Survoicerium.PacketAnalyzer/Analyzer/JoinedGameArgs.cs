namespace Survoicerium.PacketAnalyzer.Analyzer
{
    public class JoinedGameArgs
    {
        public string ServerAddress { get; set; }

        public int ServerPort { get; set; }

        public int TeamId { get; set; }

        public JoinedGameArgs(string server, int port, int team)
        {
            ServerAddress = server;
            ServerPort = port;
            TeamId = team;
        }
    }
}
