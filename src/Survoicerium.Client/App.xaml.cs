using System;
using System.Linq;
using System.Windows;
using Survoicerium.ApiClient;
using Survoicerium.ApiClient.Http;
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
            base.OnStartup(e);

            // TODO : replace with a proper command args parser
            var severity = e.Args.Contains("--debug", StringComparer.OrdinalIgnoreCase) ? Severity.Debug : Severity.Info;
            var cfg = new ConfigurationService();
            var api = new Api(cfg.GetBackendExternalAddress(), cfg.GetFrontendExternalAddress(), new RetriableHttpClient(TimeSpan.FromSeconds(5), 5));
            var vm = new MainViewModel(api, cfg)
            {
                MinLogLevel = severity
            };
            var view = new MainView(vm);
            view.ShowDialog();
        }
    }
}
