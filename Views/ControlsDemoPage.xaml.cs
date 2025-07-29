using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using UWPDemo.ViewModels;

namespace UWPDemo.Views
{
    /// <summary>
    /// Demonstrates various UWP controls and their capabilities.
    /// </summary>
    public sealed partial class ControlsDemoPage : Page
    {
        public ControlsDemoViewModel ViewModel { get; }

        public ControlsDemoPage()
        {
            this.InitializeComponent();
            ViewModel = new ControlsDemoViewModel();
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