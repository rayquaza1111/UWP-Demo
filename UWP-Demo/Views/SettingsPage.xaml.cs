using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml;
using System;

namespace UWP_Demo.Views
{
    public sealed partial class SettingsPage : Page
    {
        // THEME PERSISTENCE: Local variable to track current theme state
        private bool _isDarkTheme = false;

        public SettingsPage()
        {
            System.Diagnostics.Debug.WriteLine("=== SIMPLE SettingsPage: Starting constructor...");
            
            try
            {
                this.InitializeComponent();
                System.Diagnostics.Debug.WriteLine(">>> SIMPLE SettingsPage: InitializeComponent SUCCESS");
                
                // THEME PERSISTENCE: Initialize theme state from SettingsService - load saved theme
                try
                {
                    var settingsService = UWP_Demo.Services.SettingsService.Instance;
                    if (settingsService != null)
                    {
                        // THEME PERSISTENCE: Load saved theme state from storage
                        _isDarkTheme = settingsService.IsDarkTheme;
                        if (SimpleThemeToggle != null)
                        {
                            // THEME PERSISTENCE: Set toggle switch to match saved theme
                            SimpleThemeToggle.IsOn = _isDarkTheme;
                        }
                        
                        if (StatusDisplay != null)
                        {
                            // THEME PERSISTENCE: Display current loaded theme
                            StatusDisplay.Text = $"Theme: {settingsService.GetThemeName()}";
                        }
                        System.Diagnostics.Debug.WriteLine($">>> Initialized with theme: {settingsService.GetThemeName()}");
                    }
                    else
                    {
                        if (StatusDisplay != null)
                        {
                            StatusDisplay.Text = "Settings page loaded successfully!";
                        }
                    }
                }
                catch (Exception initEx)
                {
                    System.Diagnostics.Debug.WriteLine($">>> Init error: {initEx.Message}");
                    if (StatusDisplay != null)
                    {
                        StatusDisplay.Text = "Settings loaded (basic mode)";
                    }
                }
                
                System.Diagnostics.Debug.WriteLine(">>> SIMPLE SettingsPage: Constructor completed successfully!");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($">>> [ERROR] SIMPLE SettingsPage error: {ex.Message}");
                // Don't crash - continue with minimal functionality
            }
        }

