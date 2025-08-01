using System;
using System.Diagnostics;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
// ?? SUSPENSION & RESUME: Import for window visibility tracking
using Windows.UI.Core;
using UWP_Demo.Services;

namespace UWP_Demo
{
    sealed partial class App : Application
    {
        // ?? SUSPENSION & RESUME: Flag to prevent multiple event subscriptions
        // ??? Safety: Prevents memory leaks and duplicate event handlers
        private static bool _visibilityEventsSubscribed = false;
        
        public App()
        {
            // Initialize the XAML framework
            this.InitializeComponent();

            // ?? SUSPENSION & RESUME: Subscribe to application lifecycle events
            this.Suspending += OnSuspending;
            this.Resuming += OnResuming;  // ?? SUSPENSION & RESUME: Handle app resuming from suspension

            Debug.WriteLine("?? App: Application initialized with suspension & resume handling");
        }

        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
            try
            {
                Debug.WriteLine($"App: OnLaunched called. LaunchKind: {e.Kind}, Arguments: '{e.Arguments}'");

                // SUSPENSION & RESUME: Handle app launch and determine if resuming from suspension
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

                    // SUSPENSION & RESUME: Restore frame navigation state if resuming from suspension
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

                    // ?? SUSPENSION & RESUME: Subscribe to window visibility changes for better testing
                    // ?? Window Tracking: Detects minimize/restore events for immediate suspension simulation
                    // ?? Purpose: Provides more reliable testing than waiting for actual UWP suspension
                    try
                    {
                        if (!_visibilityEventsSubscribed)
                        {
                            Window.Current.VisibilityChanged += OnWindowVisibilityChanged;
                            _visibilityEventsSubscribed = true;
                            Debug.WriteLine("?? SUSPENSION & RESUME: Window visibility tracking enabled");
                        }
                        else
                        {
                            Debug.WriteLine("?? SUSPENSION & RESUME: Window visibility tracking already enabled");
                        }
                    }
                    catch (Exception visEx)
                    {
                        Debug.WriteLine($"?? SUSPENSION & RESUME ERROR: Failed to setup visibility tracking - {visEx.Message}");
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
                        
                        // SUSPENSION & RESUME: Pass suspension state information to MainPage
                        // ?? Launch Info: Provides app lifecycle context for UI display
                        // ?? Enables: Welcome message generation based on launch conditions
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
                    
                    // SUSPENSION & RESUME: Log suspension state summary
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

        /// <summary>
        /// ?? SUSPENSION & RESUME: Handle window visibility changes (minimize/restore)
        /// ?? Purpose: Provides more immediate testing feedback than actual suspension
        /// ?? Triggers: Immediate suspension/resume simulation for testing
        /// ? Benefits: Reliable testing without waiting for OS-level suspension
        /// </summary>
        private void OnWindowVisibilityChanged(object sender, VisibilityChangedEventArgs e)
        {
            try
            {
                Debug.WriteLine($"?? SUSPENSION & RESUME: Window visibility changed - Visible: {e.Visible}");
                
                if (!e.Visible)
                {
                    // ?? SUSPENSION & RESUME: Window hidden (minimized or covered) - simulate suspension
                    Debug.WriteLine("?? SUSPENSION & RESUME: Window hidden - simulating suspension for testing");
                    
                    // ?? SUSPENSION & RESUME: Use async pattern to avoid blocking UI thread
                    var _ = Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(
                        Windows.UI.Core.CoreDispatcherPriority.Low, () =>
                        {
                            SimulateSuspension();
                        });
                }
                else
                {
                    // ?? SUSPENSION & RESUME: Window visible (restored) - simulate resume
                    Debug.WriteLine("?? SUSPENSION & RESUME: Window visible - simulating resume for testing");
                    
                    // ?? SUSPENSION & RESUME: Use async pattern to avoid blocking UI thread
                    var _ = Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(
                        Windows.UI.Core.CoreDispatcherPriority.Low, () =>
                        {
                            SimulateResume();
                        });
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"?? SUSPENSION & RESUME ERROR: Failed to handle visibility change - {ex.Message}");
            }
        }

        /// <summary>
        /// ?? SUSPENSION & RESUME: Simulate suspension for testing when window is hidden
        /// ?? Captures: Current page information and app state
        /// ??? Safety: Non-blocking implementation to prevent UI thread freezing
        /// ?? Purpose: Immediate feedback for testing without actual OS suspension
        /// </summary>
        private void SimulateSuspension()
        {
            try
            {
                Debug.WriteLine("?? SUSPENSION & RESUME: Simulating suspension state for testing...");
                
                // ?? SUSPENSION & RESUME: Get current page information
                string currentPage = "Unknown";
                int customerCount = 0;

                try
                {
                    var rootFrame = Window.Current.Content as Frame;
                    if (rootFrame?.Content != null)
                    {
                        currentPage = rootFrame.Content.GetType().Name;
                        
                        // ?? SUSPENSION & RESUME: Don't block UI thread - use estimate instead
                        // ??? Safety: The exact count isn't critical for suspension simulation
                        customerCount = 0; // Will be updated properly on resume
                    }
                }
                catch (Exception pageEx)
                {
                    Debug.WriteLine($"?? SUSPENSION & RESUME: Error getting page info - {pageEx.Message}");
                }

                // ?? SUSPENSION & RESUME: Save suspension state for testing
                SuspensionService.Instance.SaveSuspensionState(currentPage, customerCount);
                
                Debug.WriteLine($"?? SUSPENSION & RESUME: Simulated suspension completed");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"?? SUSPENSION & RESUME ERROR: Failed to simulate suspension - {ex.Message}");
            }
        }

        /// <summary>
        /// ?? SUSPENSION & RESUME: Simulate resume for testing when window becomes visible
        /// ? Purpose: Updates resume timestamp and prepares welcome message
        /// ??? Safety: Simplified implementation to prevent threading issues
        /// </summary>
        private void SimulateResume()
        {
            try
            {
                Debug.WriteLine("?? SUSPENSION & RESUME: Simulating resume state for testing...");
                
                // ?? SUSPENSION & RESUME: Handle resume state
                SuspensionService.Instance.HandleResume();

                Debug.WriteLine($"?? SUSPENSION & RESUME: Simulated resume completed");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"?? SUSPENSION & RESUME ERROR: Failed to simulate resume - {ex.Message}");
            }
        }

        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            Debug.WriteLine($"App: Navigation failed to {e.SourcePageType.FullName}: {e.Exception.Message}");

