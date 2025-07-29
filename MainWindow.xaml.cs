using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System;
using UWPDemo.Views;

namespace UWPDemo
{
    /// <summary>
    /// The main window that hosts the application content.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            this.InitializeComponent();
            this.Title = "UWP Demo Application";
            
            // Navigate to the main page
            MainFrame.Navigate(typeof(MainPage));
        }

        private void MainFrame_NavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception($"Failed to load Page {e.SourcePageType.Name}");
        }
    }
}