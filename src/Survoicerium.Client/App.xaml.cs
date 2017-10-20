using System.Net;
using System.Windows;
using System.Windows.Threading;
using Survoicerium.Client.ViewModels;
using Survoicerium.Client.Views;

namespace Survoicerium.Client
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            var view = new MainView(new MainViewModel());
            view.ShowDialog();
        }

        public void Application_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            var comException = e.Exception as System.Runtime.InteropServices.COMException;

            if (comException != null && comException.ErrorCode == -2147221040)
            {
                e.Handled = true;
            }
        }
    }
}
