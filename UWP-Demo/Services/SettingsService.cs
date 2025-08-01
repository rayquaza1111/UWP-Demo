using System;
using Windows.Storage;
using Windows.UI.Xaml;

namespace UWP_Demo.Services
{
    /// <summary>
    /// Service for managing application settings with persistent storage
    /// Uses ApplicationDataContainer to save/load user preferences
    /// </summary>
    public class SettingsService
    {
        private static SettingsService _instance;
        // THEME PERSISTENCE: Reference to local settings storage for saving/loading theme preferences
        private readonly ApplicationDataContainer _localSettings;

        // THEME PERSISTENCE: Setting keys for ApplicationDataContainer storage
        private const string THEME_SETTING_KEY = "AppTheme";
        private const string NOTIFICATIONS_SETTING_KEY = "NotificationsEnabled";
        private const string AUTOSAVE_SETTING_KEY = "AutoSaveEnabled";

        public static SettingsService Instance => _instance ?? (_instance = new SettingsService());

        private SettingsService()
        {
            // THEME PERSISTENCE: Get reference to local settings container for saving/loading
            _localSettings = ApplicationData.Current.LocalSettings;
        }

        #region Theme Settings

        /// <summary>
        /// Gets or sets the current application theme
        /// Values: 0 = Default, 1 = Light, 2 = Dark
        /// </summary>
        public ElementTheme CurrentTheme
        {
            get
            {
                // THEME PERSISTENCE: Load theme from settings, default to system theme if not set
                var themeValue = _localSettings.Values[THEME_SETTING_KEY];
                if (themeValue != null && int.TryParse(themeValue.ToString(), out int theme))
                {
                    return (ElementTheme)theme;
                }
                return ElementTheme.Default; // System default
            }
            set
            {
                // THEME PERSISTENCE: Save theme to persistent storage
                _localSettings.Values[THEME_SETTING_KEY] = (int)value;
                
                // Apply theme immediately
                ApplyTheme(value);
                
                // Notify about theme change
                ThemeChanged?.Invoke(this, value);
                
                System.Diagnostics.Debug.WriteLine($"SettingsService: Theme changed to {value}");
            }
        }

        /// <summary>
        /// Gets the current theme as a boolean for toggle switch binding
        /// True = Dark theme, False = Light theme or Default
        /// </summary>
        public bool IsDarkTheme
        {
            // THEME PERSISTENCE: Convert theme enum to boolean for UI binding
            get => CurrentTheme == ElementTheme.Dark;
            // THEME PERSISTENCE: Convert boolean to theme enum and save via CurrentTheme property
            set => CurrentTheme = value ? ElementTheme.Dark : ElementTheme.Light;
        }

        #endregion

        #region Application Settings

        /// <summary>
        /// Gets or sets whether notifications are enabled
        /// </summary>
        public bool NotificationsEnabled
        {
            get
            {
                var value = _localSettings.Values[NOTIFICATIONS_SETTING_KEY];
                return value != null ? (bool)value : true; // Default: enabled
            }
            set
            {
                _localSettings.Values[NOTIFICATIONS_SETTING_KEY] = value;
                System.Diagnostics.Debug.WriteLine($"SettingsService: Notifications {(value ? "enabled" : "disabled")}");
            }
        }

        /// <summary>
        /// Gets or sets whether auto-save is enabled
        /// </summary>
        public bool AutoSaveEnabled
        {
            get
            {
                var value = _localSettings.Values[AUTOSAVE_SETTING_KEY];
                return value != null ? (bool)value : true; // Default: enabled
            }
            set
            {
                _localSettings.Values[AUTOSAVE_SETTING_KEY] = value;
                System.Diagnostics.Debug.WriteLine($"SettingsService: Auto-save {(value ? "enabled" : "disabled")}");
            }
        }

        #endregion

        #region Theme Management

        /// <summary>
        /// Event raised when theme changes
        /// </summary>
        public event EventHandler<ElementTheme> ThemeChanged;

