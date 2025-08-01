using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UWP_Demo.Models;
using System.Collections.ObjectModel;
// FILE I/O: Required UWP namespaces for file operations
using Windows.Storage;     // FILE I/O: Provides StorageFile, StorageFolder classes for file system access
using Newtonsoft.Json;     // FILE I/O: JSON serialization library for converting objects to/from JSON

namespace UWP_Demo.Services
{
    /// <summary>
    /// FILE I/O: Customer service with complete file persistence functionality
    /// Automatically saves and loads customer data to/from JSON file using UWP Storage APIs
    /// 
    /// FILE I/O FLOW:
    /// 1. App starts ? LoadCustomersFromFileAsync() ? Read JSON from file ? Deserialize to Customer objects
    /// 2. User adds customer ? AddCustomerAsync() ? Add to list ? SaveCustomersToFileAsync() ? Write JSON to file
    /// 3. User deletes customer ? DeleteCustomerAsync() ? Remove from list ? SaveCustomersToFileAsync() ? Write JSON to file
    /// 4. App restart ? Cycle repeats, data persists!
    /// </summary>
    public class CustomerService
    {
        // FILE I/O: In-memory storage - acts as cache between UI and file system
        private readonly List<Customer> _customers = new List<Customer>();
        
        // FILE I/O: File operation configuration constants
        private const string CUSTOMERS_FILE_NAME = "customers.json";  // FILE I/O: Target filename for customer data storage
        private StorageFolder _localFolder;                          // FILE I/O: Reference to app's local data folder
        private bool _isLoaded = false;                              // FILE I/O: Flag to track if data has been loaded from file

        public CustomerService()
        {
            // FILE I/O: Initialize file system access - get reference to app's local data storage folder
            // This folder is: %LOCALAPPDATA%\Packages\[YourAppPackageName]\LocalState\
            _localFolder = ApplicationData.Current.LocalFolder;
            System.Diagnostics.Debug.WriteLine("CustomerService: Initialized with file persistence support");
        }

        #region FILE I/O: Core File Operations

        /// <summary>
        /// FILE I/O: SAVE OPERATION - Writes customer list to JSON file
        /// This is the "OUTPUT" part of File I/O - data flows FROM app TO file
        /// 
        /// FLOW: Customer List ? JSON String ? File System
        /// 1. Serialize customer objects to JSON string
        /// 2. Create/overwrite customers.json file
        /// 3. Write JSON string to file
        /// </summary>
        private async Task SaveCustomersToFileAsync()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"FILE I/O OUTPUT: Starting save of {_customers.Count} customers to file...");
                
                // FILE I/O: Step 1 - Convert customer objects to JSON string
                string customersJson = JsonConvert.SerializeObject(_customers, Formatting.Indented);
                System.Diagnostics.Debug.WriteLine($"FILE I/O OUTPUT: Serialized customers to JSON ({customersJson.Length} characters)");
                
                // FILE I/O: Step 2 - Create file in local storage folder, replace if exists
                StorageFile customersFile = await _localFolder.CreateFileAsync(
                    CUSTOMERS_FILE_NAME, 
                    CreationCollisionOption.ReplaceExisting);
                System.Diagnostics.Debug.WriteLine($"FILE I/O OUTPUT: Created/accessed file: {customersFile.Path}");
                
                // FILE I/O: Step 3 - Write JSON string to file using UWP FileIO API
                await FileIO.WriteTextAsync(customersFile, customersJson);
                
