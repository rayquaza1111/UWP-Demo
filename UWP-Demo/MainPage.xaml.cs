using System;
using System.Diagnostics;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Controls;
using UWP_Demo.Services;
using UWP_Demo.Helpers;
using UWP_Demo.Views;

namespace UWP_Demo
{
    /// <summary>
    /// The main page of the application that serves as the application shell.
    /// This page hosts the NavigationView and manages navigation between different sections
    /// of the application including Home, Edit, Settings, and other feature pages.
    /// </summary>
    /// <remarks>
    /// This page demonstrates several key UWP concepts:
    /// - NavigationView implementation for modern Windows app navigation
    /// - Frame-based navigation between pages
    /// - Theme management with real-time switching
    /// - Network status monitoring and display
    /// - Global error handling and user feedback
    /// - Responsive design adaptation
    /// - Integration with application services
    /// 
    /// The MainPage acts as the application shell and coordinates navigation
    /// while also providing global UI elements like theme toggles, status indicators,
    /// and error notifications that are available across all pages.
    /// </remarks>
    public sealed partial class MainPage : Page
    {
        #region Private Fields

        /// <summary>
        /// Reference to the settings service for theme management and preferences.
        /// </summary>
        private readonly SettingsService _settingsService;

        /// <summary>
        /// Reference to the network service for connectivity monitoring.
        /// </summary>
        private readonly NetworkService _networkService;

        /// <summary>
        /// Timer for periodic network status updates.
        /// This ensures the network status indicator stays current.
        /// </summary>
        private DispatcherTimer _networkStatusTimer;

        /// <summary>
        /// Flag to prevent recursive navigation calls during page setup.
        /// </summary>
        private bool _isNavigating = false;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the MainPage class.
        /// Sets up the navigation structure and initializes UI components.
        /// </summary>
        public MainPage()
        {
            this.InitializeComponent();
            
            // Initialize service references
            _settingsService = SettingsService.Instance;
            _networkService = NetworkService.Instance;
            
            // Set up the page
            InitializePage();
            
            Debug.WriteLine("MainPage: Initialized successfully");
        }

        #endregion

        #region Initialization

        /// <summary>
        /// Initializes the main page components and sets up event handlers.
        /// This method configures the UI state and starts monitoring services.
        /// </summary>
        private void InitializePage()
        {
            try
            {
                // Set up navigation frame
                SetupNavigationFrame();
                
                // Initialize UI state
                UpdateThemeToggleButton();
                UpdateNetworkStatus();
                
                // Set up periodic network status updates
                SetupNetworkStatusTimer();
                
                // Subscribe to service events
                SubscribeToServiceEvents();
                
                // Show welcome message if appropriate
                ShowWelcomeMessage();
                
                Debug.WriteLine("MainPage: Page initialization completed");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"MainPage: Error during initialization: {ex.Message}");
                ShowErrorMessage("Initialization Error", "There was a problem setting up the application. Some features may not work correctly.");
            }
        }

