using System;
using System.Diagnostics;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using UWP_Demo.Services;
using UWP_Demo.Helpers;

namespace UWP_Demo
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// This class handles application lifecycle events, initialization, and global error handling.
    /// It demonstrates proper UWP application lifecycle management and service initialization.
    /// </summary>
    /// <remarks>
    /// This class demonstrates several important UWP application concepts:
    /// - Application lifecycle management (Launch, Suspend, Resume)
    /// - Service initialization and dependency injection
    /// - Navigation framework setup
    /// - Theme management and settings persistence
    /// - Global error handling and logging
    /// - State preservation during suspension
    /// - Proper resource cleanup
    /// 
    /// The App class serves as the entry point for the application and coordinates
    /// the initialization of all major systems including data services, settings,
    /// and the navigation framework.
    /// </remarks>
    sealed partial class App : Application
    {
        #region Private Fields

        /// <summary>
        /// Stores the timestamp when the application was suspended.
        /// This is used to calculate downtime and show appropriate welcome messages.
        /// </summary>
        private DateTime _suspensionTime;

        /// <summary>
        /// Flag indicating whether the application has been initialized.
        /// Prevents duplicate initialization during activation events.
        /// </summary>
        private bool _isInitialized = false;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the Application class.
        /// This is the singleton application object and the first user code to run.
        /// </summary>
        /// <remarks>
        /// The constructor sets up global event handlers for unhandled exceptions
        /// and prepares the application for initialization. Most initialization
        /// work is deferred to the OnLaunched method to improve startup performance.
        /// </remarks>
        public App()
        {
            // Initialize the XAML framework
            this.InitializeComponent();

            // Subscribe to application lifecycle events
            this.Suspending += OnSuspending;
            this.Resuming += OnResuming;

            // Set up global exception handling for debugging and error reporting
            this.UnhandledException += OnUnhandledException;

            Debug.WriteLine("App: Application constructor completed");
        }

        #endregion

        #region Application Lifecycle Events

        /// <summary>
        /// Invoked when the application is launched normally by the end user.
        /// Other entry points will be used such as when the application is launched
        /// to open a specific file or when it is launched by a protocol.
        /// </summary>
        /// <param name="e">Details about the launch request and process</param>
        /// <remarks>
        /// This method handles several initialization scenarios:
        /// - Fresh app launch (cold start)
        /// - App launch after termination (restore state)
        /// - App launch with parameters (file associations, protocols)
        /// 
        /// The method sets up the main UI frame, initializes services, applies themes,
        /// and navigates to the appropriate starting page based on the launch context.
        /// </remarks>
        protected override async void OnLaunched(LaunchActivatedEventArgs e)
        {
            try
            {
                Debug.WriteLine($"App: OnLaunched called. LaunchKind: {e.Kind}, Arguments: '{e.Arguments}'");

                // Get the root frame - this is the main navigation container for the app
                Frame rootFrame = Window.Current.Content as Frame;

                // Do not repeat app initialization when the Window already has content,
                // just ensure that the window is active
                if (rootFrame == null)
                {
                    Debug.WriteLine("App: Creating new root frame");

                    // Create a Frame to act as the navigation context and navigate to the first page
                    rootFrame = new Frame();

                    // Subscribe to navigation failure events for debugging
                    rootFrame.NavigationFailed += OnNavigationFailed;

                    // Set up the navigation helper with the root frame
                    NavigationHelper.RootFrame = rootFrame;
                    NavigationHelper.EnableNavigationEvents();

                    // Check if this is a fresh launch or restoration from suspension
                    if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                    {
                        Debug.WriteLine("App: Restoring from termination");
                        
                        try
                        {
                            // Restore navigation state if available
                            await RestoreNavigationStateAsync();
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"App: Error restoring navigation state: {ex.Message}");
                            // Continue with normal startup if restoration fails
                        }
                    }

                    // Place the frame in the current Window
                    Window.Current.Content = rootFrame;
                }

                // Initialize application services if not already done
                if (!_isInitialized)
                {
                    await InitializeApplicationAsync();
                    _isInitialized = true;
                }

                // Navigate to the main page if the frame is empty or if explicitly requested
                if (e.PrelaunchActivated == false)
                {
                    if (rootFrame.Content == null)
                    {
                        Debug.WriteLine("App: Navigating to MainPage");
                        
                        // Navigate to the main page, passing launch arguments if any
                        rootFrame.Navigate(typeof(MainPage), e.Arguments);
                    }

                    // Ensure the current window is active
                    Window.Current.Activate();
                    
                    Debug.WriteLine("App: Application launch completed successfully");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"App: Critical error during launch: {ex}");
                
                // In a production app, you might want to show an error dialog
                // or attempt recovery. For this demo, we'll log and continue.
                try
                {
                    // Attempt minimal recovery by creating a basic frame
                    if (Window.Current.Content == null)
                    {
                        var recoveryFrame = new Frame();
                        Window.Current.Content = recoveryFrame;
                        recoveryFrame.Navigate(typeof(MainPage));
                        Window.Current.Activate();
                    }
                }
                catch (Exception recoveryEx)
                {
                    Debug.WriteLine($"App: Recovery attempt failed: {recoveryEx}");
                }
            }
        }

        /// <summary>
        /// Invoked when application execution is being suspended.
        /// Application state is saved without knowing whether the application
        /// will be terminated or resumed with the contents of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request</param>
        /// <param name="e">Details about the suspend request</param>
        /// <remarks>
        /// During suspension, the application should:
        /// - Save user data and application state
        /// - Release exclusive resources and file handles
        /// - Prepare for possible termination
        /// - Record the suspension time for welcome messages
        /// 
        /// The system gives the app a limited time (5 seconds) to complete
        /// suspension tasks, so operations should be quick and efficient.
        /// </remarks>
        private async void OnSuspending(object sender, SuspendingEventArgs e)
        {
            try
            {
                Debug.WriteLine("App: Application suspending");

                // Get a deferral to perform async operations during suspension
                var deferral = e.SuspendingOperation.GetDeferral();

                try
                {
                    // Record the suspension time for welcome back messages
                    _suspensionTime = DateTime.Now;

                    // Save application state and user data
                    await SaveApplicationStateAsync();

                    // Save navigation state for potential restoration
                    await SaveNavigationStateAsync();

                    Debug.WriteLine($"App: Suspension completed at {_suspensionTime}");
                }
                finally
                {
                    // Always complete the deferral to allow suspension to proceed
                    deferral.Complete();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"App: Error during suspension: {ex.Message}");
                // Even if suspension tasks fail, we must allow the system to suspend
            }
        }

        /// <summary>
        /// Invoked when the application is resumed from suspension.
        /// The application state is restored and UI can be refreshed.
        /// </summary>
        /// <param name="sender">The source of the resume event</param>
        /// <param name="e">Details about the resume event</param>
        /// <remarks>
        /// During resume, the application should:
        /// - Refresh data that may have changed while suspended
        /// - Update UI to reflect current state
        /// - Reconnect to services and networks
        /// - Show appropriate welcome back messages
        /// 
        /// The application memory state is preserved during suspension,
        /// so only data refresh and UI updates are typically needed.
        /// </remarks>
        private async void OnResuming(object sender, object e)
        {
            try
            {
                Debug.WriteLine("App: Application resuming");

                // Calculate how long the app was suspended
                var suspensionDuration = DateTime.Now - _suspensionTime;
                Debug.WriteLine($"App: Was suspended for {suspensionDuration.TotalMinutes:F1} minutes");

                // Refresh application state after resume
                await RefreshApplicationStateAsync();

                // Update the last launch time to reflect the resume
                SettingsService.Instance.UpdateAppLaunchTime();

                // If the app was suspended for a significant time, show welcome back message
                if (suspensionDuration.TotalMinutes > 1)
                {
                    ShowWelcomeBackMessage(suspensionDuration);
                }

                Debug.WriteLine("App: Resume completed successfully");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"App: Error during resume: {ex.Message}");
                // Continue operation even if resume tasks fail
            }
        }

        #endregion

        #region Initialization Methods

        /// <summary>
        /// Initializes all application services and systems.
        /// This method sets up the foundation services needed by the application.
        /// </summary>
        /// <returns>A task representing the asynchronous initialization operation</returns>
        /// <remarks>
        /// This method initializes services in the correct order to handle dependencies:
        /// 1. Settings service (needed for theme application)
        /// 2. Theme application (affects UI rendering)
        /// 3. Data service (loads customer data)
        /// 4. Network service (prepares for API calls)
        /// 
        /// Each service initialization is wrapped in try-catch to prevent
        /// one service failure from blocking the entire application.
        /// </remarks>
        private async System.Threading.Tasks.Task InitializeApplicationAsync()
        {
            try
            {
                Debug.WriteLine("App: Starting application initialization");

                // Initialize settings service and apply saved theme
                try
                {
                    var settingsService = SettingsService.Instance;
                    settingsService.UpdateAppLaunchTime();
                    
                    // Apply the saved theme to the application
                    settingsService.ApplyTheme(settingsService.CurrentTheme);
                    
                    Debug.WriteLine($"App: Settings service initialized, theme: {settingsService.CurrentTheme}");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"App: Error initializing settings service: {ex.Message}");
                }

                // Initialize data service and load customer data
                try
                {
                    var dataService = DataService.Instance;
                    await dataService.LoadDataAsync();
                    
                    Debug.WriteLine($"App: Data service initialized, {dataService.CustomerCount} customers loaded");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"App: Error initializing data service: {ex.Message}");
                }

                // Initialize network service for API operations
                try
                {
                    var networkService = NetworkService.Instance;
                    bool isConnected = networkService.IsAvailable;
                    
                    Debug.WriteLine($"App: Network service initialized, connected: {isConnected}");
                    
                    // Optionally test API connectivity
                    if (isConnected)
                    {
                        _ = System.Threading.Tasks.Task.Run(async () =>
                        {
                            bool apiAvailable = await networkService.TestApiConnectivityAsync();
                            Debug.WriteLine($"App: API connectivity test result: {apiAvailable}");
                        });
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"App: Error initializing network service: {ex.Message}");
                }

                Debug.WriteLine("App: Application initialization completed");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"App: Critical error during initialization: {ex}");
                // Continue operation with partial initialization
            }
        }

        #endregion

        #region State Management

        /// <summary>
        /// Saves the current application state to persistent storage.
        /// This includes user data, settings, and any temporary state that should survive termination.
        /// </summary>
        /// <returns>A task representing the asynchronous save operation</returns>
        private async System.Threading.Tasks.Task SaveApplicationStateAsync()
        {
            try
            {
                Debug.WriteLine("App: Saving application state");

                // Save customer data
                try
                {
                    bool dataSaved = await DataService.Instance.SaveDataAsync();
                    Debug.WriteLine($"App: Customer data saved: {dataSaved}");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"App: Error saving customer data: {ex.Message}");
                }

                // Save application settings
                try
                {
                    SettingsService.Instance.SaveSettings();
                    Debug.WriteLine("App: Settings saved");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"App: Error saving settings: {ex.Message}");
                }

                Debug.WriteLine("App: Application state save completed");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"App: Error during state save: {ex.Message}");
            }
        }

        /// <summary>
        /// Saves the current navigation state for restoration after termination.
        /// </summary>
        /// <returns>A task representing the asynchronous save operation</returns>
        private async System.Threading.Tasks.Task SaveNavigationStateAsync()
        {
            try
            {
                string navigationState = NavigationHelper.GetNavigationState();
                if (!string.IsNullOrEmpty(navigationState))
                {
                    // Save navigation state to local settings
                    var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
                    localSettings.Values["NavigationState"] = navigationState;
                    
                    Debug.WriteLine("App: Navigation state saved");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"App: Error saving navigation state: {ex.Message}");
            }
        }

        /// <summary>
        /// Restores the navigation state from persistent storage.
        /// </summary>
        /// <returns>A task representing the asynchronous restore operation</returns>
        private async System.Threading.Tasks.Task RestoreNavigationStateAsync()
        {
            try
            {
                var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
                if (localSettings.Values.TryGetValue("NavigationState", out object navigationState) &&
                    navigationState is string stateString)
                {
                    bool restored = NavigationHelper.SetNavigationState(stateString);
                    Debug.WriteLine($"App: Navigation state restored: {restored}");
                    
                    // Clear the saved state after restoration
                    localSettings.Values.Remove("NavigationState");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"App: Error restoring navigation state: {ex.Message}");
            }
        }

        /// <summary>
        /// Refreshes application state after resuming from suspension.
        /// This updates data that may have changed while the app was suspended.
        /// </summary>
        /// <returns>A task representing the asynchronous refresh operation</returns>
        private async System.Threading.Tasks.Task RefreshApplicationStateAsync()
        {
            try
            {
                Debug.WriteLine("App: Refreshing application state");

                // Refresh data service state
                try
                {
                    // Check if data file was modified while suspended
                    // In a real app, you might reload data or sync with a server
                    await DataService.Instance.LoadDataAsync();
                    Debug.WriteLine("App: Data refreshed");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"App: Error refreshing data: {ex.Message}");
                }

                // Refresh network connectivity
                try
                {
                    var networkService = NetworkService.Instance;
                    bool isConnected = networkService.IsAvailable;
                    Debug.WriteLine($"App: Network connectivity refreshed: {isConnected}");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"App: Error refreshing network state: {ex.Message}");
                }

                Debug.WriteLine("App: Application state refresh completed");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"App: Error during state refresh: {ex.Message}");
            }
        }

        #endregion

        #region Error Handling

        /// <summary>
        /// Invoked when Navigation to a certain page fails.
        /// This provides centralized error handling for navigation failures.
        /// </summary>
        /// <param name="sender">The Frame which failed navigation</param>
        /// <param name="e">Details about the navigation failure</param>
        /// <remarks>
        /// Navigation failures can occur due to:
        /// - Missing page classes
        /// - Invalid navigation parameters
        /// - Memory issues
        /// - Programming errors in page constructors
        /// 
        /// This handler logs the error and could implement recovery strategies
        /// such as navigating to a fallback page or showing an error message.
        /// </remarks>
        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            Debug.WriteLine($"App: Navigation failed to {e.SourcePageType.FullName}: {e.Exception.Message}");

            // In a production app, you might want to:
            // 1. Log the error to a telemetry service
            // 2. Navigate to an error page
            // 3. Show a user-friendly error message
            // 4. Attempt recovery by navigating to a known good page

            // For this demo, we'll attempt to navigate to the main page as recovery
            try
            {
                if (sender is Frame frame && frame.Content == null)
                {
                    frame.Navigate(typeof(MainPage));
                    Debug.WriteLine("App: Recovered from navigation failure by navigating to MainPage");
                }
            }
            catch (Exception recoveryEx)
            {
                Debug.WriteLine($"App: Recovery navigation also failed: {recoveryEx.Message}");
            }

            // Mark the exception as handled to prevent app termination
            e.Handled = true;
        }

        /// <summary>
        /// Handles unhandled exceptions that occur anywhere in the application.
        /// This provides a last line of defense against application crashes.
        /// </summary>
        /// <param name="sender">The application object</param>
        /// <param name="e">Details about the unhandled exception</param>
        /// <remarks>
        /// Unhandled exceptions represent serious programming errors or
        /// unexpected system conditions. This handler logs the error and
        /// can implement crash reporting or recovery mechanisms.
        /// 
        /// Setting e.Handled = true prevents the application from terminating,
        /// but should only be done if the application can continue safely.
        /// </remarks>
        private void OnUnhandledException(object sender, Windows.UI.Xaml.UnhandledExceptionEventArgs e)
        {
            Debug.WriteLine($"App: Unhandled exception: {e.Exception}");

            // In a production app, you would typically:
            // 1. Log the exception to a crash reporting service
            // 2. Save any critical user data
            // 3. Show an error dialog to the user
            // 4. Determine if the app can continue safely

            // For this demo, we'll log the error and attempt to continue
            // Note: Setting Handled = true prevents app termination but should
            // only be done if you're confident the app can continue safely
            
            try
            {
                // Attempt to save critical data before potential termination
                var saveTask = SaveApplicationStateAsync();
                // Don't await here as we need to complete quickly
                
                Debug.WriteLine("App: Emergency state save initiated");
            }
            catch (Exception saveEx)
            {
                Debug.WriteLine($"App: Error during emergency save: {saveEx.Message}");
            }

            // For debugging purposes, we'll mark as handled to continue execution
            // In production, you should carefully consider whether it's safe to continue
            e.Handled = true;
        }

        #endregion

        #region UI Helpers

        /// <summary>
        /// Shows a welcome back message to the user after resuming from suspension.
        /// This provides feedback about how long the app was suspended.
        /// </summary>
        /// <param name="suspensionDuration">How long the app was suspended</param>
        private void ShowWelcomeBackMessage(TimeSpan suspensionDuration)
        {
            try
            {
                // Get the welcome message from settings service
                string welcomeMessage = SettingsService.Instance.GetWelcomeMessage();
                
                Debug.WriteLine($"App: Welcome back message: {welcomeMessage}");

                // In a real app, you might show this in:
                // - A toast notification
                // - An InfoBar in the main page
                // - A flyout or content dialog
                // - The app's status bar

                // For this demo, we'll just log it
                // You could implement UI notification here
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"App: Error showing welcome message: {ex.Message}");
            }
        }

        #endregion

        #region Cleanup

        /// <summary>
        /// Performs cleanup when the application is closing.
        /// This method is called automatically by the framework.
        /// </summary>
        /// <remarks>
        /// During cleanup, the application should:
        /// - Dispose of resources and services
        /// - Unsubscribe from events to prevent memory leaks
        /// - Save any final state information
        /// - Close network connections and file handles
        /// </remarks>
        protected override void OnApplicationExit(object sender, object e)
        {
            try
            {
                Debug.WriteLine("App: Application exiting, performing cleanup");

                // Disable navigation events to prevent memory leaks
                NavigationHelper.DisableNavigationEvents();

                // Dispose of network service resources
                try
                {
                    NetworkService.Instance.Dispose();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"App: Error disposing network service: {ex.Message}");
                }

                // Unsubscribe from application events
                this.Suspending -= OnSuspending;
                this.Resuming -= OnResuming;
                this.UnhandledException -= OnUnhandledException;

                Debug.WriteLine("App: Application cleanup completed");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"App: Error during application cleanup: {ex.Message}");
            }

            base.OnApplicationExit(sender, e);
        }

        #endregion
    }
}