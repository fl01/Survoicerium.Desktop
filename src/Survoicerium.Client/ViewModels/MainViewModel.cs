using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
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

        public bool? DialogResult
        {
            get { return _dialogResult; }
            set { Set(ref _dialogResult, value); }
        }

        public ICommand ExitAppCommand { get; set; }

        public ICommand StartSnifferCommand { get; set; }

        public ObservableCollection<NetworkInterfaceModel> Interfaces { get; set; } = new ObservableCollection<NetworkInterfaceModel>();

        public NetworkInterfaceModel SelectedNetworkInterface { get; set; }

        public MainViewModel()
        {
            InitializeCommands();

            InitializeNetworkInteraces();
        }

        private void InitializeNetworkInteraces()
        {
            IPAddress survLoginIp = Dns.GetHostAddresses("game.survarium.com").FirstOrDefault();
            var nic = NetworkInterfaces.GetBestInterface(survLoginIp).FirstOrDefault();
            Interfaces.Add(new NetworkInterfaceModel(nic));
            SelectedNetworkInterface = Interfaces.FirstOrDefault();
        }

        private void InitializeCommands()
        {
            ExitAppCommand = new RelayCommand(x => DialogResult = true, x => true);
            StartSnifferCommand = new RelayCommand(x => isSniffing = true, x => !isSniffing);
        }
    }
}