        private void SimpleThemeToggle_Toggled(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine(">>> Simple theme toggle clicked");
                
                // THEME PERSISTENCE: Update local theme state from toggle
                _isDarkTheme = SimpleThemeToggle?.IsOn == true;
                
                // THEME PERSISTENCE: Apply theme using SettingsService and save to storage
                try
                {
                    var settingsService = UWP_Demo.Services.SettingsService.Instance;
                    if (settingsService != null)
                    {
                        // THEME PERSISTENCE: Save theme change to persistent storage
                        settingsService.IsDarkTheme = _isDarkTheme;
                        if (StatusDisplay != null)
                        {
                            // THEME PERSISTENCE: Update UI to show saved theme
                            StatusDisplay.Text = $"Theme: {settingsService.GetThemeName()}";
                        }
                        System.Diagnostics.Debug.WriteLine($">>> Theme applied: {settingsService.GetThemeName()}");
                    }
                    else
                    {
                        if (StatusDisplay != null)
                        {
                            StatusDisplay.Text = _isDarkTheme ? "Dark theme (local)" : "Light theme (local)";
                        }
                        System.Diagnostics.Debug.WriteLine($">>> Local theme: {(_isDarkTheme ? "Dark" : "Light")}");
                    }
                }
                catch (Exception serviceEx)
                {
                    System.Diagnostics.Debug.WriteLine($">>> Service error: {serviceEx.Message}");
                    if (StatusDisplay != null)
                    {
                        StatusDisplay.Text = "Theme service unavailable";
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($">>> Toggle error: {ex.Message}");
            }
        }

        private void QuickToggle_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine(">>> Quick toggle clicked");
                
                // THEME PERSISTENCE: Toggle the switch which will trigger save/load through SimpleThemeToggle_Toggled
                if (SimpleThemeToggle != null)
                {
                    SimpleThemeToggle.IsOn = !SimpleThemeToggle.IsOn;
                    // The Toggled event will handle the theme change and save
                    System.Diagnostics.Debug.WriteLine($">>> ToggleSwitch set to: {SimpleThemeToggle.IsOn}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($">>> Quick toggle error: {ex.Message}");
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine(">>> Back button clicked");
                
                // Simple navigation back
                if (Frame != null)
                {
                    if (Frame.CanGoBack)
                    {
                        Frame.GoBack();
                        System.Diagnostics.Debug.WriteLine(">>> Navigated back via GoBack()");
                    }
                    else
                    {
                        Frame.Navigate(typeof(HomePage));
                        System.Diagnostics.Debug.WriteLine(">>> Navigated back via Navigate(HomePage)");
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine(">>> Frame is null!");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($">>> Back navigation error: {ex.Message}");
            }
        }

        /// <summary>
        /// ?? SUSPENSION & RESUME: Handle refresh suspension info button click
        /// ?? Displays current suspension state information for testing
        /// ?? Shows: Launch count, suspension times, welcome messages, app state
        /// ? Provides real-time debugging information for developers
        /// </summary>
        private void RefreshSuspensionInfo_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("?? SUSPENSION & RESUME: Refreshing suspension info for display");
                
                // ?? SUSPENSION & RESUME: Get current suspension state information
                if (SuspensionStatus != null)
                {
                    var suspensionService = UWP_Demo.Services.SuspensionService.Instance;
                    var suspensionSummary = suspensionService.GetSuspensionSummary();
                    var welcomeMessage = suspensionService.GetWelcomeBackMessage();
                    
                    SuspensionStatus.Text = $"Suspension Summary:\n{suspensionSummary}\n\nWelcome Message:\n{welcomeMessage}";
                    
                    System.Diagnostics.Debug.WriteLine($"?? SUSPENSION & RESUME: Updated UI with suspension info");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"?? SUSPENSION & RESUME ERROR: Failed to refresh suspension info - {ex.Message}");
                if (SuspensionStatus != null)
                {
                    SuspensionStatus.Text = $"Error loading suspension info: {ex.Message}";
                }
            }
        }

        /// <summary>
        /// ?? SUSPENSION & RESUME: Test suspension message functionality
        /// ?? Simulates app being suspended 30 seconds ago for testing welcome message
        /// ? Features: Auto-navigation to HomePage, immediate feedback, test state simulation
        /// ?? Purpose: Allows easy testing without actual app suspension
        /// </summary>
        private void TestSuspensionMessage_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("?? SUSPENSION & RESUME: Testing suspension message functionality");
                
                // ?? SUSPENSION & RESUME: Set test suspension state (30 seconds ago)
                var suspensionService = UWP_Demo.Services.SuspensionService.Instance;
                suspensionService.SetTestSuspensionState(30); // 30 seconds ago
                
                // ?? SUSPENSION & RESUME: Update display with test state
                RefreshSuspensionInfo_Click(sender, e);
                
                // ? SUSPENSION & RESUME: Show confirmation and auto-navigate to see result
                if (StatusDisplay != null)
                {
                    StatusDisplay.Text = "Test suspension set! Navigating to Home page...";
                }
                
                // ?? SUSPENSION & RESUME: Auto-navigate to HomePage to show welcome message
                try
                {
                    if (Frame != null)
                    {
                        Frame.Navigate(typeof(HomePage));
                        System.Diagnostics.Debug.WriteLine("?? SUSPENSION & RESUME: Auto-navigated to HomePage to show welcome message");
                    }
                }
                catch (Exception navEx)
                {
                    System.Diagnostics.Debug.WriteLine($"?? SUSPENSION & RESUME: Auto-navigation failed - {navEx.Message}");
                }
                
                System.Diagnostics.Debug.WriteLine("?? SUSPENSION & RESUME: Test suspension state set - welcome message should be visible on HomePage");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"?? SUSPENSION & RESUME ERROR: Failed to set test suspension - {ex.Message}");
                if (StatusDisplay != null)
                {
                    StatusDisplay.Text = $"Error setting test suspension: {ex.Message}";
                }
            }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine(">>> SettingsPage: Page loaded");
                
                // Set initial state based on SettingsService
                try
                {
                    var settingsService = UWP_Demo.Services.SettingsService.Instance;
                    if (settingsService != null && SimpleThemeToggle != null)
                    {
                        _isDarkTheme = settingsService.IsDarkTheme;
                        SimpleThemeToggle.IsOn = _isDarkTheme;
                        
                        if (StatusDisplay != null)
                        {
                            StatusDisplay.Text = $"Theme: {settingsService.GetThemeName()}";
                        }
                        
                        System.Diagnostics.Debug.WriteLine($">>> Theme state loaded: {_isDarkTheme}");
                    }
                }
                catch (Exception serviceEx)
                {
                    System.Diagnostics.Debug.WriteLine($">>> Settings service error: {serviceEx.Message}");
                    if (StatusDisplay != null)
                    {
                        StatusDisplay.Text = "Settings service unavailable";
                    }
                }

                // ?? SUSPENSION & RESUME: Initialize suspension info display on page load
                // ?? Shows current suspension state when user navigates to Settings
                RefreshSuspensionInfo_Click(sender, e);
                
