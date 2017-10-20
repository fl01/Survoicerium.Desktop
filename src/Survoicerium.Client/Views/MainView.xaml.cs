using System.Windows;
using Survoicerium.Client.ViewModels;

namespace Survoicerium.Client.Views
{
    /// <summary>
    /// Interaction logic for MainView.xaml
    /// </summary>
    public partial class MainView : Window
    {
        public MainView(MainViewModel vm)
        {
            DataContext = vm;
            InitializeComponent();
        }
    }
}
