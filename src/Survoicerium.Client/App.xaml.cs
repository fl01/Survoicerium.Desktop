using System.Windows;
using Survoicerium.Client.ViewModels;
using Survoicerium.Client.Views;

namespace Survoicerium.Client
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var view = new MainView(new MainViewModel());
            view.ShowDialog();
        }
    }
}
