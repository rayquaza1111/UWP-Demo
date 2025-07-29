using Windows.UI.Xaml.Controls;
using UWP_Demo.ViewModels;

namespace UWP_Demo.Views
{
    public sealed partial class SettingsPage : Page
    {
        private SettingsViewModel ViewModel => DataContext as SettingsViewModel;

        public SettingsPage()
        {
            this.InitializeComponent();
        }
    }
}