        /// <summary>
        /// Apply the specified theme to the current window
        /// </summary>
        /// <param name="theme">Theme to apply</param>
        public void ApplyTheme(ElementTheme theme)
        {
            try
            {
                // THEME PERSISTENCE: Apply loaded/saved theme to the UI
                if (Window.Current?.Content is FrameworkElement rootElement)
                {
                    rootElement.RequestedTheme = theme;
                    System.Diagnostics.Debug.WriteLine($"SettingsService: Applied theme {theme} to window");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SettingsService: Error applying theme - {ex.Message}");
            }
        }

        /// <summary>
        /// Toggle between Light and Dark themes
        /// </summary>
        public void ToggleTheme()
        {
            // THEME PERSISTENCE: Toggle theme and automatically save via CurrentTheme setter
            CurrentTheme = CurrentTheme == ElementTheme.Dark ? ElementTheme.Light : ElementTheme.Dark;
        }

        /// <summary>
        /// Initialize theme on app startup
        /// </summary>
        public void InitializeTheme()
        {
            // THEME PERSISTENCE: Load and apply saved theme on app startup
            ApplyTheme(CurrentTheme);
            System.Diagnostics.Debug.WriteLine($"SettingsService: Initialized with theme {CurrentTheme}");
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Get a friendly name for the current theme
        /// </summary>
        public string GetThemeName()
        {
            // C# 7.3 compatible switch statement
            switch (CurrentTheme)
            {
                case ElementTheme.Light:
                    return "Light";
                case ElementTheme.Dark:
                    return "Dark";
                case ElementTheme.Default:
                    return "System Default";
                default:
                    return "Unknown";
            }
        }

        /// <summary>
        /// Reset all settings to defaults
        /// </summary>
        public void ResetToDefaults()
        {
            // THEME PERSISTENCE: Reset theme to default and save
            CurrentTheme = ElementTheme.Default;
            NotificationsEnabled = true;
            AutoSaveEnabled = true;
            
            System.Diagnostics.Debug.WriteLine("SettingsService: Reset all settings to defaults");
        }

        /// <summary>
        /// Get summary of current settings for debugging
        /// </summary>
        public string GetSettingsSummary()
        {
            return $"Theme: {GetThemeName()}, Notifications: {NotificationsEnabled}, AutoSave: {AutoSaveEnabled}";
        }

        #endregion
    }

    /// <summary>
    /// SUSPENSION & RESUME: Service for managing application suspension and resume state
    /// Handles app lifecycle events, saves timestamps, and manages welcome back messages
    /// Uses ApplicationDataContainer for persistent storage across app sessions
    /// </summary>
    public class SuspensionService
    {
        private static SuspensionService _instance;
        
        // SUSPENSION & RESUME: Reference to local settings storage for suspension state
        private readonly ApplicationDataContainer _localSettings;

        // SUSPENSION & RESUME: Setting keys for suspension state storage
        private const string LAST_SUSPENSION_TIME_KEY = "LastSuspensionTime";
        private const string LAST_RESUME_TIME_KEY = "LastResumeTime";
        private const string APP_LAUNCH_COUNT_KEY = "AppLaunchCount";
        private const string CURRENT_PAGE_KEY = "CurrentPage";
        private const string CUSTOMER_COUNT_KEY = "CustomerCountOnSuspension";
        private const string WAS_SUSPENDED_KEY = "WasSuspended";

        public static SuspensionService Instance => _instance ?? (_instance = new SuspensionService());

        private SuspensionService()
        {
            // SUSPENSION & RESUME: Get reference to local settings container for suspension state
            _localSettings = ApplicationData.Current.LocalSettings;
        }

        #region Suspension State Properties

        /// <summary>
        /// SUSPENSION & RESUME: Gets or sets the timestamp when app was last suspended
        /// </summary>
        public DateTime? LastSuspensionTime
        {
            get
            {
                var value = _localSettings.Values[LAST_SUSPENSION_TIME_KEY];
                if (value != null && DateTime.TryParse(value.ToString(), out DateTime time))
                {
                    return time;
                }
                return null;
            }
            private set
            {
                if (value.HasValue)
                {
                    _localSettings.Values[LAST_SUSPENSION_TIME_KEY] = value.Value.ToString("O"); // ISO 8601 format
                }
                else
                {
                    _localSettings.Values.Remove(LAST_SUSPENSION_TIME_KEY);
                }
            }
        }

        /// <summary>
        /// SUSPENSION & RESUME: Gets or sets the timestamp when app was last resumed
        /// </summary>
        public DateTime? LastResumeTime
        {
            get
            {
                var value = _localSettings.Values[LAST_RESUME_TIME_KEY];
                if (value != null && DateTime.TryParse(value.ToString(), out DateTime time))
                {
                    return time;
                }
                return null;
            }
            private set
            {
                if (value.HasValue)
                {
                    _localSettings.Values[LAST_RESUME_TIME_KEY] = value.Value.ToString("O");
                }
                else
                {
                    _localSettings.Values.Remove(LAST_RESUME_TIME_KEY);
                }
            }
        }

        /// <summary>
        /// SUSPENSION & RESUME: Gets or sets the number of times the app has been launched
        /// </summary>
        public int AppLaunchCount
        {
            get
            {
                var value = _localSettings.Values[APP_LAUNCH_COUNT_KEY];
                return value != null ? (int)value : 0;
            }
            private set
            {
                _localSettings.Values[APP_LAUNCH_COUNT_KEY] = value;
            }
        }

        /// <summary>
        /// SUSPENSION & RESUME: Gets or sets the current page name for state restoration
        /// </summary>
        public string CurrentPage
        {
            get
            {
                var value = _localSettings.Values[CURRENT_PAGE_KEY];
                return value?.ToString() ?? "HomePage";
            }
            set
            {
                _localSettings.Values[CURRENT_PAGE_KEY] = value ?? "HomePage";
            }
        }

        /// <summary>
        /// SUSPENSION & RESUME: Gets or sets the customer count when app was suspended
        /// </summary>
        public int CustomerCountOnSuspension
        {
            get
            {
                var value = _localSettings.Values[CUSTOMER_COUNT_KEY];
                return value != null ? (int)value : 0;
            }
            set
            {
                _localSettings.Values[CUSTOMER_COUNT_KEY] = value;
            }
        }

        /// <summary>
        /// SUSPENSION & RESUME: Gets or sets whether the app was previously suspended
        /// </summary>
        public bool WasSuspended
        {
            get
            {
                var value = _localSettings.Values[WAS_SUSPENDED_KEY];
                return value != null ? (bool)value : false;
            }
            private set
            {
                _localSettings.Values[WAS_SUSPENDED_KEY] = value;
            }
        }

        #endregion

        #region Suspension & Resume Operations

        /// <summary>
        /// SUSPENSION & RESUME: Save app state when suspending
        /// Records timestamp, current page, and app data for restoration
        /// </summary>
        public void SaveSuspensionState(string currentPage = null, int customerCount = 0)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("SUSPENSION & RESUME: Saving suspension state...");

                // SUSPENSION & RESUME: Record suspension timestamp
                LastSuspensionTime = DateTime.Now;
                WasSuspended = true;

                // SUSPENSION & RESUME: Save current app state
                if (!string.IsNullOrEmpty(currentPage))
                {
                    CurrentPage = currentPage;
                }
                CustomerCountOnSuspension = customerCount;

                System.Diagnostics.Debug.WriteLine($"SUSPENSION & RESUME: State saved - Page: {CurrentPage}, Customers: {customerCount}, Time: {LastSuspensionTime}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SUSPENSION & RESUME ERROR: Failed to save suspension state - {ex.Message}");
            }
        }

