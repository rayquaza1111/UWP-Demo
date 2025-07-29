using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Windows.System;

namespace UWPDemo.ViewModels
{
    /// <summary>
    /// ViewModel for the Controls Demo page.
    /// </summary>
    public class ControlsDemoViewModel : BaseViewModel
    {
        private string _sampleText = "Hello, UWP!";
        private string _selectedComboBoxItem;
        private double _sliderValue = 50;
        private DateTimeOffset _selectedDate = DateTimeOffset.Now;
        private TimeSpan _selectedTime = DateTime.Now.TimeOfDay;
        private bool _isFeatureEnabled = true;
        private bool _notificationsEnabled = false;
        private bool _optionA = true;
        private bool _optionB = false;
        private double _progressValue = 0;
        private bool _isProgressActive = false;
        private bool _isToggled = false;
        private string _statusText = "Ready";

        public ControlsDemoViewModel()
        {
            InitializeComboBoxItems();
            InitializeDefaultValues();
        }

        public string SampleText
        {
            get => _sampleText;
            set
            {
                if (SetProperty(ref _sampleText, value))
                {
                    UpdateStatus($"Text changed to: {value}");
                }
            }
        }

        public ObservableCollection<string> ComboBoxItems { get; private set; }

        public string SelectedComboBoxItem
        {
            get => _selectedComboBoxItem;
            set
            {
                if (SetProperty(ref _selectedComboBoxItem, value))
                {
                    UpdateStatus($"Selected: {value}");
                }
            }
        }

        public double SliderValue
        {
            get => _sliderValue;
            set
            {
                if (SetProperty(ref _sliderValue, value))
                {
                    UpdateStatus($"Slider value: {value:F0}");
                }
            }
        }

        public DateTimeOffset SelectedDate
        {
            get => _selectedDate;
            set
            {
                if (SetProperty(ref _selectedDate, value))
                {
                    UpdateStatus($"Date selected: {value:d}");
                }
            }
        }

        public TimeSpan SelectedTime
        {
            get => _selectedTime;
            set
            {
                if (SetProperty(ref _selectedTime, value))
                {
                    UpdateStatus($"Time selected: {value:hh\\:mm}");
                }
            }
        }

        public bool IsFeatureEnabled
        {
            get => _isFeatureEnabled;
            set
            {
                if (SetProperty(ref _isFeatureEnabled, value))
                {
                    UpdateStatus($"Feature {(value ? "enabled" : "disabled")}");
                }
            }
        }

        public bool NotificationsEnabled
        {
            get => _notificationsEnabled;
            set
            {
                if (SetProperty(ref _notificationsEnabled, value))
                {
                    UpdateStatus($"Notifications {(value ? "enabled" : "disabled")}");
                }
            }
        }

        public bool OptionA
        {
            get => _optionA;
            set
            {
                if (SetProperty(ref _optionA, value) && value)
                {
                    OptionB = false;
                    UpdateStatus("Option A selected");
                }
            }
        }

        public bool OptionB
        {
            get => _optionB;
            set
            {
                if (SetProperty(ref _optionB, value) && value)
                {
                    OptionA = false;
                    UpdateStatus("Option B selected");
                }
            }
        }

        public double ProgressValue
        {
            get => _progressValue;
            set => SetProperty(ref _progressValue, value);
        }

        public bool IsProgressActive
        {
            get => _isProgressActive;
            set => SetProperty(ref _isProgressActive, value);
        }

        public bool IsToggled
        {
            get => _isToggled;
            set
            {
                if (SetProperty(ref _isToggled, value))
                {
                    UpdateStatus($"Toggle button {(value ? "pressed" : "released")}");
                }
            }
        }

        public string StatusText
        {
            get => _statusText;
            set => SetProperty(ref _statusText, value);
        }

        public async void ShowMessage()
        {
            ContentDialog dialog = new ContentDialog
            {
                Title = "Button Clicked",
                Content = "This is a standard button action!",
                CloseButtonText = "OK"
            };

            // Note: In a real app, you'd pass the XamlRoot from the page
            await dialog.ShowAsync();
            UpdateStatus("Message dialog shown");
        }

        public async void IncrementProgress()
        {
            IsProgressActive = true;
            UpdateStatus("Progress started...");

            // Simulate work with progress updates
            for (int i = 0; i <= 100; i += 5)
            {
                ProgressValue = i;
                await Task.Delay(100);
            }

            IsProgressActive = false;
            UpdateStatus("Progress completed!");
        }

        public void AddItem()
        {
            ComboBoxItems.Add($"New Item {ComboBoxItems.Count + 1}");
            UpdateStatus($"Added item. Total: {ComboBoxItems.Count}");
        }

        private void InitializeComboBoxItems()
        {
            ComboBoxItems = new ObservableCollection<string>
            {
                "Option 1",
                "Option 2",
                "Option 3",
                "Custom Option"
            };
        }

        private void InitializeDefaultValues()
        {
            SelectedComboBoxItem = ComboBoxItems[0];
            UpdateStatus("Controls demo loaded");
        }

        private void UpdateStatus(string message)
        {
            StatusText = $"{DateTime.Now:HH:mm:ss} - {message}";
        }
    }
}