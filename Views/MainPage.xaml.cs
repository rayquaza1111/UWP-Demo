using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;

namespace UWPDemo.Views
{
    /// <summary>
    /// The main landing page for the UWP Demo application.
    /// Provides navigation to different demo sections.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        private void NavigateToControlsDemo(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(ControlsDemoPage));
        }

        private void NavigateToDataBindingDemo(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(DataBindingDemoPage));
        }

        private void NavigateToSettings(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(SettingsPage));
        }

        private async void ShowAboutDialog(object sender, RoutedEventArgs e)
        {
            ContentDialog aboutDialog = new ContentDialog
            {
                Title = "About UWP Demo",
                Content = "This is a comprehensive Universal Windows Platform (UWP) demo application showcasing modern Windows development practices.\n\n" +
                         "Features:\n" +
                         "• Modern XAML UI with Fluent Design\n" +
                         "• MVVM architecture\n" +
                         "• Data binding demonstrations\n" +
                         "• Various UWP controls showcase\n" +
                         "• Responsive design\n\n" +
                         "Built with .NET and WinUI 3",
                CloseButtonText = "OK",
                XamlRoot = this.XamlRoot
            };

            await aboutDialog.ShowAsync();
        }
    }
}