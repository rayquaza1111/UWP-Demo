using System;
using System.Diagnostics;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
// 6. Suspension & Resume Handling: Import for window visibility tracking
using Windows.UI.Core;
using UWP_Demo.Services;

namespace UWP_Demo
{
    sealed partial class App : Application
    {
        // 6. Suspension & Resume Handling: Flag to prevent multiple event subscriptions
        private static bool _visibilityEventsSubscribed = false;
        
        public App()
        {
            // Initialize the XAML framework
            this.InitializeComponent();

            // 6. Suspension & Resume Handling: Subscribe to application lifecycle events
            this.Suspending += OnSuspending;
            this.Resuming += OnResuming;

            Debug.WriteLine("App: Application initialized");
        }

        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
            try
            {
                Debug.WriteLine($"App: OnLaunched called. LaunchKind: {e.Kind}, Arguments: '{e.Arguments}'");

                // 6. Suspension & Resume Handling: Handle app launch and determine if resuming from suspension
                SuspensionService.Instance.HandleLaunch();

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

                    // 6. Suspension & Resume Handling: Restore frame navigation state if resuming from suspension
                    if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                    {
                        Debug.WriteLine("SUSPENSION & RESUME: App was terminated, attempting to restore state");
                        try
                        {
                            // SUSPENSION & RESUME: Could restore navigation state here if needed
                            // For now, we'll handle state restoration in the page level
                        }
                        catch (Exception restoreEx)
                        {
                            Debug.WriteLine($"SUSPENSION & RESUME ERROR: Failed to restore state - {restoreEx.Message}");
                        }
                    }

                    // Place the frame in the current Window
                    Window.Current.Content = rootFrame;

                    // 6. Suspension & Resume Handling: Subscribe to window visibility changes for better testing
                    try
                    {
                        if (!_visibilityEventsSubscribed)
                        {
                            // Try multiple approaches to ensure the visibility event is subscribed
                            var currentWindow = Window.Current;
                            if (currentWindow != null)
                            {
                                currentWindow.VisibilityChanged += OnWindowVisibilityChanged;
                                _visibilityEventsSubscribed = true;
                                Debug.WriteLine("SUSPENSION & RESUME: Window visibility tracking enabled");
                                
                                // Test if the event is actually subscribed
                                Debug.WriteLine($"SUSPENSION & RESUME: Window.Current is NOT NULL");
                                Debug.WriteLine($"SUSPENSION & RESUME: Current window visibility: {currentWindow.Visible}");
                                Debug.WriteLine($"SUSPENSION & RESUME: Window bounds: {currentWindow.Bounds}");
                                
                                // Also try to subscribe to the CoreWindow for additional coverage
                                try
                                {
                                    var coreWindow = Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow;
                                    if (coreWindow != null)
                                    {
                                        Debug.WriteLine("SUSPENSION & RESUME: CoreWindow is available for additional event handling");
                                    }
                                }
                                catch (Exception coreEx)
                                {
                                    Debug.WriteLine($"SUSPENSION & RESUME: CoreWindow access failed - {coreEx.Message}");
                                }
                            }
                            else
                            {
                                Debug.WriteLine("SUSPENSION & RESUME ERROR: Window.Current is NULL!");
                            }
                        }
                        else
                        {
                            Debug.WriteLine("SUSPENSION & RESUME: Window visibility tracking already enabled");
                        }
                    }
                    catch (Exception visEx)
                    {
                        Debug.WriteLine($"SUSPENSION & RESUME ERROR: Failed to setup visibility tracking - {visEx.Message}");
                        Debug.WriteLine($"SUSPENSION & RESUME ERROR: Exception details: {visEx}");
                    }

                    // THEME PERSISTENCE: Apply saved theme on app startup - loads theme from storage and applies it
                    try
                    {
                        SettingsService.Instance.InitializeTheme();
                        Debug.WriteLine($"App: Theme initialized to {SettingsService.Instance.GetThemeName()}");
                    }
                    catch (Exception themeEx)
                    {
                        Debug.WriteLine($"App: Error initializing theme - {themeEx.Message}");
                    }
                }

