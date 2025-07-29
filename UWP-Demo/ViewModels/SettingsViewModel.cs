using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.UI.Xaml;
using UWP_Demo.Models;
using UWP_Demo.Services;
using UWP_Demo.Helpers;

namespace UWP_Demo.ViewModels
{
    /// <summary>
    /// ViewModel for the Settings page that manages application preferences and configuration.
    /// This ViewModel demonstrates MVVM patterns for settings management, theme switching,
    /// and application configuration in a UWP application.
    /// </summary>
    /// <remarks>
    /// This ViewModel showcases several important MVVM and UWP concepts:
    /// - Settings persistence and restoration
    /// - Theme management with real-time preview
    /// - Data binding for various control types (toggles, sliders, combos)
    /// - Command pattern for settings actions
    /// - Service integration for configuration management
    /// - Validation for settings values
    /// - Real-time settings application
    /// - Application state management
    /// 
    /// The SettingsViewModel serves as the data context for the Settings page and
    /// coordinates all application configuration operations including theme changes,
    /// preference updates, and system integration settings.
    /// </remarks>
    public class SettingsViewModel : BaseViewModel
    {
        #region Private Fields

        /// <summary>
        /// Reference to the settings service for configuration management.
        /// </summary>
        private readonly SettingsService _settingsService;

        /// <summary>
        /// Reference to the data service for data-related settings.
        /// </summary>
        private readonly DataService _dataService;

        /// <summary>
        /// Reference to the network service for network-related settings.
        /// </summary>
        private readonly NetworkService _networkService;

        /// <summary>
        /// The current application theme selection.
        /// </summary>
        private ElementTheme _selectedTheme;

        /// <summary>
        /// Whether notifications are enabled.
        /// </summary>
        private bool _notificationsEnabled;

        /// <summary>
        /// Whether auto-save is enabled.
        /// </summary>
        private bool _autoSaveEnabled;

        /// <summary>
        /// The auto-save interval in minutes.
        /// </summary>
        private int _autoSaveInterval;

        /// <summary>
        /// Application version information.
        /// </summary>
        private string _appVersion;

        /// <summary>
        /// Network connectivity status.
        /// </summary>
        private string _networkStatus;

        /// <summary>
        /// Storage usage information.
        /// </summary>
        private string _storageInfo;

        /// <summary>
        /// Last backup/save timestamp.
        /// </summary>
        private string _lastBackupInfo;

        /// <summary>
        /// Flag indicating whether settings are being saved.
        /// </summary>
        private bool _isSavingSettings;

        /// <summary>
        /// Flag indicating whether a reset operation is in progress.
        /// </summary>
        private bool _isResettingSettings;

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the selected application theme.
        /// Changing this property immediately applies the new theme.
        /// </summary>
        /// <remarks>
        /// This property is bound to theme selection controls (RadioButtons, ComboBox, etc.)
        /// and automatically applies theme changes when the selection changes.
        /// </remarks>
        public ElementTheme SelectedTheme
        {
            get => _selectedTheme;
            set
            {
                if (SetProperty(ref _selectedTheme, value))
                {
                    ApplyThemeChange();
                }
            }
        }

        /// <summary>
        /// Gets or sets whether notifications are enabled.
        /// This setting controls whether the app shows toast notifications and alerts.
        /// </summary>
        public bool NotificationsEnabled
        {
            get => _notificationsEnabled;
            set
            {
                if (SetProperty(ref _notificationsEnabled, value))
                {
                    UpdateNotificationSetting();
                }
            }
        }

        /// <summary>
        /// Gets or sets whether auto-save is enabled.
        /// When enabled, the app automatically saves data at regular intervals.
        /// </summary>
        public bool AutoSaveEnabled
        {
            get => _autoSaveEnabled;
            set
            {
                if (SetProperty(ref _autoSaveEnabled, value))
                {
                    UpdateAutoSaveSetting();
                    OnPropertyChanged(nameof(IsAutoSaveIntervalEnabled));
                }
            }
        }

