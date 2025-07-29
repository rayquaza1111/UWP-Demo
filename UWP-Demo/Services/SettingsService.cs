using System;
using System.Threading.Tasks;
using UWP_Demo.Models;
using Windows.Storage;
using Windows.UI.Xaml;
using Newtonsoft.Json;

namespace UWP_Demo.Services
{
    /// <summary>
    /// Service class responsible for managing application settings persistence using
    /// Windows ApplicationDataContainer for local settings storage.
    /// This service demonstrates UWP settings management and theme handling.
    /// </summary>
    /// <remarks>
    /// This service demonstrates several important UWP concepts:
    /// - ApplicationDataContainer for persistent settings storage
    /// - Theme management with automatic UI updates
    /// - Singleton pattern for consistent settings access
    /// - Event-driven architecture for settings changes
    /// - Default value handling and settings validation
    /// - JSON serialization for complex settings objects
    /// 
    /// The service uses ApplicationDataContainer which automatically handles:
    /// - Cross-device settings synchronization (when enabled)
    /// - Backup and restore during Windows updates
    /// - Automatic persistence without explicit save operations
    /// </remarks>
    public class SettingsService
    {
        #region Private Fields

        /// <summary>
        /// The singleton instance of the SettingsService.
        /// </summary>
        private static SettingsService _instance;

        /// <summary>
        /// Lock object for thread-safe singleton access.
        /// </summary>
        private static readonly object _lock = new object();

        /// <summary>
        /// The local settings container provided by UWP for persistent storage.
        /// This automatically handles data persistence and cross-device sync.
        /// </summary>
        private readonly ApplicationDataContainer _localSettings;

        /// <summary>
        /// The current application settings instance.
        /// This is cached in memory for performance and change tracking.
        /// </summary>
        private AppSettings _currentSettings;

        /// <summary>
        /// Key names for storing settings in ApplicationDataContainer.
        /// Using constants prevents typos and makes refactoring easier.
        /// </summary>
        private const string SETTINGS_KEY = "AppSettings";
        private const string THEME_KEY = "CurrentTheme";
        private const string LAST_CUSTOMER_KEY = "LastSelectedCustomerId";
        private const string LAST_LAUNCH_KEY = "LastAppLaunch";
        private const string FIRST_RUN_KEY = "IsFirstRun";
        private const string NOTIFICATIONS_KEY = "EnableNotifications";
        private const string AUTO_SAVE_KEY = "AutoSaveData";
        private const string AUTO_SAVE_INTERVAL_KEY = "AutoSaveIntervalMinutes";

        #endregion

        #region Events

        /// <summary>
        /// Event raised when the application theme changes.
        /// This allows UI components to respond to theme changes automatically.
        /// </summary>
        /// <remarks>
        /// Subscribe to this event to perform actions when the theme changes,
        /// such as updating custom UI elements that don't automatically
        /// respond to theme changes.
        /// </remarks>
        /// <example>
        /// SettingsService.Instance.ThemeChanged += (sender, theme) =>
        /// {
        ///     UpdateCustomControls(theme);
        /// };
        /// </example>
        public event EventHandler<ElementTheme> ThemeChanged;

