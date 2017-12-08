using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using Survoicerium.ApiClient;
using Survoicerium.Client.Common;
using Survoicerium.Client.Configuration;
using Survoicerium.Client.Models;
using Survoicerium.Logging;
using Survoicerium.PacketAnalyzer.Analyzer;
using Survoicerium.PacketAnalyzer.Network;
using Survoicerium.PacketAnalyzer.Sniffing;

namespace Survoicerium.Client.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private readonly ILogger _logger;
        private readonly Api _api;
        private bool? _dialogResult;
        private bool isSniffing = false;
        private FixedSizeQueue<string> _logs = new FixedSizeQueue<string>(1000);
        private IPAddress _loginServerIp = null;
        private SynchronizationContext _context;
        private readonly IConfigurationService _configurationService;
        private SocketSniffer _sniffer = null;
        private PacketDataAnalyzer _packetDataAnalyzer = null;
        private ApiKeyStatus _apiKeyStatus;
        private string _apiKey = string.Empty;
        private bool _isVerifyInProgress = false;

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

        public ICommand GetApiKeyCommand { get; set; }

        public ICommand CopySelectedLogCommand { get; set; }

        public ICommand StartSnifferCommand { get; set; }

        public ICommand StopSnifferCommand { get; set; }

        public ICommand VerifyApiKeyCommand { get; set; }

        public ObservableCollection<NetworkInterfaceModel> Interfaces { get; set; } = new ObservableCollection<NetworkInterfaceModel>();

        public IEnumerable<string> Logs
        {
            get { return _logs.OrderByDescending(x => x); }
        }

        /// <summary>
        /// one way, doesn't require onpropertychanged
        /// </summary>
        public string SelectedLogRecord { get; set; }

        public string ApiKeyValue
        {
            get { return _apiKey; }
            set { Set(ref _apiKey, value); }
        }

        public NetworkInterfaceModel SelectedNetworkInterface { get; set; }

        public Severity MinLogLevel
        {
            get { return _logger.MinLogLevel; }
            set { _logger.MinLogLevel = value; }
        }

        public ApiKeyStatus ApiKeyStatus
        {
            get { return _apiKeyStatus; }
            set { Set(ref _apiKeyStatus, value); }
        }

        public MainViewModel(Api api, IConfigurationService configurationService, ILogger logger = null)
        {
            _context = SynchronizationContext.Current;
            _configurationService = configurationService;
            _logger = logger ?? new CallbackBasedLogger(LogToUI);
            _api = api;

            InitializeCommands();
            InitializeNetworkInteraces();
            InitializeSettings();

            VerifyApiKey();

            _logger.Log(Severity.Info, "Hello world");
        }

        private void InitializeSettings()
        {
            ApiKeyValue = _configurationService.ApiKey;
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
            GetApiKeyCommand = new RelayCommand(x => GetApiKey(), x => true);
            VerifyApiKeyCommand = new RelayCommand(x => VerifyApiKey(), x => !_isVerifyInProgress && !string.IsNullOrEmpty(ApiKeyValue));
        }

        private async void VerifyApiKey()
        {
            _isVerifyInProgress = true;
            try
            {
                bool isApiKeyValid = await _api.UseApiKey(ApiKeyValue).VerifyApiKeyAsync();
                if (isApiKeyValid)
                {
                    ApiKeyStatus = ApiKeyStatus.Valid;
                    _configurationService.ApiKey = ApiKeyValue;
                }
                else
                {
                    ApiKeyStatus = ApiKeyStatus.Invalid;
                }
            }
            finally
            {
                _isVerifyInProgress = false;
            }
        }

        private void GetApiKey()
        {
            // TODO : send hardware id
            _api.GetApiKey("new");
        }

        private void StopSniffing()
        {
            if (_packetDataAnalyzer != null)
            {
                _packetDataAnalyzer.OnJoinedGame -= OnJoinedGame;
            }

            _sniffer?.Stop();
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

            _logger.Log(Severity.Info, "Sniffer is being loaded...");
            _packetDataAnalyzer = new PacketDataAnalyzer(_logger, options);
            _packetDataAnalyzer.OnJoinedGame += OnJoinedGame;
            _sniffer = new SocketSniffer(SelectedNetworkInterface.Data, _logger, _packetDataAnalyzer);
            _sniffer.Start();
        }

        private void OnJoinedGame(object sender, JoinedGameArgs args)
        {

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
