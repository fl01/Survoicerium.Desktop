using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Survoicerium.Client.Common;
using Survoicerium.Client.Models;
using Survoicerium.PacketAnalyzer.Network;

namespace Survoicerium.Client.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private bool? _dialogResult;
        private bool isSniffing = false;
        private Queue<string> _logs = new Queue<string>();

        public bool? DialogResult
        {
            get { return _dialogResult; }
            set { Set(ref _dialogResult, value); }
        }

        public ICommand ExitAppCommand { get; set; }

        public ICommand CopySelectedLogCommand { get; set; }

        public ICommand StartSnifferCommand { get; set; }

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

        public SynchronizationContext Context { get; set; }

        public MainViewModel()
        {
            Context = SynchronizationContext.Current;

            InitializeCommands();

            InitializeNetworkInteraces();
        }

        private void InitializeNetworkInteraces()
        {
            // TODO : implement start/stop/change active interface
            IPAddress survLoginIp = Dns.GetHostAddresses("game.survarium.com").FirstOrDefault();
            var nic = NetworkInterfaces.GetBestInterface(survLoginIp).FirstOrDefault();
            Interfaces.Add(new NetworkInterfaceModel(nic));
            SelectedNetworkInterface = Interfaces.FirstOrDefault();

            Task.Factory.StartNew(() =>
            {
                for (int i = 0; i < 100; i++)
                {
                    Context.Send(x =>
                    {
                        _logs.Enqueue($"{DateTime.UtcNow} === {i}");
                        OnPropertyChanged(nameof(Logs));
                    }, null);
                    Thread.Sleep(1000);
                }
            });
        }

        private void InitializeCommands()
        {
            ExitAppCommand = new RelayCommand(x => DialogResult = true, x => true);
            StartSnifferCommand = new RelayCommand(x => isSniffing = true, x => !isSniffing);
            CopySelectedLogCommand = new RelayCommand(x => Clipboard.SetText(SelectedLogRecord), x => true);
        }
    }
}