        /// <summary>
        /// SUSPENSION & RESUME: Handle app resuming from suspension
        /// Records resume timestamp and prepares welcome back message
        /// </summary>
        public void HandleResume()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("SUSPENSION & RESUME: Handling app resume...");

                // SUSPENSION & RESUME: Record resume timestamp
                LastResumeTime = DateTime.Now;

                System.Diagnostics.Debug.WriteLine($"SUSPENSION & RESUME: App resumed at {LastResumeTime}");

                // SUSPENSION & RESUME: Calculate time away if we have suspension time
                if (LastSuspensionTime.HasValue && LastResumeTime.HasValue)
                {
                    var timeAway = LastResumeTime.Value - LastSuspensionTime.Value;
                    System.Diagnostics.Debug.WriteLine($"SUSPENSION & RESUME: Time away: {timeAway}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SUSPENSION & RESUME ERROR: Failed to handle resume - {ex.Message}");
            }
        }

        /// <summary>
        /// SUSPENSION & RESUME: Handle app launch (fresh start or resume)
        /// Increments launch count and determines if this is a resume scenario
        /// </summary>
        public void HandleLaunch()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("SUSPENSION & RESUME: Handling app launch...");

                // SUSPENSION & RESUME: Increment launch counter
                AppLaunchCount++;

