using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using UWPDemo.ViewModels;

namespace UWPDemo.Views
{
    /// <summary>
    /// Settings and application information page.
    /// </summary>
    public sealed partial class SettingsPage : Page
    {
        public SettingsViewModel ViewModel { get; }

        public SettingsPage()
        {
            this.InitializeComponent();
            ViewModel = new SettingsViewModel();
        }

        private void NavigateBack(object sender, RoutedEventArgs e)
        {
            if (Frame.CanGoBack)
            {
                Frame.GoBack();
            }
        }
    }
}