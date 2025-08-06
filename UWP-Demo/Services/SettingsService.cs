using System;
using Windows.Storage;
using Windows.UI.Xaml;

namespace UWP_Demo.Services
{
    /// <summary>
    /// Service for managing application settings with persistent storage
    /// </summary>
    public class SettingsService
    {
        private static SettingsService _instance;
        private readonly ApplicationDataContainer _localSettings;

        private const string THEME_SETTING_KEY = "AppTheme";
        private const string NOTIFICATIONS_SETTING_KEY = "NotificationsEnabled";
        private const string AUTOSAVE_SETTING_KEY = "AutoSaveEnabled";

        public static SettingsService Instance => _instance ?? (_instance = new SettingsService());

        private SettingsService()
        {
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
                var themeValue = _localSettings.Values[THEME_SETTING_KEY];
                if (themeValue != null && int.TryParse(themeValue.ToString(), out int theme))
                {
                    return (ElementTheme)theme;
                }
                return ElementTheme.Default;
            }
            set
            {
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
        /// </summary>
        public bool IsDarkTheme
        {
            get => CurrentTheme == ElementTheme.Dark;
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
                return value == null || bool.Parse(value.ToString());
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
                return value == null || bool.Parse(value.ToString());
            }
            set
            {
                _localSettings.Values[AUTOSAVE_SETTING_KEY] = value;
                System.Diagnostics.Debug.WriteLine($"SettingsService: Auto-save {(value ? "enabled" : "disabled")}");
            }
        }

        #endregion

        #region Theme Management

        public event EventHandler<ElementTheme> ThemeChanged;

        /// <summary>
        /// Apply theme to the application
        /// </summary>
        public void ApplyTheme(ElementTheme theme)
        {
            try
            {
                if (Window.Current?.Content is FrameworkElement rootElement)
                {
                    rootElement.RequestedTheme = theme;
                    System.Diagnostics.Debug.WriteLine($"SettingsService: Applied theme {theme} to application");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SettingsService: Failed to apply theme - {ex.Message}");
            }
        }

        /// <summary>
        /// Get theme display name for UI
        /// </summary>
        public string GetThemeDisplayName(ElementTheme theme)
        {
            switch (theme)
            {
                case ElementTheme.Default:
                    return "System Default";
                case ElementTheme.Light:
                    return "Light";
                case ElementTheme.Dark:
                    return "Dark";
                default:
                    return "Unknown";
            }
        }

        /// <summary>
        /// Get current theme name
        /// </summary>
        public string GetThemeName()
        {
            return GetThemeDisplayName(CurrentTheme);
        }

        /// <summary>
        /// Initialize theme on app startup
        /// </summary>
        public void InitializeTheme()
        {
            try
            {
                ApplyTheme(CurrentTheme);
                System.Diagnostics.Debug.WriteLine($"SettingsService: Theme initialized to {GetThemeName()}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SettingsService: Failed to initialize theme - {ex.Message}");
            }
        }

        /// <summary>
        /// Toggle between light and dark theme
        /// </summary>
        public void ToggleTheme()
        {
            CurrentTheme = CurrentTheme == ElementTheme.Dark ? ElementTheme.Light : ElementTheme.Dark;
        }

        /// <summary>
        /// Reset settings to defaults
        /// </summary>
        public void ResetToDefaults()
        {
            CurrentTheme = ElementTheme.Default;
            NotificationsEnabled = true;
            AutoSaveEnabled = true;
        }

        /// <summary>
        /// Get settings summary for debugging
        /// </summary>
        public string GetSettingsSummary()
        {
            return $"Theme: {GetThemeName()}, Notifications: {NotificationsEnabled}, AutoSave: {AutoSaveEnabled}";
        }

        #endregion

        #region Initialization

        /// <summary>
        /// Initialize settings service and apply saved theme
        /// </summary>
        public void Initialize()
        {
            try
            {
                // Apply saved theme on startup
                ApplyTheme(CurrentTheme);
                System.Diagnostics.Debug.WriteLine($"SettingsService: Initialized with theme {CurrentTheme}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SettingsService: Initialization failed - {ex.Message}");
            }
        }

        #endregion
    }

    /// <summary>
    /// 6. Suspension & Resume Handling: Service for managing application suspension and resume state
    /// Features: Timestamp tracking, session counting, welcome messages, test functionality
    /// </summary>
    public class SuspensionService
    {
        private static SuspensionService _instance;
        private readonly ApplicationDataContainer _localSettings;
        
