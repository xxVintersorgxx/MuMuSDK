using Avalonia.Controls;
using Avalonia.Interactivity;
using MuMuSDK.Desktop.ViewModels;

namespace MuMuSDK.Desktop.Views
{
    public partial class PluginManagerDialog : Window
    {
        public PluginManagerDialog()
        {
            InitializeComponent();
            DataContext = new PluginManagerViewModel();
        }

        private void CloseButton_Click(object? sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