                System.Diagnostics.Debug.WriteLine($"FILE I/O OUTPUT: Successfully saved {_customers.Count} customers to {CUSTOMERS_FILE_NAME}");
            }
            catch (Exception ex)
            {
                // FILE I/O: Handle file write errors gracefully - don't crash the app
                System.Diagnostics.Debug.WriteLine($"FILE I/O OUTPUT ERROR: Failed to save customers - {ex.Message}");
            }
        }

        /// <summary>
        /// FILE I/O: LOAD OPERATION - Reads customer list from JSON file
        /// This is the "INPUT" part of File I/O - data flows FROM file TO app
        /// 
        /// FLOW: File System ? JSON String ? Customer List
        /// 1. Check if customers.json file exists
        /// 2. Read JSON string from file
        /// 3. Deserialize JSON to customer objects
        /// 4. Populate in-memory customer list
        /// </summary>
        private async Task LoadCustomersFromFileAsync()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("FILE I/O INPUT: Starting load of customers from file...");
                
                // FILE I/O: Step 1 - Check if file exists in local storage folder
                var files = await _localFolder.GetFilesAsync();
                var customersFile = files.FirstOrDefault(f => f.Name == CUSTOMERS_FILE_NAME);
                
                if (customersFile != null)
                {
                    System.Diagnostics.Debug.WriteLine($"FILE I/O INPUT: Found existing file: {customersFile.Path}");
                    
                    // FILE I/O: Step 2 - Read JSON string from file using UWP FileIO API
                    string customersJson = await FileIO.ReadTextAsync(customersFile);
                    System.Diagnostics.Debug.WriteLine($"FILE I/O INPUT: Read JSON from file ({customersJson.Length} characters)");
                    
                    if (!string.IsNullOrEmpty(customersJson))
                    {
                        // FILE I/O: Step 3 - Convert JSON string back to customer objects
                        var loadedCustomers = JsonConvert.DeserializeObject<List<Customer>>(customersJson);
                        
                        if (loadedCustomers != null)
                        {
                            // FILE I/O: Step 4 - Replace in-memory list with loaded data
                            _customers.Clear();
                            _customers.AddRange(loadedCustomers);
                            System.Diagnostics.Debug.WriteLine($"FILE I/O INPUT: Successfully loaded {_customers.Count} customers from file");
                        }
                    }
                }
                else
                {
                    // FILE I/O: No existing file found - create initial data and save to file
                    System.Diagnostics.Debug.WriteLine("FILE I/O INPUT: No customers file found, creating initial sample data");
                    CreateSampleData();
                    await SaveCustomersToFileAsync();  // FILE I/O: Save initial data to create the file
                }
                
                // FILE I/O: Mark data as loaded to prevent redundant file operations
                _isLoaded = true;
            }
            catch (Exception ex)
            {
                // FILE I/O: Handle file read errors gracefully - fallback to sample data
                System.Diagnostics.Debug.WriteLine($"FILE I/O INPUT ERROR: Failed to load customers - {ex.Message}");
                CreateSampleData();  // FILE I/O: Fallback to default data if file loading fails
                _isLoaded = true;
            }
        }

        /// <summary>
        /// FILE I/O: Creates initial sample data when no file exists
        /// This ensures the app has some data to work with on first run
        /// </summary>
        private void CreateSampleData()
        {
            if (!_customers.Any())
            {
                _customers.AddRange(new[]
                {
                    new Customer 
                    { 
                        Id = 1, 
                        FirstName = "John", 
                        LastName = "Doe", 
                        Email = "john@example.com", 
                        Company = "Acme Corp",
                        DateCreated = DateTime.Now.AddDays(-30),
                        LastModified = DateTime.Now.AddDays(-5)
                    },
                    new Customer 
                    { 
                        Id = 2, 
                        FirstName = "Jane", 
                        LastName = "Smith", 
                        Email = "jane@example.com", 
                        Phone = "555-0123",
                        Company = "Tech Solutions",
                        DateCreated = DateTime.Now.AddDays(-20),
                        LastModified = DateTime.Now.AddDays(-2)
                    }
                });
                System.Diagnostics.Debug.WriteLine("FILE I/O: Created initial sample data (2 customers)");
            }
        }

        #endregion

        #region FILE I/O: CRUD Operations with Automatic File Persistence

        /// <summary>
        /// FILE I/O: GET/READ operation - Returns all customers, loading from file if needed
        /// This triggers the initial file load on first access
        /// 
        /// FLOW: Check if loaded ? If not, load from file ? Return customer list
        /// </summary>
        public async Task<List<Customer>> GetCustomersAsync()
        {
            // FILE I/O: Lazy loading - only load from file on first access
            if (!_isLoaded)
            {
                System.Diagnostics.Debug.WriteLine("FILE I/O: Data not loaded yet, loading from file...");
                await LoadCustomersFromFileAsync();  // FILE I/O: Load data from file
            }
            
            System.Diagnostics.Debug.WriteLine($"FILE I/O: Returning {_customers.Count} customers to UI");
            return _customers.ToList();  // FILE I/O: Return copy of in-memory data
        }

        /// <summary>
        /// FILE I/O: CREATE operation - Adds new customer and auto-saves to file
        /// This demonstrates the complete File I/O cycle: modify data ? save to file
        /// 
        /// FLOW: Add to memory ? Save to file ? Data persisted
        /// </summary>
        public async Task AddCustomerAsync(Customer customer)
        {
            // FILE I/O: Ensure data is loaded before modifying
            if (!_isLoaded)
            {
                await LoadCustomersFromFileAsync();
            }
            
            // Generate new ID and timestamps
            customer.Id = _customers.Any() ? _customers.Max(c => c.Id) + 1 : 1;
            customer.DateCreated = DateTime.Now;
            customer.LastModified = DateTime.Now;
            
            // Add to in-memory list
            _customers.Add(customer);
            System.Diagnostics.Debug.WriteLine($"FILE I/O: Added customer {customer.FullName} to memory (ID: {customer.Id})");
            
            // FILE I/O: AUTO-SAVE - Immediately persist changes to file
            await SaveCustomersToFileAsync();
            
            System.Diagnostics.Debug.WriteLine($"FILE I/O: Customer {customer.FullName} added and saved to file successfully");
        }

        /// <summary>
        /// FILE I/O: DELETE operation - Removes customer and auto-saves to file
        /// This demonstrates data removal with immediate persistence
        /// 
        /// FLOW: Remove from memory ? Save to file ? Data change persisted
        /// </summary>
        public async Task DeleteCustomerAsync(int customerId)
        {
            // FILE I/O: Ensure data is loaded before modifying
            if (!_isLoaded)
            {
                await LoadCustomersFromFileAsync();
            }
            
            var customer = _customers.FirstOrDefault(c => c.Id == customerId);
            if (customer != null)
            {
                // Remove from in-memory list
                _customers.Remove(customer);
                System.Diagnostics.Debug.WriteLine($"FILE I/O: Removed customer {customer.FullName} from memory (ID: {customerId})");
                
                // FILE I/O: AUTO-SAVE - Immediately persist changes to file
                await SaveCustomersToFileAsync();
                
                System.Diagnostics.Debug.WriteLine($"FILE I/O: Customer {customer.FullName} deleted and file updated successfully");
            }
        }

        /// <summary>
        /// FILE I/O: UPDATE operation - Modifies existing customer and auto-saves to file
        /// This demonstrates data modification with immediate persistence
        /// 
        /// FLOW: Update in memory ? Save to file ? Data change persisted
        /// </summary>
        public async Task UpdateCustomerAsync(Customer updatedCustomer)
        {
            // FILE I/O: Ensure data is loaded before modifying
            if (!_isLoaded)
            {
                await LoadCustomersFromFileAsync();
            }
            
            var existingCustomer = _customers.FirstOrDefault(c => c.Id == updatedCustomer.Id);
            if (existingCustomer != null)
            {
                // Update properties in memory
                existingCustomer.FirstName = updatedCustomer.FirstName;
                existingCustomer.LastName = updatedCustomer.LastName;
                existingCustomer.Email = updatedCustomer.Email;
                existingCustomer.Phone = updatedCustomer.Phone;
                existingCustomer.Company = updatedCustomer.Company;
                existingCustomer.LastModified = DateTime.Now;
                System.Diagnostics.Debug.WriteLine($"FILE I/O: Updated customer {existingCustomer.FullName} in memory (ID: {existingCustomer.Id})");
                
                // FILE I/O: AUTO-SAVE - Immediately persist changes to file
                await SaveCustomersToFileAsync();
                
                System.Diagnostics.Debug.WriteLine($"FILE I/O: Customer {existingCustomer.FullName} updated and saved to file successfully");
            }
        }

        /// <summary>
        /// FILE I/O: Helper method to get single customer by ID
        /// Ensures data is loaded from file before searching
        /// </summary>
        public async Task<Customer> GetCustomerByIdAsync(int customerId)
        {
            // FILE I/O: Ensure data is loaded before searching
            if (!_isLoaded)
            {
                await LoadCustomersFromFileAsync();
            }
            
            return _customers.FirstOrDefault(c => c.Id == customerId);
        }

        /// <summary>
        /// FILE I/O: Manual save operation for bulk changes
        /// Useful when making multiple changes and want to save once at the end
        /// </summary>
        public async Task SaveChangesAsync()
        {
            System.Diagnostics.Debug.WriteLine("FILE I/O: Manual save operation requested");
            await SaveCustomersToFileAsync();
        }

        /// <summary>
        /// FILE I/O: File information utility for debugging and user feedback
        /// Shows file details like size, modification date, and location
        /// </summary>
        public async Task<string> GetFileInfoAsync()
        {
            try
            {
                // FILE I/O: Query file system for file details
                var files = await _localFolder.GetFilesAsync();
                var customersFile = files.FirstOrDefault(f => f.Name == CUSTOMERS_FILE_NAME);
                
                if (customersFile != null)
                {
                    // FILE I/O: Get file properties (size, dates, etc.)
                    var properties = await customersFile.GetBasicPropertiesAsync();
                    return $"File: {CUSTOMERS_FILE_NAME}, Size: {properties.Size} bytes, Modified: {properties.DateModified}";
                }
                else
                {
                    return $"File: {CUSTOMERS_FILE_NAME} does not exist";
                }
            }
            catch (Exception ex)
            {
                return $"Error getting file info: {ex.Message}";
            }
        }

        #endregion
    }

    /// <summary>
    /// Simple dialog service for demo purposes
    /// </summary>
    public class DialogService
    {
        public async Task<Windows.UI.Xaml.Controls.ContentDialogResult> ShowConfirmationAsync(
            string title, string message, string primaryText, string secondaryText)
        {
            var dialog = new Windows.UI.Xaml.Controls.ContentDialog
            {
                Title = title,
                Content = message,
                PrimaryButtonText = primaryText,
                SecondaryButtonText = secondaryText
            };

            return await dialog.ShowAsync();
        }

        public async Task ShowMessageAsync(string title, string message)
        {
            var dialog = new Windows.UI.Xaml.Controls.ContentDialog
            {
                Title = title,
                Content = message,
                CloseButtonText = "OK"
            };

            await dialog.ShowAsync();
        }
    }

    /// <summary>
    /// Simple navigation service for demo purposes
    /// </summary>
    public class NavigationService
    {
        public void NavigateTo(Type pageType, object parameter = null)
        {
            var frame = Windows.UI.Xaml.Window.Current.Content as Windows.UI.Xaml.Controls.Frame;
            frame?.Navigate(pageType, parameter);
        }
    }
}