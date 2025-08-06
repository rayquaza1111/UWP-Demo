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
        /// Handle settings button click with multiple navigation approaches
        /// </summary>
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
                
                bool navigationAttempted = false;
                
                // Approach 1: MainPage method
                try
                {
                    var rootFrame = Window.Current.Content as Frame;
                    if (rootFrame?.Content is MainPage mainPage)
                    {
                        System.Diagnostics.Debug.WriteLine("Approach 1: Using MainPage.NavigateToSettings()");
                        mainPage.NavigateToSettings();
                        navigationAttempted = true;
                        System.Diagnostics.Debug.WriteLine("Approach 1: SUCCESS");
                    }
                }
                catch (Exception ex1)
                {
                    System.Diagnostics.Debug.WriteLine($"Approach 1 failed: {ex1.Message}");
                }
                
                // Approach 2: Direct frame navigation
                if (!navigationAttempted)
                {
                    try
                    {
                        System.Diagnostics.Debug.WriteLine("Approach 2: Direct frame navigation");
                        var frame = Frame;
                        if (frame != null)
                        {
                            bool success = frame.Navigate(typeof(SettingsPage));
                            System.Diagnostics.Debug.WriteLine($"Approach 2: Result = {success}");
                            navigationAttempted = success;
                        }
                    }
                    catch (Exception ex2)
                    {
                        System.Diagnostics.Debug.WriteLine($"Approach 2 failed: {ex2.Message}");
                    }
                }
                
                // Approach 3: Root frame direct navigation
                if (!navigationAttempted)
                {
                    try
                    {
                        System.Diagnostics.Debug.WriteLine("Approach 3: Root frame direct navigation");
                        var rootFrame = Window.Current.Content as Frame;
                        if (rootFrame != null)
                        {
                            bool success = rootFrame.Navigate(typeof(SettingsPage));
                            System.Diagnostics.Debug.WriteLine($"Approach 3: Result = {success}");
                            navigationAttempted = success;
                        }
                    }
                    catch (Exception ex3)
                    {
                        System.Diagnostics.Debug.WriteLine($"Approach 3 failed: {ex3.Message}");
                    }
                }
                
                if (!navigationAttempted)
                {
                    System.Diagnostics.Debug.WriteLine("[ERROR] ALL NAVIGATION APPROACHES FAILED!");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("[SUCCESS] NAVIGATION ATTEMPTED SUCCESSFULLY!");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[CRITICAL] ERROR in SettingsButton_Click: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"[CRITICAL] Stack trace: {ex.StackTrace}");
            }
        }

        /// <summary>
        /// Handle customer edit navigation through MainPage's NavigationView system
        /// </summary>
        public void NavigateToEditCustomer(Customer customer)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"Navigating to edit customer: {customer?.FullName}");
                
                if (customer != null)
                {
                    // Store customer state for editing
                    var stateService = UWP_Demo.Services.NavigationStateService.Instance;
                    stateService.SetSelectedCustomerForEdit(customer);
                    
                    // Check if we're inside MainPage with NavigationView
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
                            
                            System.Diagnostics.Debug.WriteLine("Successfully navigated to EditPage through NavigationView");
                            return;
                        }
                    }
                    
                    // FALLBACK: Direct frame navigation if not in NavigationView
                    if (Frame != null)
                    {
                        Frame.Navigate(typeof(EditPage), customer);
                        System.Diagnostics.Debug.WriteLine("FALLBACK: Successfully navigated to EditPage via direct Frame");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to navigate to edit customer - {ex.Message}");
            }
        }

        /// <summary>
        /// Handle new customer navigation through MainPage's NavigationView system
        /// </summary>
        public void NavigateToNewCustomer()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("Navigating to create new customer");
                
                // Clear any existing state for fresh start
                var stateService = UWP_Demo.Services.NavigationStateService.Instance;
                stateService.ClearAllState();
                
                // Check if we're inside MainPage with NavigationView
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
                        
                        System.Diagnostics.Debug.WriteLine("Successfully navigated to new customer EditPage through NavigationView");
                        return;
                    }
                }
                
                // FALLBACK: Direct frame navigation if not in NavigationView
                if (Frame != null)
                {
                    Frame.Navigate(typeof(EditPage), null);
                    System.Diagnostics.Debug.WriteLine("FALLBACK: Successfully navigated to new customer EditPage via direct Frame");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to navigate to new customer - {ex.Message}");
            }
        }

        /// <summary>
        /// Navigate to customers page
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
                System.Diagnostics.Debug.WriteLine($"Failed to navigate to customers - {ex.Message}");
            }
        }

        /// <summary>
        /// Navigate to file operations page
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
                System.Diagnostics.Debug.WriteLine($"Failed to navigate to file operations - {ex.Message}");
            }
        }

        /// <summary>
        /// 6. Suspension & Resume Handling: Test suspension functionality
        /// </summary>
        private void TestSuspensionButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("=== MANUAL SUSPENSION TEST ===");
                
                // Get the ViewModel and test suspension
                if (DataContext is ViewModels.HomeViewModel viewModel)
                {
                    // First, debug current suspension state
                    viewModel.DebugSuspensionState();
                    
                    // Then test suspension and resume
                    viewModel.TestSuspensionAndResume();
                    
                    // Also test window minimization simulation
                    viewModel.TestWindowMinimization();
                    
                    System.Diagnostics.Debug.WriteLine("Manual suspension test completed - check welcome message");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("ERROR: Could not get HomeViewModel from DataContext");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ERROR in manual suspension test: {ex.Message}");
            }
        }

        /// <summary>
        /// 6. Suspension & Resume Handling: Handle welcome message dismissal
        /// </summary>
        private void WelcomeInfoBar_Closed(object sender, Microsoft.UI.Xaml.Controls.InfoBarClosedEventArgs e)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("6. Suspension & Resume Handling: Welcome InfoBar closed");
                
                // Let the ViewModel handle the dismissal
                if (DataContext is ViewModels.HomeViewModel viewModel)
                {
                    viewModel.DismissWelcomeMessage();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"6. Suspension & Resume Handling ERROR: Welcome InfoBar close error - {ex.Message}");
            }
        }

        /// <summary>
        /// 7. Error Simulation & Handling: Quick error test for demonstration
        /// </summary>
        private async void QuickErrorTest_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("?? QUICK ERROR TEST: Testing file not found error from HomePage");
                
                var errorService = UWP_Demo.Services.ErrorSimulationService.Instance;
                var result = await errorService.SimulateFileNotFoundErrorAsync();
                
                // Show result to user
                var dialog = new Windows.UI.Xaml.Controls.ContentDialog
                {
                    Title = "Error Test Result",
                    Content = $"Quick error test completed!\n\nResult: {result}\n\nCheck Visual Studio Debug Output for detailed error logs.\n\nFor more error tests, go to Settings page.",
                    CloseButtonText = "OK"
                };
                
                await dialog.ShowAsync();
                
                System.Diagnostics.Debug.WriteLine($"?? QUICK ERROR TEST: Result - {result}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"?? QUICK ERROR TEST ERROR: {ex.Message}");
                
                // Show error to user
                var dialog = new Windows.UI.Xaml.Controls.ContentDialog
                {
                    Title = "Error Test Error",
                    Content = $"An error occurred during the error test: {ex.Message}",
                    CloseButtonText = "OK"
                };
                
                await dialog.ShowAsync();
            }
        }
    }
}