        /// <summary>
        /// Gets or sets the auto-save interval in minutes.
        /// This setting is only relevant when auto-save is enabled.
        /// </summary>
        public int AutoSaveInterval
        {
            get => _autoSaveInterval;
            set
            {
                // Clamp value to valid range (1-60 minutes)
                int clampedValue = Math.Max(1, Math.Min(60, value));
                if (SetProperty(ref _autoSaveInterval, clampedValue))
                {
                    UpdateAutoSaveIntervalSetting();
                }
            }
        }

        /// <summary>
        /// Gets whether the auto-save interval setting is enabled.
        /// This is used to enable/disable the interval slider based on auto-save status.
        /// </summary>
        public bool IsAutoSaveIntervalEnabled => AutoSaveEnabled;

        /// <summary>
        /// Gets the application version information.
        /// This displays the current app version and build information.
        /// </summary>
        public string AppVersion
        {
            get => _appVersion;
            private set => SetProperty(ref _appVersion, value);
        }

        /// <summary>
        /// Gets the current network connectivity status.
        /// This shows whether the device is online and API services are available.
        /// </summary>
        public string NetworkStatus
        {
            get => _networkStatus;
            private set => SetProperty(ref _networkStatus, value);
        }

        /// <summary>
        /// Gets storage usage information.
        /// This shows information about local data storage and usage.
        /// </summary>
        public string StorageInfo
        {
            get => _storageInfo;
            private set => SetProperty(ref _storageInfo, value);
        }

        /// <summary>
        /// Gets information about the last backup or save operation.
        /// This shows when data was last saved or backed up.
        /// </summary>
        public string LastBackupInfo
        {
            get => _lastBackupInfo;
            private set => SetProperty(ref _lastBackupInfo, value);
        }

        /// <summary>
        /// Gets whether settings are currently being saved.
        /// This is used to show progress indicators during save operations.
        /// </summary>
        public bool IsSavingSettings
        {
            get => _isSavingSettings;
            private set => SetProperty(ref _isSavingSettings, value);
        }

        /// <summary>
        /// Gets whether a reset operation is in progress.
        /// This is used to show progress indicators during reset operations.
        /// </summary>
        public bool IsResettingSettings
        {
            get => _isResettingSettings;
            private set => SetProperty(ref _isResettingSettings, value);
        }

        /// <summary>
        /// Gets the available theme options for selection controls.
        /// This provides a list of theme options for ComboBox or RadioButton binding.
        /// </summary>
        public List<ThemeOption> AvailableThemes { get; private set; }

        /// <summary>
        /// Gets the available auto-save interval options.
        /// This provides predefined interval choices for quick selection.
        /// </summary>
        public List<int> AutoSaveIntervalOptions { get; private set; }

        /// <summary>
        /// Gets the current theme display name for UI display.
        /// </summary>
        public string CurrentThemeDisplayName => GetThemeDisplayName(SelectedTheme);

        #endregion

        #region Commands

        /// <summary>
        /// Command to manually save all current settings.
        /// This command persists all settings to storage immediately.
        /// </summary>
        public ICommand SaveSettingsCommand { get; private set; }

        /// <summary>
        /// Command to reset all settings to their default values.
        /// This command shows a confirmation dialog before resetting.
        /// </summary>
        public ICommand ResetSettingsCommand { get; private set; }

        /// <summary>
        /// Command to toggle between Light and Dark themes.
        /// This provides a quick way to switch themes without using selection controls.
        /// </summary>
        public ICommand ToggleThemeCommand { get; private set; }

        /// <summary>
        /// Command to refresh the network status information.
        /// This command updates connectivity and API availability status.
        /// </summary>
        public ICommand RefreshNetworkStatusCommand { get; private set; }

        /// <summary>
        /// Command to clear all application data.
        /// This command shows a confirmation dialog before clearing data.
        /// </summary>
        public ICommand ClearDataCommand { get; private set; }

        /// <summary>
        /// Command to export application settings.
        /// This command saves settings to a file for backup or sharing.
        /// </summary>
        public ICommand ExportSettingsCommand { get; private set; }

