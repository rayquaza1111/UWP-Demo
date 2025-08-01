using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml;
using System;
using UWP_Demo.Models;  // ?? STATE MANAGEMENT: Import Customer model

namespace UWP_Demo.Views
{
    public sealed partial class HomePage : Page
    {
        public HomePage()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// ?? SUSPENSION & RESUME: Handle welcome InfoBar closed event
        /// ?? Dismisses the welcome message when user closes the InfoBar
        /// ?? Integrates with SuspensionService to clear suspension state
        /// ? Provides clean user experience with proper state management
        /// </summary>
        private void WelcomeInfoBar_Closed(Microsoft.UI.Xaml.Controls.InfoBar sender, Microsoft.UI.Xaml.Controls.InfoBarClosedEventArgs args)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("?? SUSPENSION & RESUME: Welcome InfoBar closed by user");
                
                // ?? SUSPENSION & RESUME: Dismiss welcome message through ViewModel
                if (DataContext is ViewModels.HomeViewModel viewModel)
                {
                    viewModel.DismissWelcomeMessage();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"?? SUSPENSION & RESUME ERROR: Failed to handle welcome close - {ex.Message}");
            }
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("=== HomePage: SETTINGS BUTTON CLICKED! ===");
                
                // Change button text to show it's working
                try
                {
                    var button = sender as Button;
                    if (button?.Content is StackPanel stackPanel)
                    {
                        var textBlock = stackPanel.Children[1] as TextBlock;
                        if (textBlock != null)
                        {
                            textBlock.Text = "Loading...";
                        }
                    }
                }
                catch { /* Ignore button text update errors */ }
                
                // Try multiple navigation approaches
                bool navigationAttempted = false;
                
                // Approach 1: MainPage method
                try
                {
                    var rootFrame = Window.Current.Content as Frame;
                    if (rootFrame?.Content is MainPage mainPage)
                    {
                        System.Diagnostics.Debug.WriteLine(">>> Approach 1: Using MainPage.NavigateToSettings()");
                        mainPage.NavigateToSettings();
                        navigationAttempted = true;
                        System.Diagnostics.Debug.WriteLine(">>> Approach 1: SUCCESS");
                    }
                }
                catch (Exception ex1)
                {
                    System.Diagnostics.Debug.WriteLine($">>> Approach 1 failed: {ex1.Message}");
                }
                
                // Approach 2: Direct frame navigation
                if (!navigationAttempted)
                {
                    try
                    {
                        System.Diagnostics.Debug.WriteLine(">>> Approach 2: Direct frame navigation");
                        var frame = Frame;
                        if (frame != null)
                        {
                            bool success = frame.Navigate(typeof(SettingsPage));
                            System.Diagnostics.Debug.WriteLine($">>> Approach 2: Result = {success}");
                            navigationAttempted = success;
                        }
                    }
                    catch (Exception ex2)
                    {
                        System.Diagnostics.Debug.WriteLine($">>> Approach 2 failed: {ex2.Message}");
                    }
                }
                
                // Approach 3: Root frame direct navigation
                if (!navigationAttempted)
                {
                    try
                    {
                        System.Diagnostics.Debug.WriteLine(">>> Approach 3: Root frame direct navigation");
                        var rootFrame = Window.Current.Content as Frame;
                        if (rootFrame != null)
                        {
                            bool success = rootFrame.Navigate(typeof(SettingsPage));
                            System.Diagnostics.Debug.WriteLine($">>> Approach 3: Result = {success}");
                            navigationAttempted = success;
                        }
                    }
                    catch (Exception ex3)
                    {
                        System.Diagnostics.Debug.WriteLine($">>> Approach 3 failed: {ex3.Message}");
                    }
                }
                
                if (!navigationAttempted)
                {
                    System.Diagnostics.Debug.WriteLine(">>> [ERROR] ALL NAVIGATION APPROACHES FAILED!");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine(">>> [SUCCESS] NAVIGATION ATTEMPTED SUCCESSFULLY!");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($">>> [CRITICAL] ERROR in SettingsButton_Click: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($">>> [CRITICAL] Stack trace: {ex.StackTrace}");
            }
        }

