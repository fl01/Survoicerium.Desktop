using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using Survoicerium.Client.Common;
using Survoicerium.Client.Models;
using Survoicerium.Logging;
using Survoicerium.PacketAnalyzer.Analyzer;
using Survoicerium.PacketAnalyzer.Network;
using Survoicerium.PacketAnalyzer.Sniffing;

namespace Survoicerium.Client.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private bool? _dialogResult;
        private bool isSniffing = false;
        private Queue<string> _logs = new Queue<string>();
        private IPAddress _loginServerIp = null;
        private readonly ILogger _logger;
        private SynchronizationContext _context;
        private SocketSniffer _sniffer = null;

        public bool? DialogResult
        {
            get { return _dialogResult; }
            set
            {
                if (Set(ref _dialogResult, value))
                {
                    StopSniffing();
                }
            }
        }

        public ICommand ExitAppCommand { get; set; }

        public ICommand CopySelectedLogCommand { get; set; }

        public ICommand StartSnifferCommand { get; set; }

        public ICommand StopSnifferCommand { get; set; }

        public ObservableCollection<NetworkInterfaceModel> Interfaces { get; set; } = new ObservableCollection<NetworkInterfaceModel>();

        public IEnumerable<string> Logs
        {
            get { return _logs.OrderByDescending(x => x); }
        }

        /// <summary>
        /// one way, doesn't require onpropertychanged
        /// </summary>
        public string SelectedLogRecord { get; set; }

        public NetworkInterfaceModel SelectedNetworkInterface { get; set; }

        public MainViewModel(ILogger logger = null)
        {
            _context = SynchronizationContext.Current;
            _logger = logger ?? new CollectionLogger(LogToUI);

            InitializeCommands();
            InitializeNetworkInteraces();

            _logger.Log(Severity.Info, "Hello world");
        }

        private void InitializeNetworkInteraces()
        {
            // TODO : implement start/stop/change active interface
            _loginServerIp = Dns.GetHostAddresses("game.survarium.com").FirstOrDefault();
            var nic = NetworkInterfaces.GetBestInterface(_loginServerIp).FirstOrDefault();
            Interfaces.Add(new NetworkInterfaceModel(nic));
            SelectedNetworkInterface = Interfaces.FirstOrDefault();
        }

        private void InitializeCommands()
        {
            ExitAppCommand = new RelayCommand(x => DialogResult = true, x => true);
            StartSnifferCommand = new RelayCommand(x => StartSniffing(), x => !isSniffing);
            StopSnifferCommand = new RelayCommand(x => StopSniffing(), x => isSniffing);
            CopySelectedLogCommand = new RelayCommand(x => Clipboard.SetDataObject(SelectedLogRecord), x => true);
        }

        private void StopSniffing()
        {
            _sniffer.Stop();
            isSniffing = false;
            _logger.Log(Severity.Info, "Sniffing has been stopped");
        }

        private void StartSniffing()
        {
            isSniffing = true;

            var options = new PacketAnalyzerOptions()
            {
                FilterByIp = new List<IPAddress>() { _loginServerIp }
            };

            _logger.Log(Severity.Info, "Sniffing has been started");
            _sniffer = new SocketSniffer(SelectedNetworkInterface.Data, _logger, new PacketDataAnalyzer(_logger, options));
            _sniffer.Start();
        }

        private void LogToUI(string message)
        {
            _context.Send(x =>
            {
                _logs.Enqueue(message);
                OnPropertyChanged(nameof(Logs));
            }, null);
        }
    }
}
