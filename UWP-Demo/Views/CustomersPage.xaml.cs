using Windows.UI.Xaml.Controls;
using UWP_Demo.Models;
using UWP_Demo.Services;
using System;

namespace UWP_Demo.Views
{
    /// <summary>
    /// ?? CUSTOMERS PAGE: Dedicated customer directory and management interface
    /// ?? Purpose: Pure customer browsing, searching, and management operations
    /// ?? Features: Enhanced customer list, advanced search, navigation support
    /// </summary>
    public sealed partial class CustomersPage : Page
    {
        public CustomersPage()
        {
            this.InitializeComponent();
            System.Diagnostics.Debug.WriteLine("?? CUSTOMERS PAGE: CustomersPage initialized with navigation support");
        }

        /// <summary>
        /// Navigation System: Handle direct Edit button click (fallback method)
        /// Navigation System: Called when XAML binding fails, provides direct navigation
        /// Navigation System: Uses MainPage's navigation system for consistency
        /// </summary>
        public void EditCustomer_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            try
            {
                // Get customer from button's DataContext
                var button = sender as Button;
                var customer = button?.DataContext as Customer;
                
                if (customer != null)
                {
                    System.Diagnostics.Debug.WriteLine($"Navigation System: CustomersPage Edit button clicked for {customer.FullName}");
                    
                    // Navigation System: Navigate using MainPage's method
                    NavigateToEditCustomer(customer);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Navigation System ERROR: Edit button click failed - {ex.Message}");
            }
        }

        /// <summary>
        /// Navigation System: Navigate to edit customer through MainPage
        /// Navigation System: Enhanced: Use MainPage's NavigationView system
        /// Navigation System: Stores state and navigates properly
        /// </summary>
        private void NavigateToEditCustomer(Customer customer)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"Navigation System: CustomersPage navigating to edit customer: {customer?.FullName}");
                
                if (customer != null)
                {
                    // ?? STATE MANAGEMENT: Store customer state for editing
                    var stateService = NavigationStateService.Instance;
                    stateService.SetSelectedCustomerForEdit(customer);
                    
                    // Navigation System: Navigate through MainPage's system
                    var currentFrame = Windows.UI.Xaml.Window.Current.Content as Windows.UI.Xaml.Controls.Frame;
                    if (currentFrame?.Content is MainPage mainPage)
                    {
                        mainPage.NavigateToEditWithCustomer(customer);
                        System.Diagnostics.Debug.WriteLine("Navigation System: Successfully used MainPage.NavigateToEditWithCustomer from CustomersPage");
                    }
                    else
                    {
                        // Navigation System: Fallback: Direct frame navigation
                        if (Frame != null)
                        {
                            Frame.Navigate(typeof(EditPage), customer);
                            System.Diagnostics.Debug.WriteLine("Navigation System: FALLBACK: Direct navigation to EditPage from CustomersPage");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Navigation System ERROR: Failed to navigate to edit customer from CustomersPage - {ex.Message}");
            }
        }
    }
}