        /// <summary>
        /// ?? STATE MANAGEMENT: Handle customer edit navigation
        /// ?? Enhanced: Navigate through MainPage's NavigationView system
        /// ?? Called when user clicks Edit button on a customer
        /// </summary>
        public void NavigateToEditCustomer(Customer customer)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"?? STATE MANAGEMENT: Navigating to edit customer: {customer?.FullName}");
                
                if (customer != null)
                {
                    // ?? STATE MANAGEMENT: Store customer state for editing
                    var stateService = UWP_Demo.Services.NavigationStateService.Instance;
                    stateService.SetSelectedCustomerForEdit(customer);
                    
                    // ?? NAVIGATION: Check if we're inside MainPage with NavigationView
                    var currentFrame = Windows.UI.Xaml.Window.Current.Content as Windows.UI.Xaml.Controls.Frame;
                    if (currentFrame?.Content is MainPage mainPage)
                    {
                        // Navigate through MainPage's ContentFrame (NavigationView)
                        var contentFrame = mainPage.FindName("ContentFrame") as Windows.UI.Xaml.Controls.Frame;
                        if (contentFrame != null)
                        {
                            contentFrame.Navigate(typeof(EditPage), customer);
                            
                            // Update NavigationView selection to Edit page
                            var navView = mainPage.FindName("MainNavigationView") as Microsoft.UI.Xaml.Controls.NavigationView;
                            var editItem = mainPage.FindName("EditNavItem") as Microsoft.UI.Xaml.Controls.NavigationViewItem;
                            if (navView != null && editItem != null)
                            {
                                navView.SelectedItem = editItem;
                            }
                            
                            System.Diagnostics.Debug.WriteLine("?? NAVIGATION: Successfully navigated to EditPage through NavigationView");
                            return;
                        }
                    }
                    
                    // ?? FALLBACK: Direct frame navigation if not in NavigationView
                    if (Frame != null)
                    {
                        Frame.Navigate(typeof(EditPage), customer);
                        System.Diagnostics.Debug.WriteLine("?? FALLBACK: Successfully navigated to EditPage via direct Frame");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"?? STATE MANAGEMENT ERROR: Failed to navigate to edit customer - {ex.Message}");
            }
        }

        /// <summary>
        /// ?? STATE MANAGEMENT: Handle new customer navigation
        /// ?? Enhanced: Navigate through MainPage's NavigationView system
        /// ?? Called when user wants to create a new customer
        /// </summary>
        public void NavigateToNewCustomer()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("?? STATE MANAGEMENT: Navigating to create new customer");
                
                // ?? STATE MANAGEMENT: Clear any existing state for fresh start
                var stateService = UWP_Demo.Services.NavigationStateService.Instance;
                stateService.ClearAllState();
                
                // ?? NAVIGATION: Check if we're inside MainPage with NavigationView
                var currentFrame = Windows.UI.Xaml.Window.Current.Content as Windows.UI.Xaml.Controls.Frame;
                if (currentFrame?.Content is MainPage mainPage)
                {
                    // Navigate through MainPage's ContentFrame (NavigationView)
                    var contentFrame = mainPage.FindName("ContentFrame") as Windows.UI.Xaml.Controls.Frame;
                    if (contentFrame != null)
                    {
                        contentFrame.Navigate(typeof(EditPage), null);
                        
                        // Update NavigationView selection to Edit page
                        var navView = mainPage.FindName("MainNavigationView") as Microsoft.UI.Xaml.Controls.NavigationView;
                        var editItem = mainPage.FindName("EditNavItem") as Microsoft.UI.Xaml.Controls.NavigationViewItem;
                        if (navView != null && editItem != null)
                        {
                            navView.SelectedItem = editItem;
                        }
                        
                        System.Diagnostics.Debug.WriteLine("?? NAVIGATION: Successfully navigated to new customer EditPage through NavigationView");
                        return;
                    }
                }
                
                // ?? FALLBACK: Direct frame navigation if not in NavigationView
                if (Frame != null)
                {
                    Frame.Navigate(typeof(EditPage), null);
                    System.Diagnostics.Debug.WriteLine("?? FALLBACK: Successfully navigated to new customer EditPage via direct Frame");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"?? STATE MANAGEMENT ERROR: Failed to navigate to new customer - {ex.Message}");
            }
        }

        /// <summary>
        /// ?? DASHBOARD: Navigate to customers page
        /// </summary>
        private void ViewCustomersButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Navigate to customers page through MainPage
                var currentFrame = Windows.UI.Xaml.Window.Current.Content as Windows.UI.Xaml.Controls.Frame;
                if (currentFrame?.Content is MainPage mainPage)
                {
                    // Find and select the customers navigation item
                    var navView = mainPage.FindName("MainNavigationView") as Microsoft.UI.Xaml.Controls.NavigationView;
                    var customersItem = mainPage.FindName("CustomersNavItem") as Microsoft.UI.Xaml.Controls.NavigationViewItem;
                    if (navView != null && customersItem != null)
                    {
                        navView.SelectedItem = customersItem;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"?? DASHBOARD ERROR: Failed to navigate to customers - {ex.Message}");
            }
        }

        /// <summary>
        /// ?? DASHBOARD: Navigate to file operations page
        /// </summary>
        private void ManageFilesButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Navigate to file operations page through MainPage
                var currentFrame = Windows.UI.Xaml.Window.Current.Content as Windows.UI.Xaml.Controls.Frame;
                if (currentFrame?.Content is MainPage mainPage)
                {
                    // Find and select the file operations navigation item
                    var navView = mainPage.FindName("MainNavigationView") as Microsoft.UI.Xaml.Controls.NavigationView;
                    var fileOpsItem = mainPage.FindName("FileOpsNavItem") as Microsoft.UI.Xaml.Controls.NavigationViewItem;
                    if (navView != null && fileOpsItem != null)
                    {
                        navView.SelectedItem = fileOpsItem;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"?? DASHBOARD ERROR: Failed to navigate to file operations - {ex.Message}");
            }
        }
    }
}