        private const string SUSPENSION_TIME_KEY = "SuspensionTime";
        private const string WAS_SUSPENDED_KEY = "WasSuspended";
        private const string LAST_PAGE_KEY = "LastPage";
        private const string SUSPENSION_COUNT_KEY = "SuspensionCount";
        private const string RESUME_COUNT_KEY = "ResumeCount";
        private const string SESSION_START_TIME_KEY = "SessionStartTime";

        public static SuspensionService Instance => _instance ?? (_instance = new SuspensionService());

        private SuspensionService()
        {
            _localSettings = ApplicationData.Current.LocalSettings;
        }

        public bool WasSuspended
        {
            get
            {
                var value = _localSettings.Values[WAS_SUSPENDED_KEY];
                return value != null && (bool)value;
            }
            private set => _localSettings.Values[WAS_SUSPENDED_KEY] = value;
        }

        /// <summary>
        /// Gets the total number of times the app has been suspended in this session
        /// </summary>
        public int SuspensionCount
        {
            get
            {
                var value = _localSettings.Values[SUSPENSION_COUNT_KEY];
                return value != null ? (int)value : 0;
            }
            private set => _localSettings.Values[SUSPENSION_COUNT_KEY] = value;
        }

        /// <summary>
        /// Gets the total number of times the app has been resumed in this session
        /// </summary>
        public int ResumeCount
        {
            get
            {
                var value = _localSettings.Values[RESUME_COUNT_KEY];
                return value != null ? (int)value : 0;
            }
            private set => _localSettings.Values[RESUME_COUNT_KEY] = value;
        }

        /// <summary>
        /// Gets the session start time
        /// </summary>
        public DateTime SessionStartTime
        {
            get
            {
                var value = _localSettings.Values[SESSION_START_TIME_KEY];
                if (value != null)
                {
                    return DateTime.FromBinary((long)value);
                }
                return DateTime.Now;
            }
            private set => _localSettings.Values[SESSION_START_TIME_KEY] = value.ToBinary();
        }

        public void HandleLaunch()
        {
            // Initialize session start time if not already set
            if (_localSettings.Values[SESSION_START_TIME_KEY] == null)
            {
                SessionStartTime = DateTime.Now;
                System.Diagnostics.Debug.WriteLine($"SuspensionService: New session started at {SessionStartTime:HH:mm:ss}");
            }
            
            System.Diagnostics.Debug.WriteLine("SuspensionService: App launched");
            System.Diagnostics.Debug.WriteLine($"SuspensionService: Session stats - Suspensions: {SuspensionCount}, Resumes: {ResumeCount}");
        }

        public void HandleResume()
        {
            // Increment resume counter
            ResumeCount++;
            
            // Don't clear the WasSuspended flag immediately
            // Let the UI handle clearing it when the user dismisses the welcome message
            System.Diagnostics.Debug.WriteLine($"SuspensionService: App resumed (Resume #{ResumeCount}) - keeping suspension flag for welcome message");
        }

        public void SaveSuspensionState(string currentPage, int customerCount)
        {
            try
            {
                // Increment suspension counter
                SuspensionCount++;
                
                System.Diagnostics.Debug.WriteLine("=== ?? STARTING APP MINIMIZATION/SUSPENSION ===");
                System.Diagnostics.Debug.WriteLine($"? Timestamp: {DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}");
                System.Diagnostics.Debug.WriteLine($"?? Suspension Count: #{SuspensionCount} (this session)");
                System.Diagnostics.Debug.WriteLine($"?? Current Page: {currentPage}");
                System.Diagnostics.Debug.WriteLine($"?? Session Duration: {DateTime.Now - SessionStartTime:hh\\:mm\\:ss}");
                
                _localSettings.Values[SUSPENSION_TIME_KEY] = DateTime.Now.ToBinary();
                _localSettings.Values[LAST_PAGE_KEY] = currentPage;
                WasSuspended = true;
                
                System.Diagnostics.Debug.WriteLine($"?? Suspension state saved successfully");
                System.Diagnostics.Debug.WriteLine("=== ?? SUSPENSION PROCESS COMPLETED ===");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"? SuspensionService: Failed to save suspension state - {ex.Message}");
            }
        }

