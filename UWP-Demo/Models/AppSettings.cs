using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using Windows.UI.Xaml;

namespace UWP_Demo.Models
{
    /// <summary>
    /// Represents the application settings that can be configured by the user.
    /// This class manages theme preferences, user preferences, and other application state
    /// that should persist across app sessions using ApplicationDataContainer.
    /// </summary>
    /// <remarks>
    /// This model demonstrates:
    /// - Application settings persistence using Windows.Storage
    /// - Theme management for Light/Dark mode switching
    /// - Property change notification for settings UI
    /// - Type-safe enum usage for theme selection
    /// - Default value handling for settings
    /// </remarks>
    public class AppSettings : INotifyPropertyChanged
    {
        #region Private Fields

        private ElementTheme _currentTheme;
        private string _lastSelectedCustomerId;
        private DateTime _lastAppLaunch;
        private bool _isFirstRun;
        private string _userPreferredLanguage;
        private bool _enableNotifications;
        private bool _autoSaveData;
        private int _autoSaveIntervalMinutes;

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the current application theme (Light, Dark, or Default).
        /// Default means the app will follow the system theme setting.
        /// </summary>
        /// <remarks>
        /// This property is bound to the theme toggle in the Settings page.
        /// When changed, it triggers an application-wide theme update.
        /// </remarks>
        [JsonProperty("currentTheme")]
        public ElementTheme CurrentTheme
        {
            get => _currentTheme;
            set => SetProperty(ref _currentTheme, value);
        }

        /// <summary>
        /// Gets or sets the ID of the last selected customer.
        /// This is used to restore the user's selection when navigating between pages
        /// or when the app is resumed from suspension.
        /// </summary>
        [JsonProperty("lastSelectedCustomerId")]
        public string LastSelectedCustomerId
        {
            get => _lastSelectedCustomerId;
            set => SetProperty(ref _lastSelectedCustomerId, value);
        }

        /// <summary>
        /// Gets or sets the timestamp of the last app launch.
        /// This is used to display "Welcome back" messages and track app usage patterns.
        /// </summary>
        [JsonProperty("lastAppLaunch")]
        public DateTime LastAppLaunch
        {
            get => _lastAppLaunch;
            set => SetProperty(ref _lastAppLaunch, value);
        }

        /// <summary>
        /// Gets or sets whether this is the first time the app is being run.
        /// This flag is used to show welcome messages or onboarding experiences.
        /// </summary>
        [JsonProperty("isFirstRun")]
        public bool IsFirstRun
        {
            get => _isFirstRun;
            set => SetProperty(ref _isFirstRun, value);
        }

        /// <summary>
        /// Gets or sets the user's preferred language code (e.g., "en-US", "fr-FR").
        /// This could be used for future localization features.
        /// </summary>
        [JsonProperty("userPreferredLanguage")]
        public string UserPreferredLanguage
        {
            get => _userPreferredLanguage;
            set => SetProperty(ref _userPreferredLanguage, value);
        }

        /// <summary>
        /// Gets or sets whether the user wants to receive notifications.
        /// This setting controls whether the app will show toast notifications
        /// or other user alerts.
        /// </summary>
        [JsonProperty("enableNotifications")]
        public bool EnableNotifications
        {
            get => _enableNotifications;
            set => SetProperty(ref _enableNotifications, value);
        }

        /// <summary>
        /// Gets or sets whether the app should automatically save data changes.
        /// When enabled, customer data is saved automatically at regular intervals
        /// instead of requiring manual save operations.
        /// </summary>
        [JsonProperty("autoSaveData")]
        public bool AutoSaveData
        {
            get => _autoSaveData;
            set => SetProperty(ref _autoSaveData, value);
        }

        /// <summary>
        /// Gets or sets the interval (in minutes) for automatic data saving.
        /// This only applies when AutoSaveData is enabled.
        /// Valid range is 1-60 minutes.
        /// </summary>
        [JsonProperty("autoSaveIntervalMinutes")]
        public int AutoSaveIntervalMinutes
        {
            get => _autoSaveIntervalMinutes;
            set => SetProperty(ref _autoSaveIntervalMinutes, Math.Max(1, Math.Min(60, value)));
        }

        #endregion

        #region Computed Properties

        /// <summary>
        /// Gets a user-friendly string representation of the current theme.
        /// This is used in the UI to display the current theme selection.
        /// </summary>
        [JsonIgnore]
        public string CurrentThemeDisplayName
        {
            get
            {
                return CurrentTheme switch
                {
                    ElementTheme.Light => "Light",
                    ElementTheme.Dark => "Dark",
                    ElementTheme.Default => "System Default",
                    _ => "Unknown"
                };
            }
        }

        /// <summary>
        /// Gets whether the app has been launched before (not a first run).
        /// This is a convenience property that's the inverse of IsFirstRun.
        /// </summary>
        [JsonIgnore]
        public bool HasLaunchedBefore => !IsFirstRun;

