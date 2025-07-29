using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using UWP_Demo.ViewModels;
using UWP_Demo.Models;
using UWP_Demo.Helpers;

namespace UWP_Demo.Views
{
    public sealed partial class EditPage : Page
    {
        private EditViewModel ViewModel => DataContext as EditViewModel;

        public EditPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            
            // Initialize the ViewModel with the customer parameter
            var customer = NavigationHelper.GetNavigationParameter<Customer>(typeof(EditPage));
            ViewModel?.Initialize(customer);
        }
    }
}