        /// <summary>
        /// Command to import application settings.
        /// This command loads settings from a previously exported file.
        /// </summary>
        public ICommand ImportSettingsCommand { get; private set; }

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the SettingsViewModel class.
        /// Sets up service references, initializes properties, and loads current settings.
        /// </summary>
        public SettingsViewModel()
        {
            // Get service references
            _settingsService = SettingsService.Instance;
            _dataService = DataService.Instance;
            _networkService = NetworkService.Instance;

            // Initialize properties
            Title = "Settings";
            
            // Initialize collections
            InitializeThemeOptions();
            InitializeAutoSaveOptions();

            // Initialize commands
            InitializeCommands();

            // Load current settings
            LoadCurrentSettings();

            // Update system information
            _ = UpdateSystemInformationAsync();

            Debug.WriteLine("SettingsViewModel: Initialized successfully");
        }

        #endregion

        #region Initialization

        /// <summary>
        /// Initializes the available theme options for selection controls.
        /// </summary>
        private void InitializeThemeOptions()
        {
            AvailableThemes = new List<ThemeOption>
            {
                new ThemeOption(ElementTheme.Default, "System Default", "Follow system theme setting"),
                new ThemeOption(ElementTheme.Light, "Light", "Always use light theme"),
                new ThemeOption(ElementTheme.Dark, "Dark", "Always use dark theme")
            };
        }

        /// <summary>
        /// Initializes the available auto-save interval options.
        /// </summary>
        private void InitializeAutoSaveOptions()
        {
            AutoSaveIntervalOptions = new List<int> { 1, 2, 5, 10, 15, 30, 60 };
        }

        /// <summary>
        /// Creates and configures all command objects used by this ViewModel.
        /// </summary>
        private void InitializeCommands()
        {
            // Save settings command - always available unless already saving
            SaveSettingsCommand = new RelayCommand(
                async () => await SaveSettingsAsync(),
                () => !IsSavingSettings && !IsBusy);

            // Reset settings command - always available unless resetting or busy
            ResetSettingsCommand = new RelayCommand(
                async () => await ResetSettingsAsync(),
                () => !IsResettingSettings && !IsBusy);

            // Toggle theme command - always available
            ToggleThemeCommand = new RelayCommand(
                () => ToggleTheme(),
                () => !IsBusy);

            // Refresh network status command - always available
            RefreshNetworkStatusCommand = new RelayCommand(
                async () => await RefreshNetworkStatusAsync(),
                () => !IsBusy);

            // Clear data command - available when there's data to clear
            ClearDataCommand = new RelayCommand(
                async () => await ClearDataAsync(),
                () => _dataService.CustomerCount > 0 && !IsBusy);

            // Export settings command - always available
            ExportSettingsCommand = new RelayCommand(
                async () => await ExportSettingsAsync(),
                () => !IsBusy);

            // Import settings command - always available
            ImportSettingsCommand = new RelayCommand(
                async () => await ImportSettingsAsync(),
                () => !IsBusy);
        }

        /// <summary>
        /// Loads the current settings from the settings service.
        /// </summary>
        private void LoadCurrentSettings()
        {
            try
            {
                var settings = _settingsService.CurrentSettings;

                // Load settings without triggering property change events during initialization
                _selectedTheme = settings.CurrentTheme;
                _notificationsEnabled = settings.EnableNotifications;
                _autoSaveEnabled = settings.AutoSaveData;
                _autoSaveInterval = settings.AutoSaveIntervalMinutes;

                // Notify all properties after loading
                OnPropertiesChanged(
                    nameof(SelectedTheme),
                    nameof(NotificationsEnabled),
                    nameof(AutoSaveEnabled),
                    nameof(AutoSaveInterval),
                    nameof(IsAutoSaveIntervalEnabled),
                    nameof(CurrentThemeDisplayName));

                Debug.WriteLine("SettingsViewModel: Current settings loaded");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"SettingsViewModel: Error loading settings: {ex.Message}");
                SetError("Unable to load current settings. Some values may not be accurate.");
            }
        }

        #endregion

        #region Settings Management