            // Mark the exception as handled to prevent app termination
            e.Handled = true;
        }

        /// <summary>
        /// ?? SUSPENSION & RESUME: Handle application suspension
        /// ?? Saves: App state, timestamps, and current page information
        /// ?? Purpose: Preserve app state for restoration when app resumes
        /// ?? Data: Current page, customer count, settings, suspension timestamp
        /// </summary>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();

            try
            {
                Debug.WriteLine("?? SUSPENSION & RESUME: App is suspending, saving state...");

                // ?? SUSPENSION & RESUME: Get current page information
                string currentPage = "Unknown";
                int customerCount = 0;

                try
                {
                    var rootFrame = Window.Current.Content as Frame;
                    if (rootFrame?.Content != null)
                    {
                        currentPage = rootFrame.Content.GetType().Name;
                        
                        // ?? SUSPENSION & RESUME: Don't block UI thread during suspension
                        // ??? Safety: Customer count will be updated when app resumes
                        customerCount = 0; // Safe default value
                    }
                }
                catch (Exception pageEx)
                {
                    Debug.WriteLine($"?? SUSPENSION & RESUME: Error getting page info - {pageEx.Message}");
                }

                // ?? SUSPENSION & RESUME: Save suspension state with current app information
                SuspensionService.Instance.SaveSuspensionState(currentPage, customerCount);

                // THEME PERSISTENCE: Save any pending settings before suspension
                try
                {
                    var currentSettings = SettingsService.Instance.GetSettingsSummary();
                    Debug.WriteLine($"App: Suspending with settings - {currentSettings}");
                }
                catch (Exception settingsEx)
                {
                    Debug.WriteLine($"App: Error accessing settings during suspend - {settingsEx.Message}");
                }

                Debug.WriteLine($"?? SUSPENSION & RESUME: App suspension state saved successfully");
                Debug.WriteLine($"?? SUSPENSION & RESUME: {SuspensionService.Instance.GetSuspensionSummary()}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"?? SUSPENSION & RESUME ERROR: Failed to save suspension state - {ex.Message}");
            }
            finally
            {
                // Always complete the deferral to allow suspension to proceed
                deferral.Complete();
            }
        }

        /// <summary>
        /// ?? SUSPENSION & RESUME: Handle application resuming from suspension
        /// ? Updates: Resume timestamp and logs resumption information
        /// ?? Triggers: Welcome message generation for user feedback
        /// ?? Purpose: Provide seamless user experience when returning to app
        /// </summary>
        private void OnResuming(object sender, object e)
        {
            try
            {
                Debug.WriteLine("?? SUSPENSION & RESUME: App is resuming from suspension...");

                // ?? SUSPENSION & RESUME: Handle resume state
                SuspensionService.Instance.HandleResume();

                // ?? SUSPENSION & RESUME: Log welcome back message
                string welcomeMessage = SuspensionService.Instance.GetWelcomeBackMessage();
                Debug.WriteLine($"?? SUSPENSION & RESUME: {welcomeMessage}");

                // ?? SUSPENSION & RESUME: Log suspension state summary
                Debug.WriteLine($"?? SUSPENSION & RESUME: {SuspensionService.Instance.GetSuspensionSummary()}");

                Debug.WriteLine("?? SUSPENSION & RESUME: App resume handling completed");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"?? SUSPENSION & RESUME ERROR: Failed to handle resume - {ex.Message}");
            }
        }
    }
}