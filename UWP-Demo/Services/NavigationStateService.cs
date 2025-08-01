using System;
using Windows.Storage;
using UWP_Demo.Models;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace UWP_Demo.Services
{
    /// <summary>
    /// ?? STATE MANAGEMENT: Service for managing navigation state and selected customer data
    /// ?? Purpose: Store and restore selected customer state across navigation and app sessions
    /// ?? Storage: Uses ApplicationDataContainer for persistent state management
    /// ?? Features: Selected customer, form data, navigation history, auto-restore
    /// </summary>
    public class NavigationStateService
    {
        private static NavigationStateService _instance;
        
        // ?? STATE MANAGEMENT: Reference to local settings storage for navigation state
        private readonly ApplicationDataContainer _localSettings;

        // ?? STATE MANAGEMENT: Setting keys for navigation state storage
        private const string SELECTED_CUSTOMER_KEY = "SelectedCustomer";
        private const string FORM_DATA_KEY = "FormData";
        private const string NAVIGATION_HISTORY_KEY = "NavigationHistory";
        private const string LAST_PAGE_KEY = "LastPage";
        private const string EDIT_MODE_KEY = "EditMode";
        private const string FORM_DIRTY_KEY = "FormDirty";

        public static NavigationStateService Instance => _instance ?? (_instance = new NavigationStateService());

        private NavigationStateService()
        {
            // ?? STATE MANAGEMENT: Get reference to local settings container for navigation state
            _localSettings = ApplicationData.Current.LocalSettings;
        }

        #region Selected Customer State

        /// <summary>
        /// ?? STATE MANAGEMENT: Gets or sets the currently selected customer
        /// ?? Persists: Customer data across navigation and app restarts
        /// ?? Usage: Store selected customer when navigating to edit form
        /// </summary>
        public Customer SelectedCustomer
        {
            get
            {
                try
                {
                    var value = _localSettings.Values[SELECTED_CUSTOMER_KEY];
                    if (value != null)
                    {
                        var customer = JsonConvert.DeserializeObject<Customer>(value.ToString());
                        System.Diagnostics.Debug.WriteLine($"?? STATE MANAGEMENT: Retrieved selected customer: {customer?.FullName}");
                        return customer;
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"?? STATE MANAGEMENT ERROR: Failed to deserialize selected customer - {ex.Message}");
                }
                return null;
            }
            set
            {
                try
                {
                    if (value != null)
                    {
                        var json = JsonConvert.SerializeObject(value);
                        _localSettings.Values[SELECTED_CUSTOMER_KEY] = json;
                        System.Diagnostics.Debug.WriteLine($"?? STATE MANAGEMENT: Stored selected customer: {value.FullName}");
                    }
                    else
                    {
                        _localSettings.Values.Remove(SELECTED_CUSTOMER_KEY);
                        System.Diagnostics.Debug.WriteLine("?? STATE MANAGEMENT: Cleared selected customer");
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"?? STATE MANAGEMENT ERROR: Failed to serialize selected customer - {ex.Message}");
                }
            }
        }

        #endregion

        #region Form State Management

        /// <summary>
        /// ?? STATE MANAGEMENT: Form data container for preserving user input
        /// ?? Stores: Partially filled form data to restore after navigation
        /// ??? Safety: Prevents data loss when user navigates away from form
        /// </summary>
        public class FormData
        {
            public string FirstName { get; set; } = "";
            public string LastName { get; set; } = "";
            public string Email { get; set; } = "";
            public string Phone { get; set; } = "";
            public string Company { get; set; } = "";
            public bool IsEditing { get; set; } = false;
            public int CustomerId { get; set; } = 0;
            public DateTime SavedAt { get; set; } = DateTime.Now;
        }

        /// <summary>
        /// ?? STATE MANAGEMENT: Gets or sets the current form data
        /// ?? Persists: User input across navigation and app sessions
        /// ?? Usage: Save form data when user navigates away, restore when returning
        /// </summary>
        public FormData CurrentFormData
        {
            get
            {
                try
                {
                    var value = _localSettings.Values[FORM_DATA_KEY];
                    if (value != null)
                    {
                        var formData = JsonConvert.DeserializeObject<FormData>(value.ToString());
                        System.Diagnostics.Debug.WriteLine($"?? STATE MANAGEMENT: Retrieved form data for customer ID: {formData?.CustomerId}");
                        return formData;
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"?? STATE MANAGEMENT ERROR: Failed to deserialize form data - {ex.Message}");
                }
                return new FormData();
            }
            set
            {
                try
                {
                    if (value != null)
                    {
                        value.SavedAt = DateTime.Now;
                        var json = JsonConvert.SerializeObject(value);
                        _localSettings.Values[FORM_DATA_KEY] = json;
                        System.Diagnostics.Debug.WriteLine($"?? STATE MANAGEMENT: Stored form data for customer ID: {value.CustomerId}");
                    }
                    else
                    {
                        _localSettings.Values.Remove(FORM_DATA_KEY);
                        System.Diagnostics.Debug.WriteLine("?? STATE MANAGEMENT: Cleared form data");
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"?? STATE MANAGEMENT ERROR: Failed to serialize form data - {ex.Message}");
                }
            }
        }

        #endregion

        #region Navigation History

        /// <summary>
        /// Navigation System: Navigation history for breadcrumb and back navigation
        /// Navigation System: Tracks: Page navigation history with timestamps
        /// Navigation System: Usage: Smart back navigation, breadcrumb display, navigation analytics
        /// </summary>
        public class NavigationEntry
        {
            public string PageType { get; set; }
            public string PageTitle { get; set; }
            public DateTime NavigatedAt { get; set; } = DateTime.Now;
            public string Parameters { get; set; } = "";
        }

        /// <summary>
        /// Navigation System: Gets or sets the navigation history
        /// Navigation System: Maintains: List of recent page navigations for smart back navigation
        /// </summary>
        public List<NavigationEntry> NavigationHistory
        {
            get
            {
                try
                {
                    var value = _localSettings.Values[NAVIGATION_HISTORY_KEY];
                    if (value != null)
                    {
                        var history = JsonConvert.DeserializeObject<List<NavigationEntry>>(value.ToString());
                        System.Diagnostics.Debug.WriteLine($"Navigation System: Retrieved navigation history with {history?.Count ?? 0} entries");
                        return history ?? new List<NavigationEntry>();
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Navigation System ERROR: Failed to deserialize navigation history - {ex.Message}");
                }
                return new List<NavigationEntry>();
            }
            private set
            {
                try
                {
                    if (value != null)
                    {
                        // Navigation System: Keep only last 10 navigation entries to prevent storage bloat
                        if (value.Count > 10)
                        {
                            value = value.GetRange(value.Count - 10, 10);
                        }
                        
                        var json = JsonConvert.SerializeObject(value);
                        _localSettings.Values[NAVIGATION_HISTORY_KEY] = json;
                        System.Diagnostics.Debug.WriteLine($"Navigation System: Stored navigation history with {value.Count} entries");
                    }
                    else
                    {
                        _localSettings.Values.Remove(NAVIGATION_HISTORY_KEY);
                        System.Diagnostics.Debug.WriteLine("Navigation System: Cleared navigation history");
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Navigation System ERROR: Failed to serialize navigation history - {ex.Message}");
                }
            }
        }

        #endregion

        #region State Management Operations

        /// <summary>
        /// ?? STATE MANAGEMENT: Store customer state when navigating to edit form
        /// ?? Saves: Selected customer and initializes form data
        /// ?? Call this: Before navigating to EditPage with a customer
        /// </summary>
        public void SetSelectedCustomerForEdit(Customer customer)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"?? STATE MANAGEMENT: Setting customer for edit: {customer?.FullName}");
                
                // ?? STATE MANAGEMENT: Store the selected customer
                SelectedCustomer = customer;
                
                // ?? STATE MANAGEMENT: Initialize form data with customer values
                if (customer != null)
                {
                    CurrentFormData = new FormData
                    {
                        FirstName = customer.FirstName,
                        LastName = customer.LastName,
                        Email = customer.Email,
                        Phone = customer.Phone ?? "",
                        Company = customer.Company ?? "",
                        IsEditing = true,
                        CustomerId = customer.Id
                    };
                }
                
                // Navigation System: Record this action in navigation history
                AddNavigationEntry("EditPage", $"Edit {customer?.FullName}", $"CustomerId={customer?.Id}");
                
                System.Diagnostics.Debug.WriteLine("?? STATE MANAGEMENT: Customer state prepared for edit form");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"?? STATE MANAGEMENT ERROR: Failed to set customer for edit - {ex.Message}");
            }
        }

        /// <summary>
        /// ?? STATE MANAGEMENT: Store form data when user navigates away
        /// ?? Saves: Current form input to prevent data loss
        /// ?? Call this: Before leaving EditPage with unsaved changes
        /// </summary>
        public void SaveFormState(string firstName, string lastName, string email, string phone, string company, bool isEditing, int customerId)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"?? STATE MANAGEMENT: Saving form state for customer ID: {customerId}");
                
                CurrentFormData = new FormData
                {
                    FirstName = firstName ?? "",
                    LastName = lastName ?? "",
                    Email = email ?? "",
                    Phone = phone ?? "",
                    Company = company ?? "",
                    IsEditing = isEditing,
                    CustomerId = customerId
                };
                
                // ?? STATE MANAGEMENT: Mark form as dirty (has unsaved changes)
                _localSettings.Values[FORM_DIRTY_KEY] = true;
                
                System.Diagnostics.Debug.WriteLine("?? STATE MANAGEMENT: Form state saved successfully");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"?? STATE MANAGEMENT ERROR: Failed to save form state - {ex.Message}");
            }
        }

        /// <summary>
        /// ?? STATE MANAGEMENT: Check if form has unsaved changes
        /// ?? Returns: True if user has unsaved form data
        /// ?? Usage: Show "unsaved changes" warning when navigating away
        /// </summary>
        public bool HasUnsavedChanges
        {
            get
            {
                var value = _localSettings.Values[FORM_DIRTY_KEY];
                return value != null && (bool)value;
            }
        }

        /// <summary>
        /// ?? STATE MANAGEMENT: Clear form dirty flag after successful save
        /// ? Call this: After successfully saving form data
        /// </summary>
        public void MarkFormClean()
        {
            _localSettings.Values[FORM_DIRTY_KEY] = false;
            System.Diagnostics.Debug.WriteLine("?? STATE MANAGEMENT: Form marked as clean (saved)");
        }

        /// <summary>
        /// ?? STATE MANAGEMENT: Clear all stored state (fresh start)
        /// ?? Clears: Selected customer, form data, navigation history
        /// ?? Usage: When user explicitly cancels or after successful operations
        /// </summary>
        public void ClearAllState()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("?? STATE MANAGEMENT: Clearing all navigation state");
                
                SelectedCustomer = null;
                CurrentFormData = null;
                NavigationHistory = new List<NavigationEntry>(); // Navigation System: Clear navigation history
                _localSettings.Values.Remove(FORM_DIRTY_KEY);
                _localSettings.Values.Remove(LAST_PAGE_KEY);
                _localSettings.Values.Remove(EDIT_MODE_KEY);
                
                System.Diagnostics.Debug.WriteLine("?? STATE MANAGEMENT: All state cleared successfully");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"?? STATE MANAGEMENT ERROR: Failed to clear state - {ex.Message}");
            }
        }

        /// <summary>
        /// Navigation System: Add entry to navigation history
        /// Navigation System: Tracks: Page visits for smart navigation and analytics
        /// </summary>
        public void AddNavigationEntry(string pageType, string pageTitle, string parameters = "")
        {
            try
            {
                var history = NavigationHistory;
                history.Add(new NavigationEntry
                {
                    PageType = pageType,
                    PageTitle = pageTitle,
                    Parameters = parameters,
                    NavigatedAt = DateTime.Now
                });
                NavigationHistory = history;
                
                System.Diagnostics.Debug.WriteLine($"Navigation System: Added navigation entry: {pageTitle}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Navigation System ERROR: Failed to add navigation entry - {ex.Message}");
            }
        }

        /// <summary>
        /// ?? STATE MANAGEMENT: Get navigation state summary for debugging
        /// ?? Returns: Comprehensive overview of current navigation state
        /// </summary>
        public string GetStateSummary()
        {
            try
            {
                var selectedCustomer = SelectedCustomer;
                var formData = CurrentFormData;
                var history = NavigationHistory;
                
                return $"Selected Customer: {selectedCustomer?.FullName ?? "None"}, " +
                       $"Form Data: {(formData != null ? $"Customer {formData.CustomerId}" : "None")}, " +
                       $"Has Unsaved Changes: {HasUnsavedChanges}, " +
                       $"Navigation History: {history.Count} entries";
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"?? STATE MANAGEMENT ERROR: Failed to get state summary - {ex.Message}");
                return "State summary unavailable";
            }
        }

        #endregion
    }
}