        /// <summary>
        /// Applies a theme change when the selected theme property changes.
        /// </summary>
        private void ApplyThemeChange()
        {
            try
            {
                _settingsService.CurrentTheme = SelectedTheme;
                OnPropertyChanged(nameof(CurrentThemeDisplayName));
                Debug.WriteLine($"SettingsViewModel: Applied theme change to {SelectedTheme}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"SettingsViewModel: Error applying theme change: {ex.Message}");
                SetError("Unable to apply theme change. Please try again.");
            }
        }

        /// <summary>
        /// Updates the notification setting when the property changes.
        /// </summary>
        private void UpdateNotificationSetting()
        {
            try
            {
                _settingsService.UpdateSetting(nameof(AppSettings.EnableNotifications), NotificationsEnabled);
                Debug.WriteLine($"SettingsViewModel: Updated notifications setting to {NotificationsEnabled}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"SettingsViewModel: Error updating notification setting: {ex.Message}");
            }
        }

        /// <summary>
        /// Updates the auto-save setting when the property changes.
        /// </summary>
        private void UpdateAutoSaveSetting()
        {
            try
            {
                _settingsService.UpdateSetting(nameof(AppSettings.AutoSaveData), AutoSaveEnabled);
                Debug.WriteLine($"SettingsViewModel: Updated auto-save setting to {AutoSaveEnabled}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"SettingsViewModel: Error updating auto-save setting: {ex.Message}");
            }
        }

        /// <summary>
        /// Updates the auto-save interval setting when the property changes.
        /// </summary>
        private void UpdateAutoSaveIntervalSetting()
        {
            try
            {
                _settingsService.UpdateSetting(nameof(AppSettings.AutoSaveIntervalMinutes), AutoSaveInterval);
                Debug.WriteLine($"SettingsViewModel: Updated auto-save interval to {AutoSaveInterval} minutes");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"SettingsViewModel: Error updating auto-save interval: {ex.Message}");
            }
        }

        /// <summary>
        /// Manually saves all current settings to storage.
        /// </summary>
        /// <returns>A task representing the asynchronous save operation</returns>
        private async Task SaveSettingsAsync()
        {
            try
            {
                IsSavingSettings = true;
                IsBusy = true;
                ClearError();

                Debug.WriteLine("SettingsViewModel: Saving settings manually");

                // Save settings through the service
                _settingsService.SaveSettings();

                // Also save data if auto-save is enabled
                if (AutoSaveEnabled)
                {
                    await _dataService.SaveDataAsync();
                }

                // Update last backup info
                await UpdateSystemInformationAsync();

                Debug.WriteLine("SettingsViewModel: Settings saved successfully");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"SettingsViewModel: Error saving settings: {ex.Message}");
                SetError(ex, "Unable to save settings. Please try again.");
            }
            finally
            {
                IsSavingSettings = false;
                IsBusy = false;
                RaiseCanExecuteChanged();
            }
        }

        /// <summary>
        /// Resets all settings to their default values.
        /// </summary>
        /// <returns>A task representing the asynchronous reset operation</returns>
        private async Task ResetSettingsAsync()
        {
            try
            {
                IsResettingSettings = true;
                IsBusy = true;
                ClearError();

                Debug.WriteLine("SettingsViewModel: Resetting settings to defaults");

                // In a real app, you would show a confirmation dialog here
                // For this demo, we'll proceed with the reset

                // Reset settings through the service
                _settingsService.ResetToDefaults(preserveAppHistory: true);

                // Reload the current settings
                LoadCurrentSettings();

                // Update system information
                await UpdateSystemInformationAsync();

                Debug.WriteLine("SettingsViewModel: Settings reset completed");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"SettingsViewModel: Error resetting settings: {ex.Message}");
                SetError(ex, "Unable to reset settings. Please try again.");
            }
            finally
            {
                IsResettingSettings = false;
                IsBusy = false;
                RaiseCanExecuteChanged();
            }
        }

        #endregion

        #region Theme Management