        /// <summary>
        /// Sets up the navigation frame and default navigation behavior.
        /// </summary>
        private void SetupNavigationFrame()
        {
            try
            {
                // Configure the content frame for navigation
                ContentFrame.NavigationFailed += ContentFrame_NavigationFailed;
                
                // Set up navigation helper if not already configured
                if (NavigationHelper.RootFrame == null)
                {
                    NavigationHelper.RootFrame = ContentFrame;
                    NavigationHelper.EnableNavigationEvents();
                }
                
                Debug.WriteLine("MainPage: Navigation frame setup completed");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"MainPage: Error setting up navigation frame: {ex.Message}");
            }
        }

        /// <summary>
        /// Sets up periodic network status monitoring.
        /// This ensures the network status indicator stays current.
        /// </summary>
        private void SetupNetworkStatusTimer()
        {
            try
            {
                _networkStatusTimer = new DispatcherTimer();
                _networkStatusTimer.Interval = TimeSpan.FromSeconds(30); // Check every 30 seconds
                _networkStatusTimer.Tick += NetworkStatusTimer_Tick;
                _networkStatusTimer.Start();
                
                Debug.WriteLine("MainPage: Network status timer started");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"MainPage: Error setting up network status timer: {ex.Message}");
            }
        }

        /// <summary>
        /// Subscribes to events from application services.
        /// This allows the main page to respond to service state changes.
        /// </summary>
        private void SubscribeToServiceEvents()
        {
            try
            {
                // Subscribe to theme changes
                _settingsService.ThemeChanged += SettingsService_ThemeChanged;
                
                // Subscribe to network operation events
                _networkService.NetworkOperationCompleted += NetworkService_OperationCompleted;
                
                Debug.WriteLine("MainPage: Service event subscriptions completed");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"MainPage: Error subscribing to service events: {ex.Message}");
            }
        }

        #endregion

        #region Page Lifecycle

        /// <summary>
        /// Called when the page is navigated to.
        /// This method handles page activation and restoration of state.
        /// </summary>
        /// <param name="e">Navigation event arguments</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            
            try
            {
                Debug.WriteLine("MainPage: Navigated to main page");
                
                // Navigate to the default page (Home) if no content is loaded
                if (ContentFrame.Content == null && !_isNavigating)
                {
                    NavigateToPage("home");
                }
                
                // Restore navigation selection if needed
                RestoreNavigationSelection();
                
                // Update UI state
                UpdateNetworkStatus();
                UpdateBadges();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"MainPage: Error in OnNavigatedTo: {ex.Message}");
            }
        }

        /// <summary>
        /// Called when the page is navigated away from.
        /// This method handles cleanup and state preservation.
        /// </summary>
        /// <param name="e">Navigation event arguments</param>
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            
            try
            {
                Debug.WriteLine("MainPage: Navigated away from main page");
                
                // Save current state if needed
                SaveNavigationState();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"MainPage: Error in OnNavigatedFrom: {ex.Message}");
            }
        }

        #endregion

        #region Navigation Event Handlers

        /// <summary>
        /// Handles navigation view selection changes.
        /// This method navigates to the appropriate page based on user selection.
        /// </summary>
        /// <param name="sender">The NavigationView that raised the event</param>
        /// <param name="args">Selection change event arguments</param>
        private void NavigationView_SelectionChanged(Microsoft.UI.Xaml.Controls.NavigationView sender, Microsoft.UI.Xaml.Controls.NavigationViewSelectionChangedEventArgs args)
        {
            try
            {
                if (_isNavigating) return;
                
                string selectedTag = null;
                
                // Handle settings selection
                if (args.IsSettingsSelected)
                {
                    selectedTag = "settings";
                }
                // Handle menu item selection
                else if (args.SelectedItem is Microsoft.UI.Xaml.Controls.NavigationViewItem selectedItem)
                {
                    selectedTag = selectedItem.Tag?.ToString();
                }
                
                if (!string.IsNullOrEmpty(selectedTag))
                {
                    NavigateToPage(selectedTag);
                    UpdatePageHeader(selectedTag);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"MainPage: Error in navigation selection: {ex.Message}");
                ShowErrorMessage("Navigation Error", "Unable to navigate to the selected page.");
            }
        }

        /// <summary>
        /// Handles back button requests from the NavigationView.
        /// This method implements back navigation behavior.
        /// </summary>
        /// <param name="sender">The NavigationView that raised the event</param>
        /// <param name="args">Back request event arguments</param>
        private void NavigationView_BackRequested(Microsoft.UI.Xaml.Controls.NavigationView sender, Microsoft.UI.Xaml.Controls.NavigationViewBackRequestedEventArgs args)
        {
            try
            {
                if (NavigationHelper.CanGoBack)
                {
                    NavigationHelper.GoBack();
                    UpdateNavigationSelection();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"MainPage: Error in back navigation: {ex.Message}");
            }
        }

        /// <summary>
        /// Handles navigation failures in the content frame.
        /// This method provides error handling for page navigation issues.
        /// </summary>
        /// <param name="sender">The Frame that failed to navigate</param>
        /// <param name="e">Navigation failure event arguments</param>
        private void ContentFrame_NavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            Debug.WriteLine($"MainPage: Content frame navigation failed to {e.SourcePageType?.Name}: {e.Exception.Message}");
            
            ShowErrorMessage("Page Load Error", 
                $"Unable to load the requested page. Please try again.\n\nError: {e.Exception.Message}");
            
            // Attempt recovery by navigating to home page
            try
            {
                NavigateToPage("home");
            }
            catch (Exception recoveryEx)
            {
                Debug.WriteLine($"MainPage: Recovery navigation also failed: {recoveryEx.Message}");
            }
        }

        #endregion

        #region Navigation Methods

        /// <summary>
        /// Navigates to the specified page based on the tag identifier.
        /// This method maps tag strings to actual page types and performs navigation.
        /// </summary>
        /// <param name="pageTag">The tag identifying which page to navigate to</param>
        private void NavigateToPage(string pageTag)
        {
            if (string.IsNullOrEmpty(pageTag) || _isNavigating)
                return;

            try
            {
                _isNavigating = true;
                
                Type pageType = GetPageTypeFromTag(pageTag);
                
                if (pageType != null)
                {
                    // Check if we're already on this page
                    if (ContentFrame.CurrentSourcePageType != pageType)
                    {
                        bool navigationResult = ContentFrame.Navigate(pageType);
                        Debug.WriteLine($"MainPage: Navigation to {pageType.Name}: {navigationResult}");
                        
                        if (navigationResult)
                        {
                            UpdatePageHeader(pageTag);
                        }
                    }
                }
                else
                {
                    Debug.WriteLine($"MainPage: Unknown page tag: {pageTag}");
                    ShowErrorMessage("Navigation Error", $"Unknown page: {pageTag}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"MainPage: Error navigating to page '{pageTag}': {ex.Message}");
                ShowErrorMessage("Navigation Error", $"Unable to navigate to {pageTag} page.");
            }
            finally
            {
                _isNavigating = false;
            }
        }

        /// <summary>
        /// Maps page tag strings to actual page types.
        /// This method centralizes the mapping between navigation identifiers and page classes.
        /// </summary>
        /// <param name="pageTag">The page tag to map</param>
        /// <returns>The Type of the page to navigate to, or null if not found</returns>
        private Type GetPageTypeFromTag(string pageTag)
        {
            return pageTag?.ToLowerInvariant() switch
            {
                "home" => typeof(HomePage),
                "edit" => typeof(EditPage),
                "settings" => typeof(SettingsPage),
                "network" => typeof(HomePage), // For now, could be a dedicated NetworkPage
                "about" => typeof(SettingsPage), // For now, could be a dedicated AboutPage
                _ => null
            };
        }

        /// <summary>
        /// Updates the page header content based on the current page.
        /// This method customizes the header text and subtitle for different pages.
        /// </summary>
        /// <param name="pageTag">The tag of the current page</param>
        private void UpdatePageHeader(string pageTag)
        {
            try
            {
                var (title, subtitle) = GetPageHeaderContent(pageTag);
                
                PageTitleTextBlock.Text = title;
                
                if (!string.IsNullOrEmpty(subtitle))
                {
                    PageSubtitleTextBlock.Text = subtitle;
                    PageSubtitleTextBlock.Visibility = Visibility.Visible;
                }
                else
                {
                    PageSubtitleTextBlock.Visibility = Visibility.Collapsed;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"MainPage: Error updating page header: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets the appropriate title and subtitle for a given page tag.
        /// </summary>
        /// <param name="pageTag">The page tag</param>
        /// <returns>A tuple containing the title and subtitle</returns>
        private (string title, string subtitle) GetPageHeaderContent(string pageTag)
        {
            return pageTag?.ToLowerInvariant() switch
            {
                "home" => ("Customer Management", "View and manage your customer database"),
                "edit" => ("Edit Customer", "Add or modify customer information"),
                "settings" => ("Settings", "Configure application preferences"),
                "network" => ("Network Demo", "API integration and network features"),
                "about" => ("About", "Application information and credits"),
                _ => ("UWP Demo Application", "Modern Windows development showcase")
            };
        }

        /// <summary>
        /// Restores the navigation selection to match the current page.
        /// This ensures the navigation menu reflects the actual current page.
        /// </summary>
        private void RestoreNavigationSelection()
        {
            try
            {
                var currentPageType = ContentFrame.CurrentSourcePageType;
                if (currentPageType == null) return;

                string pageTag = GetTagFromPageType(currentPageType);
                if (!string.IsNullOrEmpty(pageTag))
                {
                    UpdateNavigationSelection(pageTag);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"MainPage: Error restoring navigation selection: {ex.Message}");
            }
        }

        /// <summary>
        /// Maps page types back to their corresponding tags.
        /// This is the inverse of GetPageTypeFromTag.
        /// </summary>
        /// <param name="pageType">The page type to map</param>
        /// <returns>The corresponding tag, or null if not found</returns>
        private string GetTagFromPageType(Type pageType)
        {
            if (pageType == typeof(HomePage)) return "home";
            if (pageType == typeof(EditPage)) return "edit";
            if (pageType == typeof(SettingsPage)) return "settings";
            return null;
        }

        /// <summary>
        /// Updates the navigation view selection to match the specified tag.
        /// </summary>
        /// <param name="pageTag">The tag of the page to select</param>
        private void UpdateNavigationSelection(string pageTag = null)
        {
            try
            {
                if (pageTag == "settings")
                {
                    MainNavigationView.SelectedItem = null;
                    MainNavigationView.IsSettingsSelected = true;
                }
                else
                {
                    MainNavigationView.IsSettingsSelected = false;
                    
                    // Find and select the matching menu item
                    foreach (var item in MainNavigationView.MenuItems)
                    {
                        if (item is Microsoft.UI.Xaml.Controls.NavigationViewItem navItem &&
                            navItem.Tag?.ToString() == pageTag)
                        {
                            MainNavigationView.SelectedItem = navItem;
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"MainPage: Error updating navigation selection: {ex.Message}");
            }
        }

        /// <summary>
        /// Saves the current navigation state for restoration later.
        /// </summary>
        private void SaveNavigationState()
        {
            try
            {
                // Save current page selection
                var currentPageType = ContentFrame.CurrentSourcePageType;
                if (currentPageType != null)
                {
                    string pageTag = GetTagFromPageType(currentPageType);
                    if (!string.IsNullOrEmpty(pageTag))
                    {
                        _settingsService.UpdateSetting("LastPageTag", pageTag);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"MainPage: Error saving navigation state: {ex.Message}");
            }
        }

        #endregion

        #region UI Event Handlers

        /// <summary>
        /// Handles theme toggle button clicks.
        /// This method cycles through available themes when the user clicks the theme button.
        /// </summary>
        /// <param name="sender">The button that was clicked</param>
        /// <param name="e">Click event arguments</param>
        private void ThemeToggleButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Toggle the theme using the settings service
                _settingsService.ToggleTheme();
                
                Debug.WriteLine($"MainPage: Theme toggled to {_settingsService.CurrentTheme}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"MainPage: Error toggling theme: {ex.Message}");
                ShowErrorMessage("Theme Error", "Unable to change the application theme.");
            }
        }

        #endregion

        #region Service Event Handlers

        /// <summary>
        /// Handles theme change events from the settings service.
        /// This method updates the UI to reflect theme changes.
        /// </summary>
        /// <param name="sender">The settings service</param>
        /// <param name="newTheme">The new theme that was applied</param>
        private void SettingsService_ThemeChanged(object sender, ElementTheme newTheme)
        {
            try
            {
                // Update theme toggle button appearance
                UpdateThemeToggleButton();
                
                Debug.WriteLine($"MainPage: Responded to theme change: {newTheme}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"MainPage: Error handling theme change: {ex.Message}");
            }
        }

        /// <summary>
        /// Handles network operation completion events.
        /// This method can show progress indicators or status updates for network operations.
        /// </summary>
        /// <param name="sender">The network service</param>
        /// <param name="e">Network operation event arguments</param>
        private void NetworkService_OperationCompleted(object sender, NetworkOperationEventArgs e)
        {
            try
            {
                // Show brief feedback for network operations
                if (!e.Success && !string.IsNullOrEmpty(e.ErrorMessage))
                {
                    ShowInfoMessage("Network Operation", $"Operation '{e.OperationName}' failed: {e.ErrorMessage}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"MainPage: Error handling network operation event: {ex.Message}");
            }
        }

        /// <summary>
        /// Handles network status timer ticks.
        /// This method periodically updates the network status indicator.
        /// </summary>
        /// <param name="sender">The timer</param>
        /// <param name="e">Timer event arguments</param>
        private void NetworkStatusTimer_Tick(object sender, object e)
        {
            try
            {
                UpdateNetworkStatus();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"MainPage: Error in network status timer: {ex.Message}");
            }
        }

        #endregion

        #region UI Update Methods

        /// <summary>
        /// Updates the theme toggle button appearance based on the current theme.
        /// This method changes the button icon to reflect the current theme state.
        /// </summary>
        private void UpdateThemeToggleButton()
        {
            try
            {
                var currentTheme = _settingsService.CurrentTheme;
                
                // Update button content based on current theme
                string buttonContent = currentTheme switch
                {
                    ElementTheme.Dark => "☀️", // Sun icon for switching to light
                    ElementTheme.Light => "🌙", // Moon icon for switching to dark
                    ElementTheme.Default => "🔄", // Cycle icon for system default
                    _ => "🌙"
                };
                
                ThemeToggleButton.Content = buttonContent;
                
                // Update tooltip
                string tooltip = currentTheme switch
                {
                    ElementTheme.Dark => "Switch to Light theme",
                    ElementTheme.Light => "Switch to Dark theme",
                    ElementTheme.Default => "Switch to Light theme",
                    _ => "Toggle theme"
                };
                
                ToolTipService.SetToolTip(ThemeToggleButton, tooltip);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"MainPage: Error updating theme toggle button: {ex.Message}");
            }
        }

        /// <summary>
        /// Updates the network status indicator based on current connectivity.
        /// This method shows whether the device is online and the API is reachable.
        /// </summary>
        private async void UpdateNetworkStatus()
        {
            try
            {
                bool isConnected = _networkService.IsAvailable;
                
                if (isConnected)
                {
                    NetworkStatusBorder.Background = (Windows.UI.Xaml.Media.Brush)Application.Current.Resources["SuccessBrush"];
                    NetworkStatusText.Text = "Online";
                    
                    // Test API connectivity in background
                    _ = System.Threading.Tasks.Task.Run(async () =>
                    {
                        bool apiConnected = await _networkService.TestApiConnectivityAsync();
                        
                        await Dispatcher.BeginInvoke(() =>
                        {
                            if (!apiConnected)
                            {
                                NetworkStatusBorder.Background = (Windows.UI.Xaml.Media.Brush)Application.Current.Resources["WarningBrush"];
                                NetworkStatusText.Text = "Limited";
                            }
                        });
                    });
                }
                else
                {
                    NetworkStatusBorder.Background = (Windows.UI.Xaml.Media.Brush)Application.Current.Resources["ErrorBrush"];
                    NetworkStatusText.Text = "Offline";
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"MainPage: Error updating network status: {ex.Message}");
                
                // Show unknown status on error
                NetworkStatusBorder.Background = (Windows.UI.Xaml.Media.Brush)Application.Current.Resources["WarningBrush"];
                NetworkStatusText.Text = "Unknown";
            }
        }

        /// <summary>
        /// Updates notification badges on navigation items.
        /// This method can show counts or alerts on navigation menu items.
        /// </summary>
        private void UpdateBadges()
        {
            try
            {
                // Update home badge with customer count
                var dataService = DataService.Instance;
                if (dataService.IsDataLoaded)
                {
                    int customerCount = dataService.CustomerCount;
                    if (customerCount > 0)
                    {
                        HomeBadge.Value = customerCount;
                        HomeBadge.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        HomeBadge.Visibility = Visibility.Collapsed;
                    }
                }
                
                // Update edit badge if there are unsaved changes
                // This would be implemented based on edit page state
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"MainPage: Error updating badges: {ex.Message}");
            }
        }

        #endregion

        #region User Feedback Methods

        /// <summary>
        /// Shows an error message to the user using the global InfoBar.
        /// </summary>
        /// <param name="title">The error title</param>
        /// <param name="message">The error message</param>
        private void ShowErrorMessage(string title, string message)
        {
            try
            {
                GlobalInfoBar.Title = title;
                GlobalInfoBar.Message = message;
                GlobalInfoBar.Severity = InfoBarSeverity.Error;
                GlobalInfoBar.IsOpen = true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"MainPage: Error showing error message: {ex.Message}");
            }
        }

        /// <summary>
        /// Shows an informational message to the user using the global InfoBar.
        /// </summary>
        /// <param name="title">The message title</param>
        /// <param name="message">The message content</param>
        private void ShowInfoMessage(string title, string message)
        {
            try
            {
                GlobalInfoBar.Title = title;
                GlobalInfoBar.Message = message;
                GlobalInfoBar.Severity = InfoBarSeverity.Informational;
                GlobalInfoBar.IsOpen = true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"MainPage: Error showing info message: {ex.Message}");
            }
        }

        /// <summary>
        /// Shows a welcome message if appropriate based on app usage history.
        /// </summary>
        private void ShowWelcomeMessage()
        {
            try
            {
                string welcomeMessage = _settingsService.GetWelcomeMessage();
                
                if (_settingsService.CurrentSettings.IsFirstRun)
                {
                    ShowInfoMessage("Welcome!", welcomeMessage);
                }
                else if (_settingsService.CurrentSettings.TimeSinceLastLaunch.TotalHours > 24)
                {
                    ShowInfoMessage("Welcome Back!", welcomeMessage);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"MainPage: Error showing welcome message: {ex.Message}");
            }
        }

        #endregion

        #region Cleanup

        /// <summary>
        /// Performs cleanup when the page is being disposed.
        /// This method unsubscribes from events and stops timers to prevent memory leaks.
        /// </summary>
        ~MainPage()
        {
            try
            {
                // Stop network status timer
                _networkStatusTimer?.Stop();
                
                // Unsubscribe from service events
                if (_settingsService != null)
                {
                    _settingsService.ThemeChanged -= SettingsService_ThemeChanged;
                }
                
                if (_networkService != null)
                {
                    _networkService.NetworkOperationCompleted -= NetworkService_OperationCompleted;
                }
                
                Debug.WriteLine("MainPage: Cleanup completed");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"MainPage: Error during cleanup: {ex.Message}");
            }
        }

        #endregion
    }
}