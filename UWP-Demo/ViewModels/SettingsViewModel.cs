using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Windows.UI.Xaml;
using UWP_Demo.Commands;
using UWP_Demo.Services;

namespace UWP_Demo.ViewModels
{
    /// <summary>
    /// ViewModel for the Settings page
    /// Manages theme switching, notifications, and other app settings
    /// </summary>
    public class SettingsViewModel : INotifyPropertyChanged
    {
        // THEME PERSISTENCE: Reference to settings service for save/load operations
        private readonly SettingsService _settingsService;

        public SettingsViewModel()
        {
            System.Diagnostics.Debug.WriteLine("======= SettingsViewModel: Constructor called! =======");
            
            try
            {
                // THEME PERSISTENCE: Get settings service instance for save/load operations
                _settingsService = SettingsService.Instance;
                System.Diagnostics.Debug.WriteLine(">>> SettingsViewModel: SettingsService.Instance obtained");
                
                // THEME PERSISTENCE: Subscribe to theme changes for automatic UI updates when theme is saved/loaded
                _settingsService.ThemeChanged += OnThemeChanged;
                System.Diagnostics.Debug.WriteLine(">>> SettingsViewModel: ThemeChanged event subscribed");
                
                InitializeCommands();
                System.Diagnostics.Debug.WriteLine(">>> SettingsViewModel: Commands initialized");
                
                System.Diagnostics.Debug.WriteLine(">>> SettingsViewModel: Initialized successfully!");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($">>> [ERROR] SettingsViewModel: Error in constructor: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($">>> [ERROR] SettingsViewModel: Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        #region Properties

        /// <summary>
        /// Page title
        /// </summary>
        public string Title => "Settings";

        /// <summary>
        /// Page subtitle with current theme info
        /// </summary>
        public string Subtitle => $"Current theme: {_settingsService.GetThemeName()}";

        /// <summary>
        /// Is Dark Theme enabled (for ToggleSwitch binding)
        /// </summary>
        public bool IsDarkTheme
        {
            // THEME PERSISTENCE: Get current theme from saved settings
            get => _settingsService.IsDarkTheme;
            set
            {
                if (_settingsService.IsDarkTheme != value)
                {
                    // THEME PERSISTENCE: Save theme change to persistent storage
                    _settingsService.IsDarkTheme = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(Subtitle));
                    OnPropertyChanged(nameof(CurrentThemeText));
                }
            }
        }

        /// <summary>
        /// Current theme display text
        /// </summary>
        public string CurrentThemeText => $"Current: {_settingsService.GetThemeName()}";

        /// <summary>
        /// Are notifications enabled
        /// </summary>
        public bool NotificationsEnabled
        {
            get => _settingsService.NotificationsEnabled;
            set
            {
                if (_settingsService.NotificationsEnabled != value)
                {
                    _settingsService.NotificationsEnabled = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Is auto-save enabled
        /// </summary>
        public bool AutoSaveEnabled
        {
            get => _settingsService.AutoSaveEnabled;
            set
            {
                if (_settingsService.AutoSaveEnabled != value)
                {
                    _settingsService.AutoSaveEnabled = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Settings summary for display
        /// </summary>
        public string SettingsSummary => _settingsService.GetSettingsSummary();

        #endregion

        #region Commands

        public ICommand ToggleThemeCommand { get; private set; }
        public ICommand ResetSettingsCommand { get; private set; }
        public ICommand SaveSettingsCommand { get; private set; }

        private void InitializeCommands()
        {
            ToggleThemeCommand = new RelayCommand(ToggleTheme);
            ResetSettingsCommand = new RelayCommand(ResetSettings);
            SaveSettingsCommand = new RelayCommand(SaveSettings);
        }

        #endregion

        #region Command Methods

        /// <summary>
        /// Toggle between Light and Dark themes
        /// </summary>
        private void ToggleTheme()
        {
            try
            {
                // THEME PERSISTENCE: Toggle theme and automatically save to storage
                _settingsService.ToggleTheme();
                
                // Update UI
                OnPropertyChanged(nameof(IsDarkTheme));
                OnPropertyChanged(nameof(Subtitle));
                OnPropertyChanged(nameof(CurrentThemeText));
                OnPropertyChanged(nameof(SettingsSummary));
                
                System.Diagnostics.Debug.WriteLine($"SettingsViewModel: Theme toggled to {_settingsService.GetThemeName()}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SettingsViewModel: Error toggling theme - {ex.Message}");
            }
        }

        /// <summary>
        /// Reset all settings to defaults
        /// </summary>
        private void ResetSettings()
        {
            try
            {
                // THEME PERSISTENCE: Reset theme to default and save to storage
                _settingsService.ResetToDefaults();
                
                // Update all UI properties
                OnPropertyChanged(nameof(IsDarkTheme));
                OnPropertyChanged(nameof(NotificationsEnabled));
                OnPropertyChanged(nameof(AutoSaveEnabled));
                OnPropertyChanged(nameof(Subtitle));
                OnPropertyChanged(nameof(CurrentThemeText));
                OnPropertyChanged(nameof(SettingsSummary));
                
                System.Diagnostics.Debug.WriteLine("SettingsViewModel: Settings reset to defaults");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SettingsViewModel: Error resetting settings - {ex.Message}");
            }
        }

        /// <summary>
        /// Save current settings (already auto-saved, this is for user feedback)
        /// </summary>
        private void SaveSettings()
        {
            try
            {
                // THEME PERSISTENCE: Settings are automatically saved when changed, this is just for user feedback
                OnPropertyChanged(nameof(SettingsSummary));
                System.Diagnostics.Debug.WriteLine("SettingsViewModel: Settings saved (auto-saved on change)");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SettingsViewModel: Error in save settings - {ex.Message}");
            }
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Handle theme change events from SettingsService
        /// </summary>
        private void OnThemeChanged(object sender, ElementTheme newTheme)
        {
            // THEME PERSISTENCE: Update UI when theme changes from other sources (loaded from storage)
            OnPropertyChanged(nameof(IsDarkTheme));
            OnPropertyChanged(nameof(Subtitle));
            OnPropertyChanged(nameof(CurrentThemeText));
            OnPropertyChanged(nameof(SettingsSummary));
        }

        #endregion

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        #region Cleanup

        /// <summary>
        /// Cleanup when ViewModel is disposed
        /// </summary>
        public void Dispose()
        {
            if (_settingsService != null)
            {
                // THEME PERSISTENCE: Unsubscribe from theme change events
                _settingsService.ThemeChanged -= OnThemeChanged;
            }
        }

        #endregion
    }
}