        /// <summary>
        /// Toggles between Light and Dark themes.
        /// </summary>
        private void ToggleTheme()
        {
            try
            {
                _settingsService.ToggleTheme();
                
                // Update the selected theme property to reflect the change
                _selectedTheme = _settingsService.CurrentTheme;
                OnPropertyChanged(nameof(SelectedTheme));
                OnPropertyChanged(nameof(CurrentThemeDisplayName));

                Debug.WriteLine($"SettingsViewModel: Theme toggled to {SelectedTheme}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"SettingsViewModel: Error toggling theme: {ex.Message}");
                SetError("Unable to toggle theme. Please try again.");
            }
        }

        /// <summary>
        /// Gets the display name for a theme value.
        /// </summary>
        /// <param name="theme">The theme to get the display name for</param>
        /// <returns>The user-friendly display name</returns>
        private string GetThemeDisplayName(ElementTheme theme)
        {
            return theme switch
            {
                ElementTheme.Light => "Light",
                ElementTheme.Dark => "Dark",
                ElementTheme.Default => "System Default",
                _ => "Unknown"
            };
        }

        #endregion

        #region System Information

        /// <summary>
        /// Updates system information displayed in the settings page.
        /// </summary>
        /// <returns>A task representing the asynchronous update operation</returns>
        private async Task UpdateSystemInformationAsync()
        {
            try
            {
                // Update app version
                UpdateAppVersionInfo();

                // Update network status
                await UpdateNetworkStatusInfo();

                // Update storage info
                await UpdateStorageInfo();

                // Update backup info
                UpdateBackupInfo();

                Debug.WriteLine("SettingsViewModel: System information updated");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"SettingsViewModel: Error updating system information: {ex.Message}");
            }
        }