        /// <summary>
        /// Gets the time elapsed since the last app launch.
        /// This can be used to display "Last used X hours ago" messages.
        /// </summary>
        [JsonIgnore]
        public TimeSpan TimeSinceLastLaunch => DateTime.Now - LastAppLaunch;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the AppSettings class with default values.
        /// These defaults represent sensible choices for new users.
        /// </summary>
        public AppSettings()
        {
            // Set default theme to follow system setting
            CurrentTheme = ElementTheme.Default;
            
            // Initialize to empty/default values
            LastSelectedCustomerId = string.Empty;
            LastAppLaunch = DateTime.Now;
            IsFirstRun = true;
            
            // Set default language to system language
            UserPreferredLanguage = Windows.System.UserProfile.GlobalizationPreferences.Languages[0];
            
            // Enable notifications by default
            EnableNotifications = true;
            
            // Enable auto-save with 5-minute interval
            AutoSaveData = true;
            AutoSaveIntervalMinutes = 5;
        }

        #endregion

        #region Settings Management Methods

        /// <summary>
        /// Resets all settings to their default values.
        /// This can be called from a "Reset to Defaults" button in the settings UI.
        /// </summary>
        public void ResetToDefaults()
        {
            CurrentTheme = ElementTheme.Default;
            LastSelectedCustomerId = string.Empty;
            EnableNotifications = true;
            AutoSaveData = true;
            AutoSaveIntervalMinutes = 5;
            UserPreferredLanguage = Windows.System.UserProfile.GlobalizationPreferences.Languages[0];
            
            // Note: We don't reset LastAppLaunch or IsFirstRun as these are historical data
        }

        /// <summary>
        /// Validates that all settings have valid values.
        /// This should be called after loading settings from storage to ensure data integrity.
        /// </summary>
        /// <returns>True if all settings are valid, false if any need to be reset</returns>
        public bool ValidateSettings()
        {
            // Ensure theme is a valid enum value
            if (!Enum.IsDefined(typeof(ElementTheme), CurrentTheme))
            {
                CurrentTheme = ElementTheme.Default;
                return false;
            }

            // Ensure auto-save interval is within valid range
            if (AutoSaveIntervalMinutes < 1 || AutoSaveIntervalMinutes > 60)
            {
                AutoSaveIntervalMinutes = 5;
                return false;
            }

            // Ensure language code is not null or empty
            if (string.IsNullOrWhiteSpace(UserPreferredLanguage))
            {
                UserPreferredLanguage = "en-US";
                return false;
            }

            return true;
        }

        /// <summary>
        /// Updates the last app launch timestamp to the current time.
        /// This should be called during app startup to track usage.
        /// </summary>
        public void UpdateLastLaunchTime()
        {
            LastAppLaunch = DateTime.Now;
            
            // If this was a first run, mark it as no longer first run
            if (IsFirstRun)
            {
                IsFirstRun = false;
            }
        }

        #endregion

        #region Theme Helper Methods

        /// <summary>
        /// Cycles to the next theme in the sequence: Default -> Light -> Dark -> Default
        /// This can be used for a "cycle theme" button or keyboard shortcut.
        /// </summary>
        public void CycleTheme()
        {
            CurrentTheme = CurrentTheme switch
            {
                ElementTheme.Default => ElementTheme.Light,
                ElementTheme.Light => ElementTheme.Dark,
                ElementTheme.Dark => ElementTheme.Default,
                _ => ElementTheme.Default
            };
        }

        /// <summary>
        /// Sets the theme to the opposite of the current theme (Light <-> Dark).
        /// If currently using Default, switches to Dark theme.
        /// </summary>
        public void ToggleTheme()
        {
            CurrentTheme = CurrentTheme switch
            {
                ElementTheme.Light => ElementTheme.Dark,
                ElementTheme.Dark => ElementTheme.Light,
                ElementTheme.Default => ElementTheme.Dark,
                _ => ElementTheme.Light
            };
        }

        #endregion

        #region INotifyPropertyChanged Implementation

        /// <summary>
        /// Event raised when a property value changes.
        /// This enables data binding in XAML controls to automatically update
        /// when settings are modified.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Sets the property value and raises PropertyChanged event if the value changed.
        /// This method ensures that bound UI controls are automatically updated.
        /// </summary>
        /// <typeparam name="T">The type of the property being set</typeparam>
        /// <param name="field">Reference to the backing field</param>
        /// <param name="value">The new value to set</param>
        /// <param name="propertyName">Name of the property (automatically provided)</param>
        /// <returns>True if the property value was changed</returns>
        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (Equals(field, value))
                return false;

            field = value;
            OnPropertyChanged(propertyName);
            
            // Notify related computed properties when base properties change
            if (propertyName == nameof(CurrentTheme))
            {
                OnPropertyChanged(nameof(CurrentThemeDisplayName));
            }
            else if (propertyName == nameof(IsFirstRun))
            {
                OnPropertyChanged(nameof(HasLaunchedBefore));
            }
            else if (propertyName == nameof(LastAppLaunch))
            {
                OnPropertyChanged(nameof(TimeSinceLastLaunch));
            }
            
            return true;
        }

        /// <summary>
        /// Raises the PropertyChanged event for the specified property.
        /// This notifies any bound UI controls that the property value has changed.
        /// </summary>
        /// <param name="propertyName">The name of the property that changed</param>
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        #region Object Overrides

        /// <summary>
        /// Returns a string representation of the settings for debugging purposes.
        /// </summary>
        /// <returns>A formatted string with key setting values</returns>
        public override string ToString()
        {
            return $"AppSettings: Theme={CurrentTheme}, AutoSave={AutoSaveData}, " +
                   $"FirstRun={IsFirstRun}, LastLaunch={LastAppLaunch:yyyy-MM-dd HH:mm}";
        }

        #endregion
    }
}