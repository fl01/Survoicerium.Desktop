using System;
using System.Linq;
using System.Net;
using System.Security.Principal;
using System.Threading;
using Survoicerium.PacketAnalyzer.Analyzer;
using Survoicerium.PacketAnalyzer.Network;
using Survoicerium.PacketAnalyzer.Sniffing;

namespace Survoicerium.PacketAnalyzer
{
    public class EntryPoint
    {
        private static bool _isStopping;

        private static void Main(string[] args)
        {
            if (!IsElevated())
            {
                Console.WriteLine("Please run with elevated prilileges");
                Environment.Exit(1);
            }

            IPAddress survLoginIp = Dns.GetHostAddresses("game.survarium.com").FirstOrDefault();
            var nic = NetworkInterfaces.GetBestInterface(survLoginIp).FirstOrDefault();
            var options = new PacketAnalyzerOptions()
            {
                FilterByIp = new System.Collections.Generic.List<IPAddress>() { survLoginIp }
            };

            var sniffer = new SocketSniffer(nic, new PacketDataAnalyzer(options));
            sniffer.Start();

            Console.WriteLine();
            Console.WriteLine("Capturing on interface {0} ({1})", nic.Name, nic.IPAddress);
            Console.WriteLine("Press CTRL+C to stop");
            Console.WriteLine();

            // Shutdown gracefully on CTRL+C
            Console.CancelKeyPress += ConsoleOnCancelKeyPress;

            while (!_isStopping)
            {
                Thread.Sleep(200);
            }

            sniffer.Stop();
        }

        private static void ConsoleOnCancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            e.Cancel = true;
            _isStopping = true;
        }

        private static bool IsElevated()
        {
            return WindowsIdentity.GetCurrent().Owner.IsWellKnown(WellKnownSidType.BuiltinAdministratorsSid);
        }
    }
}
