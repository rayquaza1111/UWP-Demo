using System;
using System.Collections.ObjectModel;
using System.Globalization;
using UWPDemo.Models;

namespace UWPDemo.ViewModels
{
    /// <summary>
    /// ViewModel for the Data Binding Demo page.
    /// </summary>
    public class DataBindingDemoViewModel : BaseViewModel
    {
        private string _selectedItem;
        private bool _showAdditionalInfo = false;
        private double _numericValue = 150;

        public DataBindingDemoViewModel()
        {
            InitializePerson();
            InitializeItems();
        }

        public Person Person { get; private set; }

        public ObservableCollection<string> Items { get; private set; }

        public string SelectedItem
        {
            get => _selectedItem;
            set => SetProperty(ref _selectedItem, value);
        }

        public int ItemCount => Items.Count;

        public bool ShowAdditionalInfo
        {
            get => _showAdditionalInfo;
            set => SetProperty(ref _showAdditionalInfo, value);
        }

        public double NumericValue
        {
            get => _numericValue;
            set
            {
                if (SetProperty(ref _numericValue, value))
                {
                    OnPropertyChanged(nameof(FormattedValue));
                }
            }
        }

        public string FormattedValue => $"Formatted: {NumericValue:C2} ({NumericValue:F1}%)";

        public void AddItem()
        {
            // This method would be called from XAML, but we need the text input
            // The actual implementation is in the code-behind
        }

        public void AddItemFromText(string text)
        {
            if (!string.IsNullOrWhiteSpace(text))
            {
                Items.Add(text);
                OnPropertyChanged(nameof(ItemCount));
            }
        }

        public void RemoveItem(string item)
        {
            if (Items.Contains(item))
            {
                Items.Remove(item);
                OnPropertyChanged(nameof(ItemCount));
                
                // Clear selection if the selected item was removed
                if (SelectedItem == item)
                {
                    SelectedItem = null;
                }
            }
        }

        public void ClearItems()
        {
            Items.Clear();
            SelectedItem = null;
            OnPropertyChanged(nameof(ItemCount));
        }

        private void InitializePerson()
        {
            Person = new Person
            {
                FirstName = "John",
                LastName = "Doe",
                Age = 30,
                Email = "john.doe@example.com",
                Bio = "A sample person for demonstrating data binding in UWP applications. This bio will update in real-time as you type!"
            };

            // Subscribe to person property changes to update UI
            Person.PropertyChanged += (s, e) => OnPropertyChanged(nameof(Person));
        }

        private void InitializeItems()
        {
            Items = new ObservableCollection<string>
            {
                "Sample Item 1",
                "Sample Item 2",
                "Sample Item 3",
                "Another Item",
                "Last Item"
            };

            SelectedItem = Items[0];
        }
    }
}