        /// <summary>
        /// Event raised when any application setting changes.
        /// This provides a centralized way to respond to settings changes.
        /// </summary>
        public event EventHandler<AppSettings> SettingsChanged;

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the singleton instance of the SettingsService.
        /// Creates the instance if it doesn't exist (thread-safe).
        /// </summary>
        public static SettingsService Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new SettingsService();
                        }
                    }
                }
                return _instance;
            }
        }

        /// <summary>
        /// Gets the current application settings.
        /// This property loads settings from storage if not already cached.
        /// </summary>
        /// <remarks>
        /// The settings are cached in memory for performance. The first access
        /// loads from ApplicationDataContainer, subsequent accesses use the cache.
        /// Settings are automatically saved when modified through this service.
        /// </remarks>
        public AppSettings CurrentSettings
        {
            get
            {
                if (_currentSettings == null)
                {
                    LoadSettings();
                }
                return _currentSettings;
            }
            private set
            {
                _currentSettings = value;
                SaveSettings();
                SettingsChanged?.Invoke(this, value);
            }
        }

        /// <summary>
        /// Gets or sets the current application theme.
        /// Changing this property automatically updates the UI and saves the setting.
        /// </summary>
        /// <remarks>
        /// This property provides a convenient way to get/set the theme without
        /// accessing the full settings object. It automatically applies the theme
        /// to the current window and raises the ThemeChanged event.
        /// </remarks>
        /// <example>
        /// // Change to dark theme
        /// SettingsService.Instance.CurrentTheme = ElementTheme.Dark;
        /// </example>
        public ElementTheme CurrentTheme
        {
            get => CurrentSettings.CurrentTheme;
            set
            {
                if (CurrentSettings.CurrentTheme != value)
                {
                    CurrentSettings.CurrentTheme = value;
                    ApplyTheme(value);
                    SaveSettings();
                    ThemeChanged?.Invoke(this, value);
                }
            }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Private constructor for singleton pattern.
        /// Initializes the settings container and loads current settings.
        /// </summary>
        private SettingsService()
        {
            // Get reference to the local settings container
            _localSettings = ApplicationData.Current.LocalSettings;
            
            // Load settings immediately to ensure they're available
            LoadSettings();
        }

        #endregion

        #region Settings Loading and Saving

        /// <summary>
        /// Loads application settings from ApplicationDataContainer.
        /// If no settings exist, creates default settings.
        /// </summary>
        /// <remarks>
        /// This method first tries to load a complete settings object from JSON.
        /// If that fails (due to version changes or corruption), it falls back
        /// to loading individual settings values with default fallbacks.
        /// </remarks>
        private void LoadSettings()
        {
            try
            {
                // Try to load complete settings object first (newer format)
                if (_localSettings.Values.TryGetValue(SETTINGS_KEY, out object settingsJson) && 
                    settingsJson is string jsonString && 
                    !string.IsNullOrWhiteSpace(jsonString))
                {
                    try
                    {
                        _currentSettings = JsonConvert.DeserializeObject<AppSettings>(jsonString);
                        
                        // Validate loaded settings
                        if (_currentSettings != null && _currentSettings.ValidateSettings())
                        {
                            System.Diagnostics.Debug.WriteLine("SettingsService: Loaded settings from JSON");
                            return;
                        }
                    }
                    catch (JsonException ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"SettingsService: JSON deserialization error: {ex.Message}");
                    }
                }

                // Fall back to loading individual settings (older format or first run)
                _currentSettings = new AppSettings();
                LoadIndividualSettings();

                System.Diagnostics.Debug.WriteLine("SettingsService: Loaded individual settings or created defaults");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SettingsService: Error loading settings: {ex.Message}");
                
                // If all else fails, create default settings
                _currentSettings = new AppSettings();
            }

            // Ensure settings are saved in the new format
            SaveSettings();
        }

        /// <summary>
        /// Loads individual settings from ApplicationDataContainer for backward compatibility.
        /// This method provides fallback when JSON loading fails.
        /// </summary>
        private void LoadIndividualSettings()
        {
            // Load theme setting
            if (_localSettings.Values.TryGetValue(THEME_KEY, out object themeValue) && 
                themeValue is int themeInt && 
                Enum.IsDefined(typeof(ElementTheme), themeInt))
            {
                _currentSettings.CurrentTheme = (ElementTheme)themeInt;
            }

            // Load last selected customer
            if (_localSettings.Values.TryGetValue(LAST_CUSTOMER_KEY, out object customerValue) && 
                customerValue is string customerString)
            {
                _currentSettings.LastSelectedCustomerId = customerString;
            }

            // Load last launch time
            if (_localSettings.Values.TryGetValue(LAST_LAUNCH_KEY, out object launchValue) && 
                launchValue is string launchString && 
                DateTime.TryParse(launchString, out DateTime launchTime))
            {
                _currentSettings.LastAppLaunch = launchTime;
            }

            // Load first run flag
            if (_localSettings.Values.TryGetValue(FIRST_RUN_KEY, out object firstRunValue) && 
                firstRunValue is bool firstRunBool)
            {
                _currentSettings.IsFirstRun = firstRunBool;
            }

            // Load notifications setting
            if (_localSettings.Values.TryGetValue(NOTIFICATIONS_KEY, out object notificationsValue) && 
                notificationsValue is bool notificationsBool)
            {
                _currentSettings.EnableNotifications = notificationsBool;
            }

            // Load auto-save setting
            if (_localSettings.Values.TryGetValue(AUTO_SAVE_KEY, out object autoSaveValue) && 
                autoSaveValue is bool autoSaveBool)
            {
                _currentSettings.AutoSaveData = autoSaveBool;
            }

            // Load auto-save interval
            if (_localSettings.Values.TryGetValue(AUTO_SAVE_INTERVAL_KEY, out object intervalValue) && 
                intervalValue is int intervalInt)
            {
                _currentSettings.AutoSaveIntervalMinutes = intervalInt;
            }
        }

        /// <summary>
        /// Saves the current settings to ApplicationDataContainer.
        /// Uses JSON serialization for the complete settings object.
        /// </summary>
        /// <remarks>
        /// This method saves settings both as a complete JSON object (for future loads)
        /// and as individual values (for backward compatibility and cross-platform access).
        /// The ApplicationDataContainer automatically handles persistence.
        /// </remarks>
        public void SaveSettings()
        {
            if (_currentSettings == null)
                return;

            try
            {
                // Save complete settings as JSON
                string settingsJson = JsonConvert.SerializeObject(_currentSettings, Formatting.Indented);
                _localSettings.Values[SETTINGS_KEY] = settingsJson;

                // Also save individual values for backward compatibility and easier access
                _localSettings.Values[THEME_KEY] = (int)_currentSettings.CurrentTheme;
                _localSettings.Values[LAST_CUSTOMER_KEY] = _currentSettings.LastSelectedCustomerId ?? string.Empty;
                _localSettings.Values[LAST_LAUNCH_KEY] = _currentSettings.LastAppLaunch.ToString("O"); // ISO 8601 format
                _localSettings.Values[FIRST_RUN_KEY] = _currentSettings.IsFirstRun;
                _localSettings.Values[NOTIFICATIONS_KEY] = _currentSettings.EnableNotifications;
                _localSettings.Values[AUTO_SAVE_KEY] = _currentSettings.AutoSaveData;
                _localSettings.Values[AUTO_SAVE_INTERVAL_KEY] = _currentSettings.AutoSaveIntervalMinutes;

                System.Diagnostics.Debug.WriteLine("SettingsService: Settings saved successfully");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SettingsService: Error saving settings: {ex.Message}");
            }
        }

        #endregion

        #region Theme Management

        /// <summary>
        /// Applies the specified theme to the current application window.
        /// This method updates the visual appearance of all UI elements.
        /// </summary>
        /// <param name="theme">The theme to apply</param>
        /// <remarks>
        /// This method immediately applies the theme to the current window.
        /// The theme change affects all UI elements that use theme-aware resources.
        /// Custom controls may need to respond to the ThemeChanged event for updates.
        /// </remarks>
        /// <example>
        /// // Apply dark theme immediately
        /// SettingsService.Instance.ApplyTheme(ElementTheme.Dark);
        /// </example>
        public void ApplyTheme(ElementTheme theme)
        {
            try
            {
                // Apply theme to the current window's content
                if (Window.Current?.Content is FrameworkElement rootElement)
                {
                    rootElement.RequestedTheme = theme;
                    System.Diagnostics.Debug.WriteLine($"SettingsService: Applied theme: {theme}");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("SettingsService: No window content available for theme application");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SettingsService: Error applying theme: {ex.Message}");
            }
        }

        /// <summary>
        /// Toggles between Light and Dark themes.
        /// If currently using Default, switches to Dark theme.
        /// </summary>
        /// <remarks>
        /// This method provides a convenient way to implement theme toggle buttons.
        /// It automatically saves the new theme setting and applies it immediately.
        /// </remarks>
        /// <example>
        /// // Toggle theme on button click
        /// SettingsService.Instance.ToggleTheme();
        /// </example>
        public void ToggleTheme()
        {
            ElementTheme newTheme = CurrentTheme switch
            {
                ElementTheme.Light => ElementTheme.Dark,
                ElementTheme.Dark => ElementTheme.Light,
                ElementTheme.Default => ElementTheme.Dark,
                _ => ElementTheme.Light
            };

            CurrentTheme = newTheme;
        }

        /// <summary>
        /// Cycles through all available themes: Default -> Light -> Dark -> Default.
        /// This provides an alternative to toggle for applications with three theme options.
        /// </summary>
        public void CycleTheme()
        {
            ElementTheme newTheme = CurrentTheme switch
            {
                ElementTheme.Default => ElementTheme.Light,
                ElementTheme.Light => ElementTheme.Dark,
                ElementTheme.Dark => ElementTheme.Default,
                _ => ElementTheme.Default
            };

            CurrentTheme = newTheme;
        }

        #endregion

        #region App Lifecycle Methods

        /// <summary>
        /// Updates the last app launch timestamp and handles first-run logic.
        /// This should be called during application startup.
        /// </summary>
        /// <remarks>
        /// This method:
        /// - Updates the last launch timestamp for "welcome back" messages
        /// - Marks the app as no longer first-run after the initial launch
        /// - Automatically saves the updated settings
        /// </remarks>
        /// <example>
        /// // In App.xaml.cs OnLaunched method
        /// SettingsService.Instance.UpdateAppLaunchTime();
        /// </example>
        public void UpdateAppLaunchTime()
        {
            try
            {
                var settings = CurrentSettings;
                settings.UpdateLastLaunchTime();
                SaveSettings();
                
                System.Diagnostics.Debug.WriteLine($"SettingsService: Updated app launch time. First run: {settings.IsFirstRun}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SettingsService: Error updating launch time: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets a welcome message based on the app usage history.
        /// Returns different messages for first-time users vs. returning users.
        /// </summary>
        /// <returns>A personalized welcome message</returns>
        /// <example>
        /// // Display welcome message in the UI
        /// string welcomeMessage = SettingsService.Instance.GetWelcomeMessage();
        /// WelcomeTextBlock.Text = welcomeMessage;
        /// </example>
        public string GetWelcomeMessage()
        {
            var settings = CurrentSettings;
            
            if (settings.IsFirstRun)
            {
                return "Welcome to UWP Demo! This app showcases modern Windows development features.";
            }
            else
            {
                var timeSinceLastLaunch = settings.TimeSinceLastLaunch;
                
                if (timeSinceLastLaunch.TotalDays >= 7)
                {
                    return $"Welcome back! It's been {(int)timeSinceLastLaunch.TotalDays} days since your last visit.";
                }
                else if (timeSinceLastLaunch.TotalDays >= 1)
                {
                    return $"Welcome back! You last used the app {(int)timeSinceLastLaunch.TotalDays} day(s) ago.";
                }
                else if (timeSinceLastLaunch.TotalHours >= 1)
                {
                    return $"Welcome back! You last used the app {(int)timeSinceLastLaunch.TotalHours} hour(s) ago.";
                }
                else
                {
                    return "Welcome back!";
                }
            }
        }

        #endregion

        #region State Management

        /// <summary>
        /// Stores the ID of the currently selected customer for state restoration.
        /// This allows the app to restore the user's selection after suspension/resume.
        /// </summary>
        /// <param name="customerId">The ID of the selected customer</param>
        /// <example>
        /// // Save selection when customer is selected
        /// SettingsService.Instance.SetSelectedCustomerId(customer.Id.ToString());
        /// </example>
        public void SetSelectedCustomerId(string customerId)
        {
            try
            {
                CurrentSettings.LastSelectedCustomerId = customerId ?? string.Empty;
                SaveSettings();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SettingsService: Error setting selected customer ID: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets the ID of the last selected customer for state restoration.
        /// </summary>
        /// <returns>The customer ID, or null if none was selected</returns>
        /// <example>
        /// // Restore selection on app startup
        /// string lastCustomerId = SettingsService.Instance.GetSelectedCustomerId();
        /// if (!string.IsNullOrEmpty(lastCustomerId))
        /// {
        ///     RestoreCustomerSelection(lastCustomerId);
        /// }
        /// </example>
        public string GetSelectedCustomerId()
        {
            return CurrentSettings.LastSelectedCustomerId;
        }

        /// <summary>
        /// Clears the stored customer selection.
        /// Useful when the customer list is cleared or the selection is no longer valid.
        /// </summary>
        public void ClearSelectedCustomerId()
        {
            SetSelectedCustomerId(null);
        }

        #endregion

        #region Settings Reset and Management

        /// <summary>
        /// Resets all settings to their default values.
        /// This can be used for a "Reset to Defaults" feature in the settings UI.
        /// </summary>
        /// <param name="preserveAppHistory">
        /// If true, preserves app launch history and first-run status.
        /// If false, resets everything including usage history.
        /// </param>
        /// <example>
        /// // Reset user preferences but keep usage history
        /// SettingsService.Instance.ResetToDefaults(preserveAppHistory: true);
        /// </example>
        public void ResetToDefaults(bool preserveAppHistory = true)
        {
            try
            {
                var oldSettings = CurrentSettings;
                var newSettings = new AppSettings();

                if (preserveAppHistory)
                {
                    // Keep historical data
                    newSettings.LastAppLaunch = oldSettings.LastAppLaunch;
                    newSettings.IsFirstRun = oldSettings.IsFirstRun;
                }

                CurrentSettings = newSettings;
                
                // Apply the default theme
                ApplyTheme(newSettings.CurrentTheme);
                
                System.Diagnostics.Debug.WriteLine("SettingsService: Reset settings to defaults");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SettingsService: Error resetting settings: {ex.Message}");
            }
        }

        /// <summary>
        /// Updates a specific setting value without affecting other settings.
        /// This method provides type-safe access to individual settings.
        /// </summary>
        /// <typeparam name="T">The type of the setting value</typeparam>
        /// <param name="settingName">The name of the setting to update</param>
        /// <param name="value">The new value for the setting</param>
        /// <returns>True if the setting was updated successfully</returns>
        /// <example>
        /// // Update auto-save interval
        /// SettingsService.Instance.UpdateSetting("AutoSaveIntervalMinutes", 10);
        /// </example>
        public bool UpdateSetting<T>(string settingName, T value)
        {
            try
            {
                var settings = CurrentSettings;
                var property = typeof(AppSettings).GetProperty(settingName);
                
                if (property != null && property.CanWrite && property.PropertyType == typeof(T))
                {
                    property.SetValue(settings, value);
                    SaveSettings();
                    return true;
                }
                
                System.Diagnostics.Debug.WriteLine($"SettingsService: Setting '{settingName}' not found or type mismatch");
                return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SettingsService: Error updating setting '{settingName}': {ex.Message}");
                return false;
            }
        }

        #endregion
    }
}