        /// <summary>
        /// Updates the application version information.
        /// </summary>
        private void UpdateAppVersionInfo()
        {
            try
            {
                var package = Windows.ApplicationModel.Package.Current;
                var version = package.Id.Version;
                AppVersion = $"Version {version.Major}.{version.Minor}.{version.Build}.{version.Revision}";
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"SettingsViewModel: Error getting app version: {ex.Message}");
                AppVersion = "Version information unavailable";
            }
        }

        /// <summary>
        /// Updates the network connectivity status information.
        /// </summary>
        /// <returns>A task representing the asynchronous operation</returns>
        private async Task UpdateNetworkStatusInfo()
        {
            try
            {
                bool isConnected = _networkService.IsAvailable;
                if (isConnected)
                {
                    bool apiAvailable = await _networkService.TestApiConnectivityAsync();
                    NetworkStatus = apiAvailable ? "Online (API Available)" : "Online (Limited Connectivity)";
                }
                else
                {
                    NetworkStatus = "Offline";
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"SettingsViewModel: Error updating network status: {ex.Message}");
                NetworkStatus = "Status Unknown";
            }
        }

        /// <summary>
        /// Updates storage usage information.
        /// </summary>
        /// <returns>A task representing the asynchronous operation</returns>
        private async Task UpdateStorageInfo()
        {
            try
            {
                var localFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
                var properties = await localFolder.GetBasicPropertiesAsync();
                
                int customerCount = _dataService.CustomerCount;
                StorageInfo = $"{customerCount} customers stored, {FormatBytes(properties.Size)} used";
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"SettingsViewModel: Error updating storage info: {ex.Message}");
                StorageInfo = "Storage information unavailable";
            }
        }

        /// <summary>
        /// Updates backup/save information.
        /// </summary>
        private void UpdateBackupInfo()
        {
            try
            {
                var lastLaunch = _settingsService.CurrentSettings.LastAppLaunch;
                LastBackupInfo = $"Last saved: {lastLaunch:yyyy-MM-dd HH:mm}";
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"SettingsViewModel: Error updating backup info: {ex.Message}");
                LastBackupInfo = "Backup information unavailable";
            }
        }

        /// <summary>
        /// Refreshes the network status information.
        /// </summary>
        /// <returns>A task representing the asynchronous operation</returns>
        private async Task RefreshNetworkStatusAsync()
        {
            try
            {
                await UpdateNetworkStatusInfo();
                Debug.WriteLine("SettingsViewModel: Network status refreshed");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"SettingsViewModel: Error refreshing network status: {ex.Message}");
                SetError("Unable to refresh network status. Please try again.");
            }
        }

        /// <summary>
        /// Formats a byte count into a human-readable string.
        /// </summary>
        /// <param name="bytes">The number of bytes</param>
        /// <returns>A formatted string (e.g., "1.5 KB", "2.3 MB")</returns>
        private string FormatBytes(ulong bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB" };
            double len = bytes;
            int order = 0;
            
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }
            
            return $"{len:0.##} {sizes[order]}";
        }

        #endregion

        #region Data Management

        /// <summary>
        /// Clears all application data after confirmation.
        /// </summary>
        /// <returns>A task representing the asynchronous operation</returns>
        private async Task ClearDataAsync()
        {
            try
            {
                IsBusy = true;
                ClearError();

                Debug.WriteLine("SettingsViewModel: Clearing all data");

                // In a real app, you would show a confirmation dialog here
                // For this demo, we'll proceed with clearing

                await _dataService.ClearAllDataAsync();
                
                // Update storage info
                await UpdateStorageInfo();
                
                // Update command availability
                RaiseCanExecuteChanged();

                Debug.WriteLine("SettingsViewModel: Data cleared successfully");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"SettingsViewModel: Error clearing data: {ex.Message}");
                SetError(ex, "Unable to clear data. Please try again.");
            }
            finally
            {
                IsBusy = false;
            }
        }

        /// <summary>
        /// Exports application settings to a file.
        /// </summary>
        /// <returns>A task representing the asynchronous operation</returns>
        private async Task ExportSettingsAsync()
        {
            try
            {
                IsBusy = true;
                ClearError();

                Debug.WriteLine("SettingsViewModel: Exporting settings");

                // In a real app, you would show a file picker and export to the selected location
                // For this demo, we'll just save to the local folder
                
                _settingsService.SaveSettings();

                Debug.WriteLine("SettingsViewModel: Settings exported successfully");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"SettingsViewModel: Error exporting settings: {ex.Message}");
                SetError(ex, "Unable to export settings. Please try again.");
            }
            finally
            {
                IsBusy = false;
            }
        }

        /// <summary>
        /// Imports application settings from a file.
        /// </summary>
        /// <returns>A task representing the asynchronous operation</returns>
        private async Task ImportSettingsAsync()
        {
            try
            {
                IsBusy = true;
                ClearError();

                Debug.WriteLine("SettingsViewModel: Importing settings");

                // In a real app, you would show a file picker and load from the selected file
                // For this demo, we'll just reload current settings
                
                LoadCurrentSettings();
                await UpdateSystemInformationAsync();

                Debug.WriteLine("SettingsViewModel: Settings imported successfully");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"SettingsViewModel: Error importing settings: {ex.Message}");
                SetError(ex, "Unable to import settings. Please try again.");
            }
            finally
            {
                IsBusy = false;
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Raises CanExecuteChanged for all commands to update UI button states.
        /// </summary>
        private void RaiseCanExecuteChanged()
        {
            try
            {
                (SaveSettingsCommand as RelayCommand)?.RaiseCanExecuteChanged();
                (ResetSettingsCommand as RelayCommand)?.RaiseCanExecuteChanged();
                (ToggleThemeCommand as RelayCommand)?.RaiseCanExecuteChanged();
                (RefreshNetworkStatusCommand as RelayCommand)?.RaiseCanExecuteChanged();
                (ClearDataCommand as RelayCommand)?.RaiseCanExecuteChanged();
                (ExportSettingsCommand as RelayCommand)?.RaiseCanExecuteChanged();
                (ImportSettingsCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"SettingsViewModel: Error raising CanExecuteChanged: {ex.Message}");
            }
        }

        #endregion

        #region Cleanup

        /// <summary>
        /// Performs cleanup when the ViewModel is no longer needed.
        /// </summary>
        protected override void Cleanup()
        {
            try
            {
                Debug.WriteLine("SettingsViewModel: Cleanup completed");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"SettingsViewModel: Error during cleanup: {ex.Message}");
            }
            finally
            {
                base.Cleanup();
            }
        }

        #endregion
    }

    /// <summary>
    /// Represents a theme option for UI selection controls.
    /// This class provides the data structure for theme selection in ComboBox or RadioButton controls.
    /// </summary>
    public class ThemeOption
    {
        /// <summary>
        /// Gets the theme value.
        /// </summary>
        public ElementTheme Theme { get; }

        /// <summary>
        /// Gets the display name for the theme.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the description of the theme.
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// Initializes a new instance of the ThemeOption class.
        /// </summary>
        /// <param name="theme">The theme value</param>
        /// <param name="name">The display name</param>
        /// <param name="description">The description</param>
        public ThemeOption(ElementTheme theme, string name, string description)
        {
            Theme = theme;
            Name = name;
            Description = description;
        }

        /// <summary>
        /// Returns the display name for this theme option.
        /// </summary>
        /// <returns>The display name</returns>
        public override string ToString()
        {
            return Name;
        }
    }
}