                // Navigate to the main page if the frame is empty or if explicitly requested
                if (e.PrelaunchActivated == false)
                {
                    if (rootFrame.Content == null)
                    {
                        Debug.WriteLine("App: Navigating to MainPage");
                        
                        // 6. Suspension & Resume Handling: Pass suspension state information to MainPage
                        var navigationParameter = new 
                        {
                            LaunchKind = e.Kind.ToString(),
                            PreviousState = e.PreviousExecutionState.ToString(),
                            WasSuspended = SuspensionService.Instance.WasSuspended,
                            WelcomeMessage = SuspensionService.Instance.GetWelcomeBackMessage()
                        };

                        // Navigate to MainPage which will handle internal navigation
                        rootFrame.Navigate(typeof(MainPage), navigationParameter);
                    }

                    // Ensure the current window is active
                    Window.Current.Activate();
                    
                    Debug.WriteLine("App: Application launch completed successfully");
                    
                    // 6. Suspension & Resume Handling: Log suspension state summary
                    Debug.WriteLine($"SUSPENSION & RESUME: {SuspensionService.Instance.GetSuspensionSummary()}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"App: Critical error during launch: {ex}");
                
                // Attempt minimal recovery by creating a basic frame
                try
                {
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

        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            Debug.WriteLine($"App: Navigation failed to {e.SourcePageType.FullName}: {e.Exception.Message}");

            // Mark the exception as handled to prevent app termination
            e.Handled = true;
        }

        #region 6. Suspension & Resume Handling: Event Handlers

        /// <summary>
        /// 6. Suspension & Resume Handling: Handle window visibility changes for testing
        /// This helps simulate suspension when the app is minimized
        /// </summary>
        private void OnWindowVisibilityChanged(object sender, VisibilityChangedEventArgs e)
        {
            try
            {
                Debug.WriteLine($"SUSPENSION & RESUME: Window visibility changed to {e.Visible}");
                
                if (!e.Visible)
                {
                    // Window became invisible (minimized or hidden)
                    Debug.WriteLine("SUSPENSION & RESUME: Window minimized - simulating suspension");
                    SimulateSuspension();
                }
                else
                {
                    // Window became visible again
                    Debug.WriteLine("SUSPENSION & RESUME: Window restored - simulating resume");
                    SimulateResume();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"SUSPENSION & RESUME ERROR: Window visibility event failed - {ex.Message}");
            }
        }

        /// <summary>
        /// 6. Suspension & Resume Handling: Simulate suspension for testing
        /// </summary>
        private void SimulateSuspension()
        {
            try
            {
                Debug.WriteLine("SUSPENSION & RESUME: SimulateSuspension called");
                
                // Get current page info (simplified)
                var currentPage = "Unknown";
                var customerCount = 0;
                
                try
                {
                    var rootFrame = Window.Current.Content as Frame;
                    if (rootFrame?.Content != null)
                    {
                        currentPage = rootFrame.Content.GetType().Name;
                    }
                }
                catch { }
                
                SuspensionService.Instance.SaveSuspensionState(currentPage, customerCount);
                Debug.WriteLine("SUSPENSION & RESUME: Simulation suspension completed");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"SUSPENSION & RESUME ERROR: SimulateSuspension failed - {ex.Message}");
            }
        }

        /// <summary>
        /// 6. Suspension & Resume Handling: Simulate resume for testing
        /// </summary>
        private void SimulateResume()
        {
            try
            {
                Debug.WriteLine("SUSPENSION & RESUME: SimulateResume called");
                SuspensionService.Instance.HandleResume();
                Debug.WriteLine("SUSPENSION & RESUME: Simulation resume completed");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"SUSPENSION & RESUME ERROR: SimulateResume failed - {ex.Message}");
            }
        }

        /// <summary>
        /// 6. Suspension & Resume Handling: Handle actual app suspension
        /// </summary>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            try
            {
                Debug.WriteLine("SUSPENSION & RESUME: OnSuspending called");
                
                var deferral = e.SuspendingOperation.GetDeferral();
                
                try
                {
                    // Get current page info
                    var currentPage = "Unknown";
                    var customerCount = 0;
                    
                    try
                    {
                        var rootFrame = Window.Current.Content as Frame;
                        if (rootFrame?.Content != null)
                        {
                            currentPage = rootFrame.Content.GetType().Name;
                        }
                    }
                    catch { }
                    
                    SuspensionService.Instance.SaveSuspensionState(currentPage, customerCount);
                    Debug.WriteLine("SUSPENSION & RESUME: OnSuspending completed");
                }
                finally
                {
                    deferral.Complete();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"SUSPENSION & RESUME ERROR: OnSuspending failed - {ex.Message}");
            }
        }

        /// <summary>
        /// 6. Suspension & Resume Handling: Handle actual app resume
        /// </summary>
        private void OnResuming(object sender, object e)
        {
            try
            {
                Debug.WriteLine("SUSPENSION & RESUME: OnResuming called");
                SuspensionService.Instance.HandleResume();
                Debug.WriteLine("SUSPENSION & RESUME: OnResuming completed");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"SUSPENSION & RESUME ERROR: OnResuming failed - {ex.Message}");
            }
        }

        #endregion
    }
}