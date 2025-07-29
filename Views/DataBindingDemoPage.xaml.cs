using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using UWPDemo.ViewModels;

namespace UWPDemo.Views
{
    /// <summary>
    /// Demonstrates data binding patterns and MVVM architecture.
    /// </summary>
    public sealed partial class DataBindingDemoPage : Page
    {
        public DataBindingDemoViewModel ViewModel { get; }

        public DataBindingDemoPage()
        {
            this.InitializeComponent();
            ViewModel = new DataBindingDemoViewModel();
        }

        private void NavigateBack(object sender, RoutedEventArgs e)
        {
            if (Frame.CanGoBack)
            {
                Frame.GoBack();
            }
        }

        private void DeleteItem(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is string item)
            {
                ViewModel.RemoveItem(item);
            }
        }

        private void AddItemButton_Click(object sender, RoutedEventArgs e)
        {
            string newItem = NewItemTextBox.Text;
            if (!string.IsNullOrWhiteSpace(newItem))
            {
                ViewModel.AddItemFromText(newItem);
                NewItemTextBox.Text = string.Empty;
            }
        }
    }
}