                // ?? STATE MANAGEMENT: Initialize state info display on page load
                RefreshStateInfo_Click(sender, e);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($">>> Page load error: {ex.Message}");
                // Don't crash - continue with minimal functionality
            }
        }

        #region State Management Testing

        /// <summary>
        /// ?? STATE MANAGEMENT: Refresh navigation state information display
        /// ?? Shows current state management status for testing
        /// </summary>
        private void RefreshStateInfo_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("?? STATE MANAGEMENT: Refreshing navigation state info for display");
                
                if (StateStatus != null)
                {
                    var stateService = UWP_Demo.Services.NavigationStateService.Instance;
                    var stateSummary = stateService.GetStateSummary();
                    var selectedCustomer = stateService.SelectedCustomer;
                    var formData = stateService.CurrentFormData;
                    
                    var detailedInfo = $"State Summary:\n{stateSummary}\n\n";
                    
                    if (selectedCustomer != null)
                    {
                        detailedInfo += $"Selected Customer:\n";
                        detailedInfo += $"  Name: {selectedCustomer.FullName}\n";
                        detailedInfo += $"  ID: {selectedCustomer.Id}\n";
                        detailedInfo += $"  Email: {selectedCustomer.Email}\n";
                        detailedInfo += $"  Created: {selectedCustomer.DateCreated:MM/dd/yyyy}\n\n";
                    }
                    else
                    {
                        detailedInfo += "Selected Customer: None\n\n";
                    }
                    
                    if (formData != null && !string.IsNullOrEmpty(formData.FirstName))
                    {
                        detailedInfo += $"Saved Form Data:\n";
                        detailedInfo += $"  Name: {formData.FirstName} {formData.LastName}\n";
                        detailedInfo += $"  Email: {formData.Email}\n";
                        detailedInfo += $"  Editing: {formData.IsEditing}\n";
                        detailedInfo += $"  Customer ID: {formData.CustomerId}\n";
                        detailedInfo += $"  Saved At: {formData.SavedAt:MM/dd/yyyy HH:mm:ss}\n\n";
                    }
                    else
                    {
                        detailedInfo += "Saved Form Data: None\n\n";
                    }
                    
                    detailedInfo += $"Has Unsaved Changes: {stateService.HasUnsavedChanges}";
                    
                    StateStatus.Text = detailedInfo;
                    
                    System.Diagnostics.Debug.WriteLine($"?? STATE MANAGEMENT: Updated UI with state info");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"?? STATE MANAGEMENT ERROR: Failed to refresh state info - {ex.Message}");
                if (StateStatus != null)
                {
                    StateStatus.Text = $"Error loading state info: {ex.Message}";
                }
            }
        }

        /// <summary>
        /// ?? STATE MANAGEMENT: Test navigation state by creating a sample customer and navigating
        /// ?? Creates test data and navigates to EditPage to demonstrate state management
        /// </summary>
        private void TestNavigationState_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("?? STATE MANAGEMENT: Testing navigation state functionality");
                
                // ?? STATE MANAGEMENT: Create a test customer for state management demonstration
                var testCustomer = new UWP_Demo.Models.Customer
                {
                    Id = 999,
                    FirstName = "Test",
                    LastName = "Customer",
                    Email = "test.customer@example.com",
                    Phone = "555-0123",
                    Company = "State Management Demo Corp",
                    DateCreated = DateTime.Now.AddDays(-30),
                    LastModified = DateTime.Now
                };
                
                // ?? STATE MANAGEMENT: Store test customer state
                var stateService = UWP_Demo.Services.NavigationStateService.Instance;
                stateService.SetSelectedCustomerForEdit(testCustomer);
                
                // ?? STATE MANAGEMENT: Navigate to EditPage with test customer
                try
                {
                    if (Frame != null)
                    {
                        Frame.Navigate(typeof(UWP_Demo.Views.EditPage), testCustomer);
                        System.Diagnostics.Debug.WriteLine("?? STATE MANAGEMENT: Successfully navigated to EditPage with test customer");
                    }
                }
                catch (Exception navEx)
                {
                    System.Diagnostics.Debug.WriteLine($"?? STATE MANAGEMENT: Navigation failed - {navEx.Message}");
                    
                    // Update status to show test completed
                    if (StatusDisplay != null)
                    {
                        StatusDisplay.Text = "State management test completed! Check state info above.";
                    }
                }
                
                // ?? STATE MANAGEMENT: Update state display
                RefreshStateInfo_Click(sender, e);
                
                System.Diagnostics.Debug.WriteLine("?? STATE MANAGEMENT: Navigation state test completed");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"?? STATE MANAGEMENT ERROR: Failed to test navigation state - {ex.Message}");
                if (StatusDisplay != null)
                {
                    StatusDisplay.Text = $"Error testing navigation state: {ex.Message}";
                }
            }
        }

        /// <summary>
        /// ?? STATE MANAGEMENT: Clear all navigation state for fresh start
        /// ?? Clears all stored state and refreshes display
        /// </summary>
        private void ClearNavigationState_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("?? STATE MANAGEMENT: Clearing all navigation state");
                
                // ?? STATE MANAGEMENT: Clear all stored state
                var stateService = UWP_Demo.Services.NavigationStateService.Instance;
                stateService.ClearAllState();
                
                // ?? STATE MANAGEMENT: Update displays
                RefreshStateInfo_Click(sender, e);
                
                if (StatusDisplay != null)
                {
                    StatusDisplay.Text = "All navigation state cleared successfully!";
                }
                
                System.Diagnostics.Debug.WriteLine("?? STATE MANAGEMENT: All navigation state cleared");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"?? STATE MANAGEMENT ERROR: Failed to clear navigation state - {ex.Message}");
                if (StatusDisplay != null)
                {
                    StatusDisplay.Text = $"Error clearing navigation state: {ex.Message}";
                }
            }
        }

        #endregion
    }
}