        public string GetWelcomeBackMessage()
        {
            if (WasSuspended)
            {
                try
                {
                    var suspensionTimeValue = _localSettings.Values[SUSPENSION_TIME_KEY];
                    if (suspensionTimeValue != null)
                    {
                        var suspensionTime = DateTime.FromBinary((long)suspensionTimeValue);
                        var timeAway = DateTime.Now - suspensionTime;
                        
                        // Build time message
                        string timeMessage = "";
                        if (timeAway.TotalSeconds < 60)
                        {
                            int seconds = (int)timeAway.TotalSeconds;
                            timeMessage = $"{seconds} second{(seconds != 1 ? "s" : "")}";
                        }
                        else if (timeAway.TotalMinutes < 60)
                        {
                            int minutes = (int)timeAway.TotalMinutes;
                            int seconds = (int)(timeAway.TotalSeconds % 60);
                            
                            if (seconds > 0)
                            {
                                timeMessage = $"{minutes} minute{(minutes != 1 ? "s" : "")} and {seconds} second{(seconds != 1 ? "s" : "")}";
                            }
                            else
                            {
                                timeMessage = $"{minutes} minute{(minutes != 1 ? "s" : "")}";
                            }
                        }
                        else if (timeAway.TotalHours < 24)
                        {
                            int hours = (int)timeAway.TotalHours;
                            int minutes = (int)(timeAway.TotalMinutes % 60);
                            
                            if (minutes > 0)
                            {
                                timeMessage = $"{hours} hour{(hours != 1 ? "s" : "")} and {minutes} minute{(minutes != 1 ? "s" : "")}";
                            }
                            else
                            {
                                timeMessage = $"{hours} hour{(hours != 1 ? "s" : "")}";
                            }
                        }
                        else
                        {
                            int days = (int)timeAway.TotalDays;
                            int hours = (int)(timeAway.TotalHours % 24);
                            
                            if (hours > 0)
                            {
                                timeMessage = $"{days} day{(days != 1 ? "s" : "")} and {hours} hour{(hours != 1 ? "s" : "")}";
                            }
                            else
                            {
                                timeMessage = $"{days} day{(days != 1 ? "s" : "")}";
                            }
                        }
                        
                        // Include session statistics in the welcome message
                        string sessionInfo = "";
                        if (SuspensionCount > 1)
                        {
                            sessionInfo = $" (#{ResumeCount} return this session)";
                        }
                        
                        return $"Welcome back! You were away for {timeMessage}.{sessionInfo}";
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"SuspensionService: Error calculating time away - {ex.Message}");
                }
                
                return "Welcome back to Customer Management!";
            }
            return ""; // Don't show welcome message on first launch
        }

        public void ClearSuspensionFlag()
        {
            WasSuspended = false;
            System.Diagnostics.Debug.WriteLine($"SuspensionService: Suspension flag cleared by user (Total resumes this session: {ResumeCount})");
        }

        public string GetSuspensionSummary()
        {
            var sessionDuration = DateTime.Now - SessionStartTime;
            return $"Suspended: {WasSuspended}, Count: {SuspensionCount}, Resumes: {ResumeCount}, Session: {sessionDuration:hh\\:mm\\:ss}";
        }

        public void SetTestSuspensionState(int secondsAgo)
        {
            var testTime = DateTime.Now.AddSeconds(-secondsAgo);
            _localSettings.Values[SUSPENSION_TIME_KEY] = testTime.ToBinary();
            WasSuspended = true;
            
            // Don't increment the real counter for test scenarios
            System.Diagnostics.Debug.WriteLine($"SuspensionService: Set TEST suspension state {secondsAgo} seconds ago (not counted in session stats)");
        }

        /// <summary>
        /// Reset session counters (useful for testing or new sessions)
        /// </summary>
        public void ResetSessionCounters()
        {
            SuspensionCount = 0;
            ResumeCount = 0;
            SessionStartTime = DateTime.Now;
            System.Diagnostics.Debug.WriteLine("SuspensionService: Session counters reset");
        }

        /// <summary>
        /// Get detailed debugging information about suspension state
        /// </summary>
        public string GetDetailedDebugInfo()
        {
            var sessionDuration = DateTime.Now - SessionStartTime;
            var lastSuspensionTime = "Never";
            
            try
            {
                var suspensionTimeValue = _localSettings.Values[SUSPENSION_TIME_KEY];
                if (suspensionTimeValue != null)
                {
                    var suspensionTime = DateTime.FromBinary((long)suspensionTimeValue);
                    lastSuspensionTime = suspensionTime.ToString("HH:mm:ss");
                }
            }
            catch { }
            
            return $"Session Start: {SessionStartTime:HH:mm:ss}\n" +
                   $"Session Duration: {sessionDuration:hh\\:mm\\:ss}\n" +
                   $"Suspensions: {SuspensionCount}\n" +
                   $"Resumes: {ResumeCount}\n" +
                   $"Currently Suspended: {WasSuspended}\n" +
                   $"Last Suspension: {lastSuspensionTime}";
        }
    }
}