                System.Diagnostics.Debug.WriteLine($"SUSPENSION & RESUME: Launch #{AppLaunchCount}, Was previously suspended: {WasSuspended}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SUSPENSION & RESUME ERROR: Failed to handle launch - {ex.Message}");
            }
        }

        /// <summary>
        /// SUSPENSION & RESUME: Get welcome back message based on suspension state
        /// Enhanced with seconds-based counting for precise testing
        /// </summary>
        public string GetWelcomeBackMessage()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"SUSPENSION & RESUME: Getting welcome message - WasSuspended: {WasSuspended}, LastSuspensionTime: {LastSuspensionTime}");

                // SUSPENSION & RESUME: Always show welcome message on first launch
                if (AppLaunchCount == 1)
                {
                    return $"?? Welcome to Customer Management! (First time launch)";
                }

                // SUSPENSION & RESUME: Show launch count if no suspension data
                if (!WasSuspended || !LastSuspensionTime.HasValue)
                {
                    return $"?? Welcome back to Customer Management! (Launch #{AppLaunchCount})";
                }

                // SUSPENSION & RESUME: Calculate time away with precise seconds counting
                var timeAway = DateTime.Now - LastSuspensionTime.Value;
                System.Diagnostics.Debug.WriteLine($"SUSPENSION & RESUME: Time away calculated: {timeAway}");

                // SUSPENSION & RESUME: Enhanced time formatting with seconds precision
                if (timeAway.TotalSeconds < 10)
                {
                    return $"? Welcome back! You were away for {timeAway.Seconds} second(s). (Quick return!)";
                }
                else if (timeAway.TotalSeconds < 60)
                {
                    return $"?? Welcome back! You were away for {(int)timeAway.TotalSeconds} second(s).";
                }
                else if (timeAway.TotalMinutes < 1)
                {
                    return $"?? Welcome back! You were away for {timeAway.Seconds} second(s).";
                }
                else if (timeAway.TotalMinutes < 60)
                {
                    return $"? Welcome back! You were away for {(int)timeAway.TotalMinutes} minute(s) and {timeAway.Seconds} second(s).";
                }
                else if (timeAway.TotalHours < 24)
                {
                    return $"?? Welcome back! You were away for {(int)timeAway.TotalHours} hour(s), {timeAway.Minutes} minute(s), and {timeAway.Seconds} second(s).";
                }
                else
                {
                    return $"?? Welcome back! You were away for {timeAway.Days} day(s), {timeAway.Hours} hour(s), {timeAway.Minutes} minute(s), and {timeAway.Seconds} second(s).";
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SUSPENSION & RESUME ERROR: Failed to get welcome message - {ex.Message}");
                return "?? Welcome back to Customer Management!";
            }
        }

        /// <summary>
        /// SUSPENSION & RESUME: Get suspension state summary for debugging with seconds precision
        /// </summary>
        public string GetSuspensionSummary()
        {
            try
            {
                var summary = $"Launches: {AppLaunchCount}, " +
                       $"Last Suspended: {LastSuspensionTime?.ToString("MM/dd/yyyy HH:mm:ss") ?? "Never"}, " +
                       $"Last Resumed: {LastResumeTime?.ToString("MM/dd/yyyy HH:mm:ss") ?? "Never"}, " +
                       $"Was Suspended: {WasSuspended}, " +
                       $"Page: {CurrentPage}, " +
                       $"Customers: {CustomerCountOnSuspension}";

                // SUSPENSION & RESUME: Add time away calculation if available
                if (LastSuspensionTime.HasValue && LastResumeTime.HasValue)
                {
                    var timeAway = LastResumeTime.Value - LastSuspensionTime.Value;
                    summary += $", Time Away: {timeAway.Days}d {timeAway.Hours}h {timeAway.Minutes}m {timeAway.Seconds}s";
                }

                return summary;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SUSPENSION & RESUME ERROR: Failed to get summary - {ex.Message}");
                return "Suspension state unavailable";
            }
        }

        /// <summary>
        /// SUSPENSION & RESUME: Clear suspension flag after successful resume
        /// Modified to delay clearing for testing purposes
        /// </summary>
        public void ClearSuspensionFlag()
        {
            System.Diagnostics.Debug.WriteLine("SUSPENSION & RESUME: Clearing suspension flag after welcome message shown");
            WasSuspended = false;
        }

        /// <summary>
        /// SUSPENSION & RESUME: Force suspension state for testing purposes
        /// Useful for debugging welcome message functionality
        /// </summary>
        public void SetTestSuspensionState(int secondsAgo = 30)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"SUSPENSION & RESUME: Setting test suspension state ({secondsAgo} seconds ago)");

                LastSuspensionTime = DateTime.Now.AddSeconds(-secondsAgo);
                WasSuspended = true;
                CurrentPage = "HomePage";
                CustomerCountOnSuspension = 5;

                System.Diagnostics.Debug.WriteLine($"SUSPENSION & RESUME: Test state set - suspended {secondsAgo} seconds ago");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SUSPENSION & RESUME ERROR: Failed to set test state - {ex.Message}");
            }
        }
        #endregion
    }
}