using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml;
using System;
using System.Linq;
using System.Threading.Tasks;

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
                System.Diagnostics.Debug.WriteLine("Navigation System: Back button clicked");
                
                // Navigation System: Simple navigation back
                if (Frame != null)
                {
                    if (Frame.CanGoBack)
                    {
                        Frame.GoBack();
                        System.Diagnostics.Debug.WriteLine("Navigation System: Navigated back via GoBack()");
                    }
                    else
                    {
                        Frame.Navigate(typeof(HomePage));
                        System.Diagnostics.Debug.WriteLine("Navigation System: Navigated back via Navigate(HomePage)");
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("Navigation System: Frame is null!");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Navigation System ERROR: Back navigation error: {ex.Message}");
            }
        }

        /// <summary>
        /// 6. Suspension & Resume Handling: Handle refresh suspension info button click
        /// Displays current suspension state information for testing
        /// Shows: Launch count, suspension times, welcome messages, app state
        /// Provides real-time debugging information for developers
        /// </summary>
        private void RefreshSuspensionInfo_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("SUSPENSION & RESUME: Refreshing suspension info for display");
                
                if (SuspensionStatus != null)
                {
                    var suspensionService = UWP_Demo.Services.SuspensionService.Instance;
                    var detailedInfo = suspensionService.GetDetailedDebugInfo();
                    var welcomeMessage = suspensionService.GetWelcomeBackMessage();
                    
                    var fullInfo = $"=== SUSPENSION DEBUG INFO ===\n\n" +
                                  $"{detailedInfo}\n\n" +
                                  $"Welcome Message:\n" +
                                  $"{(string.IsNullOrEmpty(welcomeMessage) ? "(No message - not suspended)" : welcomeMessage)}\n\n" +
                                  $"Last Updated: {DateTime.Now:HH:mm:ss}";
                    
                    SuspensionStatus.Text = fullInfo;
                    
                    System.Diagnostics.Debug.WriteLine($"SUSPENSION & RESUME: Updated UI with detailed suspension info");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SUSPENSION & RESUME ERROR: Failed to refresh suspension info - {ex.Message}");
                if (SuspensionStatus != null)
                {
                    SuspensionStatus.Text = $"Error loading suspension info: {ex.Message}";
                }
            }
        }

        /// <summary>
        /// 6. Suspension & Resume Handling: Test suspension message functionality
        /// Simulates app being suspended 15 seconds ago for testing welcome message
        /// Features: Auto-navigation to HomePage, immediate feedback, test state simulation
        /// Purpose: Allows easy testing without actual app suspension
        /// </summary>
        private void TestSuspensionMessage_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("SUSPENSION & RESUME: Testing suspension message functionality");
                
                var suspensionService = UWP_Demo.Services.SuspensionService.Instance;
                suspensionService.SetTestSuspensionState(15); // 15 seconds ago
                
                RefreshSuspensionInfo_Click(sender, e);
                
                if (StatusDisplay != null)
                {
                    StatusDisplay.Text = "Test suspension set! Navigating to Home page...";
                }
                
                try
                {
                    if (Frame != null)
                    {
                        Frame.Navigate(typeof(HomePage));
                        System.Diagnostics.Debug.WriteLine("SUSPENSION & RESUME: Auto-navigated to HomePage to show welcome message");
                    }
                }
                catch (Exception navEx)
                {
                    System.Diagnostics.Debug.WriteLine($"SUSPENSION & RESUME: Auto-navigation failed - {navEx.Message}");
                }
                
                System.Diagnostics.Debug.WriteLine("SUSPENSION & RESUME: Test suspension state set - welcome message should be visible on HomePage");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SUSPENSION & RESUME ERROR: Failed to set test suspension - {ex.Message}");
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

                // 6. Suspension & Resume Handling: Initialize suspension info display on page load
                RefreshSuspensionInfo_Click(sender, e);
                
                // STATE MANAGEMENT: Initialize state info display on page load
                RefreshStateInfo_Click(sender, e);
                
                // 7. Error Simulation & Handling: Initialize error log display
                Task.Run(async () =>
                {
                    await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
                    {
                        await RefreshErrorLogDisplay();
                    });
                });
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
                
                // Navigation System: Navigate to EditPage with test customer
                try
                {
                    if (Frame != null)
                    {
                        Frame.Navigate(typeof(UWP_Demo.Views.EditPage), testCustomer);
                        System.Diagnostics.Debug.WriteLine("Navigation System: Successfully navigated to EditPage with test customer");
                    }
                }
                catch (Exception navEx)
                {
                    System.Diagnostics.Debug.WriteLine($"Navigation System: Navigation failed - {navEx.Message}");
                    
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

        #region Error Simulation & Handling

        /// <summary>
        /// 7. Error Simulation & Handling: Test file not found error
        /// </summary>
        private async void SimulateFileNotFound_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("?? ERROR SIMULATION: Testing file not found error");
                
                var errorService = UWP_Demo.Services.ErrorSimulationService.Instance;
                var result = await errorService.SimulateFileNotFoundErrorAsync();
                
                // Update UI with result
                if (StatusDisplay != null)
                {
                    StatusDisplay.Text = "File Not Found test completed! Check error log above.";
                }
                
                // Refresh error log display
                await RefreshErrorLogDisplay();
                
                System.Diagnostics.Debug.WriteLine($"?? ERROR SIMULATION: File not found test result: {result}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"?? ERROR SIMULATION ERROR: Failed to test file not found - {ex.Message}");
                if (StatusDisplay != null)
                {
                    StatusDisplay.Text = $"Error during file not found test: {ex.Message}";
                }
            }
        }

        /// <summary>
        /// 7. Error Simulation & Handling: Test JSON parsing error
        /// </summary>
        private async void SimulateJsonError_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("?? ERROR SIMULATION: Testing JSON parsing error");
                
                var errorService = UWP_Demo.Services.ErrorSimulationService.Instance;
                var result = await errorService.SimulateJsonParsingErrorAsync();
                
                // Update UI with result
                if (StatusDisplay != null)
                {
                    StatusDisplay.Text = "JSON parsing test completed! Check error log above.";
                }
                
                // Refresh error log display
                await RefreshErrorLogDisplay();
                
                System.Diagnostics.Debug.WriteLine($"?? ERROR SIMULATION: JSON parsing test result: {result}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"?? ERROR SIMULATION ERROR: Failed to test JSON parsing - {ex.Message}");
                if (StatusDisplay != null)
                {
                    StatusDisplay.Text = $"Error during JSON parsing test: {ex.Message}";
                }
            }
        }

        /// <summary>
        /// 7. Error Simulation & Handling: Test access denied error
        /// </summary>
        private async void SimulateAccessDenied_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("?? ERROR SIMULATION: Testing access denied error");
                
                var errorService = UWP_Demo.Services.ErrorSimulationService.Instance;
                var result = await errorService.SimulateAccessDeniedErrorAsync();
                
                // Update UI with result
                if (StatusDisplay != null)
                {
                    StatusDisplay.Text = "Access denied test completed! Check error log above.";
                }
                
                // Refresh error log display
                await RefreshErrorLogDisplay();
                
                System.Diagnostics.Debug.WriteLine($"?? ERROR SIMULATION: Access denied test result: {result}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"?? ERROR SIMULATION ERROR: Failed to test access denied - {ex.Message}");
                if (StatusDisplay != null)
                {
                    StatusDisplay.Text = $"Error during access denied test: {ex.Message}";
                }
            }
        }

        /// <summary>
        /// 7. Error Simulation & Handling: Test corrupted data error
        /// </summary>
        private async void SimulateCorruptedData_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("?? ERROR SIMULATION: Testing corrupted data error");
                
                var errorService = UWP_Demo.Services.ErrorSimulationService.Instance;
                var result = await errorService.SimulateCorruptedFileErrorAsync();
                
                // Update UI with result
                if (StatusDisplay != null)
                {
                    StatusDisplay.Text = "Corrupted data test completed! Check error log above.";
                }
                
                // Refresh error log display
                await RefreshErrorLogDisplay();
                
                System.Diagnostics.Debug.WriteLine($"?? ERROR SIMULATION: Corrupted data test result: {result}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"?? ERROR SIMULATION ERROR: Failed to test corrupted data - {ex.Message}");
                if (StatusDisplay != null)
                {
                    StatusDisplay.Text = $"Error during corrupted data test: {ex.Message}";
                }
            }
        }

        /// <summary>
        /// 7. Error Simulation & Handling: Test network error
        /// </summary>
        private async void SimulateNetworkError_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("?? ERROR SIMULATION: Testing network error");
                
                var errorService = UWP_Demo.Services.ErrorSimulationService.Instance;
                var result = await errorService.SimulateNetworkFileErrorAsync();
                
                // Update UI with result
                if (StatusDisplay != null)
                {
                    StatusDisplay.Text = "Network error test completed! Check error log above.";
                }
                
                // Refresh error log display
                await RefreshErrorLogDisplay();
                
                System.Diagnostics.Debug.WriteLine($"?? ERROR SIMULATION: Network error test result: {result}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"?? ERROR SIMULATION ERROR: Failed to test network error - {ex.Message}");
                if (StatusDisplay != null)
                {
                    StatusDisplay.Text = $"Error during network error test: {ex.Message}";
                }
            }
        }

        /// <summary>
        /// 7. Error Simulation & Handling: Clear error log
        /// </summary>
        private async void ClearErrorLog_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("?? ERROR SIMULATION: Clearing error log");
                
                var errorService = UWP_Demo.Services.ErrorSimulationService.Instance;
                await errorService.ClearErrorLogAsync();
                
                // Update UI
                if (StatusDisplay != null)
                {
                    StatusDisplay.Text = "Error log cleared successfully!";
                }
                
                // Refresh error log display
                await RefreshErrorLogDisplay();
                
                System.Diagnostics.Debug.WriteLine("?? ERROR SIMULATION: Error log cleared successfully");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"?? ERROR SIMULATION ERROR: Failed to clear error log - {ex.Message}");
                if (StatusDisplay != null)
                {
                    StatusDisplay.Text = $"Error clearing error log: {ex.Message}";
                }
            }
        }

        /// <summary>
        /// 7. Error Simulation & Handling: Refresh error log display
        /// </summary>
        private async Task RefreshErrorLogDisplay()
        {
            try
            {
                var errorService = UWP_Demo.Services.ErrorSimulationService.Instance;
                var statistics = errorService.GetErrorStatistics();
                var sessionErrors = errorService.GetSessionErrors();
                
                var displayText = $"=== ERROR STATISTICS ===\n{statistics}\n\n";
                
                if (sessionErrors.Any())
                {
                    displayText += "=== RECENT ERRORS ===\n";
                    foreach (var error in sessionErrors.TakeLast(5))
                    {
                        displayText += $"[{error.Timestamp:HH:mm:ss}] {error.ErrorType}\n";
                        displayText += $"Operation: {error.Operation}\n";
                        displayText += $"Message: {error.Message}\n";
                        if (!string.IsNullOrEmpty(error.AdditionalInfo))
                        {
                            displayText += $"Info: {error.AdditionalInfo}\n";
                        }
                        displayText += "---\n";
                    }
                }
                else
                {
                    displayText += "No errors logged in this session.\n";
                }
                
                displayText += $"\nLog file saved as 'error_log.txt' in app local folder.\n";
                displayText += $"Last updated: {DateTime.Now:HH:mm:ss}";
                
                if (ErrorLogDisplay != null)
                {
                    ErrorLogDisplay.Text = displayText;
                }
            }
            catch (Exception ex)
            {
                if (ErrorLogDisplay != null)
                {
                    ErrorLogDisplay.Text = $"Error refreshing log display: {ex.Message}";
                }
            }
        }

        #endregion

        #region Simple File Access Examples

        /// <summary>
        /// Simple example: Try to open a missing file and catch the exception
        /// </summary>
        private async void SimpleFileTest_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("?? SIMPLE EXAMPLE: Testing file not found");
                
                var errorService = UWP_Demo.Services.ErrorSimulationService.Instance;
                var result = await errorService.TryOpenNonExistentFileAsync("my_missing_file.txt");
                
                // Update UI with result
                if (StatusDisplay != null)
                {
                    StatusDisplay.Text = "Simple file test completed! Check debug output.";
                }
                
                // Show result in a dialog
                var dialog = new Windows.UI.Xaml.Controls.ContentDialog
                {
                    Title = "Simple File Test Result",
                    Content = $"Result: {result}\n\nThis demonstrates basic exception handling when trying to open a file that doesn't exist.",
                    CloseButtonText = "OK"
                };
                
                await dialog.ShowAsync();
                
                // Refresh error log display
                await RefreshErrorLogDisplay();
                
                System.Diagnostics.Debug.WriteLine($"?? SIMPLE EXAMPLE: Result - {result}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"?? SIMPLE EXAMPLE ERROR: {ex.Message}");
                if (StatusDisplay != null)
                {
                    StatusDisplay.Text = $"Error during simple file test: {ex.Message}";
                }
            }
        }

        /// <summary>
        /// Example: Create a file, then delete it, then try to access it
        /// </summary>
        private async void CreateDeleteTest_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("?? CREATE/DELETE EXAMPLE: Testing file lifecycle");
                
                var errorService = UWP_Demo.Services.ErrorSimulationService.Instance;
                var result = await errorService.CreateAndDeleteFileExampleAsync();
                
                // Update UI with result
                if (StatusDisplay != null)
                {
                    StatusDisplay.Text = "Create/Delete test completed! Check debug output.";
                }
                
                // Show result in a dialog
                var dialog = new Windows.UI.Xaml.Controls.ContentDialog
                {
                    Title = "Create & Delete Test Result",
                    Content = $"Result: {result}\n\nThis demonstrates creating a file, confirming it exists, deleting it, then trying to access the deleted file.",
                    CloseButtonText = "OK"
                };
                
                await dialog.ShowAsync();
                
                // Refresh error log display
                await RefreshErrorLogDisplay();
                
                System.Diagnostics.Debug.WriteLine($"?? CREATE/DELETE EXAMPLE: Result - {result}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"?? CREATE/DELETE EXAMPLE ERROR: {ex.Message}");
                if (StatusDisplay != null)
                {
                    StatusDisplay.Text = $"Error during create/delete test: {ex.Message}";
                }
            }
        }

        /// <summary>
        /// Example: Check if file exists before trying to open it (defensive programming)
        /// </summary>
        private async void SafeAccessTest_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("?? SAFE ACCESS EXAMPLE: Testing defensive file access");
                
                var errorService = UWP_Demo.Services.ErrorSimulationService.Instance;
                var result = await errorService.SafeFileAccessExampleAsync("defensive_test_file.txt");
                
                // Update UI with result
                if (StatusDisplay != null)
                {
                    StatusDisplay.Text = "Safe access test completed! Check debug output.";
                }
                
                // Show result in a dialog
                var dialog = new Windows.UI.Xaml.Controls.ContentDialog
                {
                    Title = "Safe Access Test Result",
                    Content = $"Result: {result}\n\nThis demonstrates checking if a file exists before trying to open it, avoiding exceptions through defensive programming.",
                    CloseButtonText = "OK"
                };
                
                await dialog.ShowAsync();
                
                // Refresh error log display
                await RefreshErrorLogDisplay();
                
                System.Diagnostics.Debug.WriteLine($"?? SAFE ACCESS EXAMPLE: Result - {result}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"?? SAFE ACCESS EXAMPLE ERROR: {ex.Message}");
                if (StatusDisplay != null)
                {
                    StatusDisplay.Text = $"Error during safe access test: {ex.Message}";
                }
            }
        }

        #endregion
    }
}