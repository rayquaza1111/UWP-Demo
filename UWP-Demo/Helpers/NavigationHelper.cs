using System;
using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace UWP_Demo.Helpers
{
    /// <summary>
    /// NavigationHelper provides a centralized way to manage page navigation in the UWP application.
    /// This helper class maintains navigation state, handles parameter passing between pages,
    /// and provides methods for common navigation scenarios.
    /// </summary>
    /// <remarks>
    /// This class demonstrates several UWP navigation concepts:
    /// - Frame-based navigation between pages
    /// - Parameter passing during navigation
    /// - Navigation state preservation
    /// - Back button handling
    /// - Navigation history management
    /// 
    /// The NavigationHelper acts as a service that can be used by ViewModels
    /// to navigate without directly coupling to the UI framework.
    /// </remarks>
    public static class NavigationHelper
    {
        #region Private Fields

        /// <summary>
        /// Cache of navigation parameters for pages that haven't been navigated to yet.
        /// This allows us to store parameters before navigation and retrieve them after.
        /// </summary>
        private static readonly Dictionary<Type, object> _navigationParameters = new Dictionary<Type, object>();

        /// <summary>
        /// The main application frame used for navigation.
        /// This is set during app initialization and used for all page navigation.
        /// </summary>
        private static Frame _rootFrame;

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the root frame used for navigation.
        /// This should be set during application startup (typically in App.xaml.cs).
        /// </summary>
        /// <remarks>
        /// The root frame is the main navigation container for the application.
        /// All page navigation operations will use this frame.
        /// </remarks>
        public static Frame RootFrame
        {
            get => _rootFrame;
            set => _rootFrame = value;
        }

        /// <summary>
        /// Gets whether the application can navigate back to a previous page.
        /// This is useful for enabling/disabling back buttons in the UI.
        /// </summary>
        public static bool CanGoBack => _rootFrame?.CanGoBack ?? false;

        /// <summary>
        /// Gets whether the application can navigate forward to a next page.
        /// This is useful for enabling/disabling forward buttons in the UI.
        /// </summary>
        public static bool CanGoForward => _rootFrame?.CanGoForward ?? false;

        /// <summary>
        /// Gets the type of the current page being displayed.
        /// Returns null if no page is currently loaded.
        /// </summary>
        public static Type CurrentPageType => _rootFrame?.CurrentSourcePageType;

        /// <summary>
        /// Gets the current page content as a Page object.
        /// Returns null if no page is currently loaded.
        /// </summary>
        public static Page CurrentPage => _rootFrame?.Content as Page;

        #endregion

        #region Navigation Methods

        /// <summary>
        /// Navigates to the specified page type without any parameters.
        /// </summary>
        /// <typeparam name="T">The type of page to navigate to (must inherit from Page)</typeparam>
        /// <returns>True if navigation was successful, false otherwise</returns>
        /// <example>
        /// // Navigate to the Settings page
        /// NavigationHelper.NavigateTo&lt;SettingsPage&gt;();
        /// </example>
        public static bool NavigateTo<T>() where T : Page
        {
            return NavigateTo<T>(null);
        }

        /// <summary>
        /// Navigates to the specified page type with a parameter.
        /// The parameter can be retrieved in the target page using GetNavigationParameter.
        /// </summary>
        /// <typeparam name="T">The type of page to navigate to (must inherit from Page)</typeparam>
        /// <param name="parameter">The parameter to pass to the target page</param>
        /// <returns>True if navigation was successful, false otherwise</returns>
        /// <example>
        /// // Navigate to the Edit page with a customer object
        /// var customer = GetSelectedCustomer();
        /// NavigationHelper.NavigateTo&lt;EditPage&gt;(customer);
        /// </example>
        public static bool NavigateTo<T>(object parameter) where T : Page
        {
            if (_rootFrame == null)
            {
                System.Diagnostics.Debug.WriteLine("NavigationHelper: Root frame is not set. Cannot navigate.");
                return false;
            }

            try
            {
                // Store the parameter for the target page type
                var pageType = typeof(T);
                if (parameter != null)
                {
                    _navigationParameters[pageType] = parameter;
                }
                else
                {
                    _navigationParameters.Remove(pageType);
                }

                // Perform the navigation
                bool result = _rootFrame.Navigate(pageType, parameter);
                
                if (!result)
                {
                    System.Diagnostics.Debug.WriteLine($"NavigationHelper: Failed to navigate to {pageType.Name}");
                }

                return result;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"NavigationHelper: Exception during navigation to {typeof(T).Name}: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Navigates to the specified page type by its Type object.
        /// This overload is useful when the page type is determined at runtime.
        /// </summary>
        /// <param name="pageType">The Type of the page to navigate to</param>
        /// <param name="parameter">The parameter to pass to the target page</param>
        /// <returns>True if navigation was successful, false otherwise</returns>
        /// <example>
        /// // Navigate based on a runtime-determined type
        /// Type pageType = GetPageTypeFromSettings();
        /// NavigationHelper.NavigateTo(pageType, someParameter);
        /// </example>
        public static bool NavigateTo(Type pageType, object parameter = null)
        {
            if (_rootFrame == null)
            {
                System.Diagnostics.Debug.WriteLine("NavigationHelper: Root frame is not set. Cannot navigate.");
                return false;
            }

            if (!typeof(Page).IsAssignableFrom(pageType))
            {
                System.Diagnostics.Debug.WriteLine($"NavigationHelper: Type {pageType.Name} does not inherit from Page.");
                return false;
            }

            try
            {
                // Store the parameter for the target page type
                if (parameter != null)
                {
                    _navigationParameters[pageType] = parameter;
                }
                else
                {
                    _navigationParameters.Remove(pageType);
                }

                // Perform the navigation
                bool result = _rootFrame.Navigate(pageType, parameter);
                
                if (!result)
                {
                    System.Diagnostics.Debug.WriteLine($"NavigationHelper: Failed to navigate to {pageType.Name}");
                }

                return result;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"NavigationHelper: Exception during navigation to {pageType.Name}: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Navigates back to the previous page in the navigation history.
        /// </summary>
        /// <returns>True if back navigation was successful, false if there's no back history</returns>
        /// <example>
        /// // Handle a back button click
        /// if (NavigationHelper.CanGoBack)
        /// {
        ///     NavigationHelper.GoBack();
        /// }
        /// </example>
        public static bool GoBack()
        {
            if (_rootFrame?.CanGoBack == true)
            {
                try
                {
                    _rootFrame.GoBack();
                    return true;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"NavigationHelper: Exception during back navigation: {ex.Message}");
                    return false;
                }
            }

            return false;
        }

        /// <summary>
        /// Navigates forward to the next page in the navigation history.
        /// This is only possible if the user has previously navigated back.
        /// </summary>
        /// <returns>True if forward navigation was successful, false if there's no forward history</returns>
        public static bool GoForward()
        {
            if (_rootFrame?.CanGoForward == true)
            {
                try
                {
                    _rootFrame.GoForward();
                    return true;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"NavigationHelper: Exception during forward navigation: {ex.Message}");
                    return false;
                }
            }

            return false;
        }

        #endregion

        #region Parameter Management

        /// <summary>
        /// Retrieves the navigation parameter for the specified page type.
        /// This should be called in the target page's OnNavigatedTo method or ViewModel constructor.
        /// </summary>
        /// <typeparam name="T">The expected type of the parameter</typeparam>
        /// <param name="pageType">The type of the page requesting the parameter</param>
        /// <returns>The parameter cast to the specified type, or default(T) if not found</returns>
        /// <example>
        /// // In EditPage.xaml.cs or EditViewModel
        /// protected override void OnNavigatedTo(NavigationEventArgs e)
        /// {
        ///     var customer = NavigationHelper.GetNavigationParameter&lt;Customer&gt;(typeof(EditPage));
        ///     if (customer != null)
        ///     {
        ///         LoadCustomer(customer);
        ///     }
        /// }
        /// </example>
        public static T GetNavigationParameter<T>(Type pageType)
        {
            if (_navigationParameters.TryGetValue(pageType, out object parameter))
            {
                if (parameter is T typedParameter)
                {
                    return typedParameter;
                }
                else if (parameter != null)
                {
                    System.Diagnostics.Debug.WriteLine($"NavigationHelper: Parameter for {pageType.Name} is {parameter.GetType().Name}, expected {typeof(T).Name}");
                }
            }

            return default(T);
        }

        /// <summary>
        /// Retrieves the navigation parameter for the current page.
        /// This is a convenience method that automatically uses the current page type.
        /// </summary>
        /// <typeparam name="T">The expected type of the parameter</typeparam>
        /// <returns>The parameter cast to the specified type, or default(T) if not found</returns>
        /// <example>
        /// // In any page's code-behind or ViewModel
        /// var customer = NavigationHelper.GetCurrentPageParameter&lt;Customer&gt;();
        /// </example>
        public static T GetCurrentPageParameter<T>()
        {
            var currentPageType = CurrentPageType;
            if (currentPageType != null)
            {
                return GetNavigationParameter<T>(currentPageType);
            }

            return default(T);
        }

        /// <summary>
        /// Clears the navigation parameter for the specified page type.
        /// This helps prevent memory leaks by removing references to large objects.
        /// </summary>
        /// <param name="pageType">The page type whose parameter should be cleared</param>
        /// <example>
        /// // Clean up parameters after processing them
        /// var customer = NavigationHelper.GetNavigationParameter&lt;Customer&gt;(typeof(EditPage));
        /// ProcessCustomer(customer);
        /// NavigationHelper.ClearNavigationParameter(typeof(EditPage));
        /// </example>
        public static void ClearNavigationParameter(Type pageType)
        {
            _navigationParameters.Remove(pageType);
        }

        /// <summary>
        /// Clears all stored navigation parameters.
        /// This is useful for memory cleanup when the app is being suspended or closed.
        /// </summary>
        /// <example>
        /// // In App.xaml.cs OnSuspending method
        /// NavigationHelper.ClearAllNavigationParameters();
        /// </example>
        public static void ClearAllNavigationParameters()
        {
            _navigationParameters.Clear();
        }

        #endregion

        #region State Management

        /// <summary>
        /// Gets the current navigation state as a string that can be persisted.
        /// This can be used to restore navigation state when the app is resumed.
        /// </summary>
        /// <returns>A string representing the current navigation state, or null if no frame is available</returns>
        /// <example>
        /// // Save navigation state during app suspension
        /// string navState = NavigationHelper.GetNavigationState();
        /// localSettings.Values["NavigationState"] = navState;
        /// </example>
        public static string GetNavigationState()
        {
            return _rootFrame?.GetNavigationState();
        }

        /// <summary>
        /// Restores the navigation state from a previously saved state string.
        /// This should be called during app activation to restore the user's previous location.
        /// </summary>
        /// <param name="navigationState">The navigation state string to restore</param>
        /// <returns>True if the state was successfully restored, false otherwise</returns>
        /// <example>
        /// // Restore navigation state during app activation
        /// if (localSettings.Values.TryGetValue("NavigationState", out object navState))
        /// {
        ///     NavigationHelper.SetNavigationState(navState as string);
        /// }
        /// </example>
        public static bool SetNavigationState(string navigationState)
        {
            if (_rootFrame == null || string.IsNullOrEmpty(navigationState))
                return false;

            try
            {
                _rootFrame.SetNavigationState(navigationState);
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"NavigationHelper: Exception restoring navigation state: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Clears the navigation history, removing all back/forward entries.
        /// This is useful when you want to prevent the user from navigating back
        /// (for example, after a logout operation).
        /// </summary>
        /// <example>
        /// // Clear history after logout
        /// NavigationHelper.ClearNavigationHistory();
        /// NavigationHelper.NavigateTo&lt;LoginPage&gt;();
        /// </example>
        public static void ClearNavigationHistory()
        {
            if (_rootFrame != null)
            {
                // Clear the back stack by creating a new navigation entry
                var currentPageType = CurrentPageType;
                if (currentPageType != null)
                {
                    var currentParameter = GetCurrentPageParameter<object>();
                    _rootFrame.BackStack.Clear();
                    
                    // Optionally navigate to the same page to refresh it
                    // NavigateTo(currentPageType, currentParameter);
                }
            }
        }

        #endregion

        #region Event Handling

        /// <summary>
        /// Event raised when navigation occurs to any page.
        /// This can be used to perform global navigation logging or analytics.
        /// </summary>
        public static event EventHandler<NavigationEventArgs> NavigationOccurred;

        /// <summary>
        /// Subscribes to navigation events from the root frame.
        /// This should be called during app initialization to enable navigation event handling.
        /// </summary>
        /// <example>
        /// // In App.xaml.cs after setting up the root frame
        /// NavigationHelper.RootFrame = rootFrame;
        /// NavigationHelper.EnableNavigationEvents();
        /// </example>
        public static void EnableNavigationEvents()
        {
            if (_rootFrame != null)
            {
                _rootFrame.Navigated += OnFrameNavigated;
            }
        }

        /// <summary>
        /// Unsubscribes from navigation events from the root frame.
        /// This should be called during app shutdown to prevent memory leaks.
        /// </summary>
        public static void DisableNavigationEvents()
        {
            if (_rootFrame != null)
            {
                _rootFrame.Navigated -= OnFrameNavigated;
            }
        }

        /// <summary>
        /// Handles the Navigated event from the root frame and raises our own NavigationOccurred event.
        /// </summary>
        /// <param name="sender">The frame that raised the event</param>
        /// <param name="e">Navigation event arguments</param>
        private static void OnFrameNavigated(object sender, NavigationEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine($"NavigationHelper: Navigated to {e.SourcePageType.Name}");
            
            // Raise our own event for any subscribers
            NavigationOccurred?.Invoke(sender, e);
        }

        #endregion
    }
}