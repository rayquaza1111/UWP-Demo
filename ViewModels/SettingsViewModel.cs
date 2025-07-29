using Microsoft.UI.Xaml.Controls;
using System;
using System.Reflection;
using System.Runtime.InteropServices;
using Windows.ApplicationModel;
using Windows.System;
using Windows.System.Profile;

namespace UWPDemo.ViewModels
{
    /// <summary>
    /// ViewModel for the Settings page.
    /// </summary>
    public class SettingsViewModel : BaseViewModel
    {
        private int _selectedThemeIndex = 2; // System Default
        private bool _useAccentColor = true;
        private bool _showNavigationIcons = true;
        private bool _rememberWindowSettings = true;
        private bool _showConfirmationDialogs = true;
        private bool _enableAnimations = true;
        private double _animationSpeed = 1.0;
        private bool _autoSave = true;
        private double _cacheSizeLimit = 100;

        public SettingsViewModel()
        {
            LoadSystemInformation();
        }

        // Appearance Settings
        public int SelectedThemeIndex
        {
            get => _selectedThemeIndex;
            set => SetProperty(ref _selectedThemeIndex, value);
        }

        public bool UseAccentColor
        {
            get => _useAccentColor;
            set => SetProperty(ref _useAccentColor, value);
        }

        public bool ShowNavigationIcons
        {
            get => _showNavigationIcons;
            set => SetProperty(ref _showNavigationIcons, value);
        }

        // Behavior Settings
        public bool RememberWindowSettings
        {
            get => _rememberWindowSettings;
            set => SetProperty(ref _rememberWindowSettings, value);
        }

        public bool ShowConfirmationDialogs
        {
            get => _showConfirmationDialogs;
            set => SetProperty(ref _showConfirmationDialogs, value);
        }

        public bool EnableAnimations
        {
            get => _enableAnimations;
            set => SetProperty(ref _enableAnimations, value);
        }

        public double AnimationSpeed
        {
            get => _animationSpeed;
            set
            {
                if (SetProperty(ref _animationSpeed, value))
                {
                    OnPropertyChanged(nameof(AnimationSpeedText));
                }
            }
        }

        public string AnimationSpeedText => $"{AnimationSpeed:F1}x speed";

        // Data Settings
        public bool AutoSave
        {
            get => _autoSave;
            set => SetProperty(ref _autoSave, value);
        }

        public double CacheSizeLimit
        {
            get => _cacheSizeLimit;
            set => SetProperty(ref _cacheSizeLimit, value);
        }

        // System Information
        public string AppVersion { get; private set; } = "1.0.0.0";
        public string OperatingSystem { get; private set; } = "Windows 10/11";
        public string Architecture { get; private set; } = "x64";
        public string DotNetVersion { get; private set; } = ".NET 6.0";
        public string DeviceFamily { get; private set; } = "Desktop";
        public string WinUIVersion { get; private set; } = "WinUI 3";

        // Actions
        public async void ClearCache()
        {
            ContentDialog dialog = new ContentDialog
            {
                Title = "Clear Cache",
                Content = "Cache has been cleared successfully!",
                CloseButtonText = "OK"
            };

            // Note: In a real app, you'd pass the XamlRoot from the page
            await dialog.ShowAsync();
        }

        public void ResetToDefaults()
        {
            SelectedThemeIndex = 2;
            UseAccentColor = true;
            ShowNavigationIcons = true;
            RememberWindowSettings = true;
            ShowConfirmationDialogs = true;
            EnableAnimations = true;
            AnimationSpeed = 1.0;
            AutoSave = true;
            CacheSizeLimit = 100;
        }

        public async void ExportSettings()
        {
            ContentDialog dialog = new ContentDialog
            {
                Title = "Export Settings",
                Content = "Settings export feature would be implemented here.\nThis would save current settings to a file.",
                CloseButtonText = "OK"
            };

            await dialog.ShowAsync();
        }

        public async void ImportSettings()
        {
            ContentDialog dialog = new ContentDialog
            {
                Title = "Import Settings",
                Content = "Settings import feature would be implemented here.\nThis would load settings from a file.",
                CloseButtonText = "OK"
            };

            await dialog.ShowAsync();
        }

        public async void OpenRepository()
        {
            await Launcher.LaunchUriAsync(new Uri("https://github.com/rayquaza1111/UWP-Demo"));
        }

        public async void OpenDocumentation()
        {
            await Launcher.LaunchUriAsync(new Uri("https://docs.microsoft.com/en-us/windows/uwp/"));
        }

        public async void ReportIssue()
        {
            await Launcher.LaunchUriAsync(new Uri("https://github.com/rayquaza1111/UWP-Demo/issues"));
        }

        private void LoadSystemInformation()
        {
            try
            {
                // Get app version
                var package = Package.Current;
                var version = package.Id.Version;
                AppVersion = $"{version.Major}.{version.Minor}.{version.Build}.{version.Revision}";

                // Get system architecture
                Architecture = RuntimeInformation.ProcessArchitecture.ToString();

                // Get .NET version
                DotNetVersion = RuntimeInformation.FrameworkDescription;

                // Get device family
                DeviceFamily = AnalyticsInfo.VersionInfo.DeviceFamily;

                // Get OS version (simplified)
                var osVersion = Environment.OSVersion.Version;
                if (osVersion.Major == 10 && osVersion.Build >= 22000)
                {
                    OperatingSystem = "Windows 11";
                }
                else if (osVersion.Major == 10)
                {
                    OperatingSystem = "Windows 10";
                }
                else
                {
                    OperatingSystem = $"Windows {osVersion.Major}.{osVersion.Minor}";
                }
            }
            catch (Exception)
            {
                // Fallback values if system info retrieval fails
                AppVersion = "1.0.0.0";
                Architecture = "Unknown";
                DotNetVersion = ".NET 6.0+";
                DeviceFamily = "Desktop";
                OperatingSystem = "Windows";
            }
        }
    }
}