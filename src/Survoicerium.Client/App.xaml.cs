using System;
using System.Linq;
using System.Windows;
using Survoicerium.ApiClient;
using Survoicerium.Client.Configuration;
using Survoicerium.Client.ViewModels;
using Survoicerium.Client.Views;
using Survoicerium.Logging;

namespace Survoicerium.Client
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            // TODO : replace with a proper command args parser
            var severity = e.Args.Contains("--debug", StringComparer.OrdinalIgnoreCase) ? Severity.Debug : Severity.Info;
            // TODO : read from config / etc
            MainViewModel vm = new MainViewModel(new Api("http://localhost:5001"), new ConfigurationService())
            {
                MinLogLevel = severity
            };
            base.OnStartup(e);
            var view = new MainView(vm);
            view.ShowDialog();
        }
    }
}
