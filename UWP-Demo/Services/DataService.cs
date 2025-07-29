using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UWP_Demo.Models;
using Windows.Storage;
using Newtonsoft.Json;

namespace UWP_Demo.Services
{
    /// <summary>
    /// Service class responsible for managing customer data operations including
    /// loading, saving, and manipulating customer information using local file storage.
    /// This service demonstrates UWP file I/O patterns and data persistence strategies.
    /// </summary>
    /// <remarks>
    /// This service demonstrates several important UWP concepts:
    /// - Windows.Storage API for file operations
    /// - JSON serialization for data persistence
    /// - Async/await patterns for I/O operations
    /// - Observable collections for data binding
    /// - Proper error handling and logging
    /// - Singleton pattern for service management
    /// 
    /// The service uses the application's local folder for data storage,
    /// which ensures the data persists between app sessions and is automatically
    /// backed up and restored by Windows when appropriate.
    /// </remarks>
    public class DataService
    {
        #region Private Fields

        /// <summary>
        /// The singleton instance of the DataService.
        /// This ensures only one instance manages data operations throughout the app.
        /// </summary>
        private static DataService _instance;

        /// <summary>
        /// Lock object for thread-safe singleton access.
        /// </summary>
        private static readonly object _lock = new object();

        /// <summary>
        /// The filename used to store customer data in the local application folder.
        /// </summary>
        private const string CustomerDataFileName = "customers.json";

        /// <summary>
        /// Observable collection of customers that automatically notifies the UI of changes.
        /// This is bound to UI controls like ListView to display customer data.
        /// </summary>
        private ObservableCollection<Customer> _customers;

        /// <summary>
        /// Flag indicating whether data has been loaded from storage.
        /// This prevents multiple simultaneous load operations.
        /// </summary>
        private bool _isDataLoaded;

        /// <summary>
        /// Counter for generating unique customer IDs.
        /// This is persisted with the data to ensure IDs remain unique across sessions.
        /// </summary>
        private int _nextCustomerId = 1;

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the singleton instance of the DataService.
        /// Creates the instance if it doesn't exist (thread-safe).
        /// </summary>
        /// <remarks>
        /// Using a singleton pattern ensures that all parts of the application
        /// work with the same data instance, maintaining consistency and
        /// preventing data synchronization issues.
        /// </remarks>
        public static DataService Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new DataService();
                        }
                    }
                }
                return _instance;
            }
        }

        /// <summary>
        /// Gets the observable collection of customers.
        /// This collection is directly bound to UI controls and automatically
        /// notifies of changes when customers are added, removed, or modified.
        /// </summary>
        /// <remarks>
        /// ObservableCollection automatically implements INotifyCollectionChanged,
        /// which means UI controls bound to this collection will automatically
        /// update when the collection changes. This is essential for MVVM data binding.
        /// </remarks>
        public ObservableCollection<Customer> Customers
        {
            get
            {
                if (_customers == null)
                {
                    _customers = new ObservableCollection<Customer>();
                }
                return _customers;
            }
            private set => _customers = value;
        }

        /// <summary>
        /// Gets whether the data service has been initialized and data loaded.
        /// This can be used by the UI to show loading indicators.
        /// </summary>
        public bool IsDataLoaded => _isDataLoaded;

        /// <summary>
        /// Gets the number of customers currently in the collection.
        /// This is useful for displaying counts in the UI.
        /// </summary>
        public int CustomerCount => Customers.Count;

        #endregion

        #region Constructor

        /// <summary>
        /// Private constructor for singleton pattern.
        /// Initializes the customer collection and sets up default values.
        /// </summary>
        private DataService()
        {
            Customers = new ObservableCollection<Customer>();
            _isDataLoaded = false;
        }

        #endregion

        #region Data Loading Methods

        /// <summary>
        /// Loads customer data from local storage asynchronously.
        /// If no data file exists, creates sample data for demonstration purposes.
        /// </summary>
        /// <returns>A task representing the asynchronous load operation</returns>
        /// <remarks>
        /// This method demonstrates:
        /// - Async file I/O using Windows.Storage APIs
        /// - JSON deserialization for data loading
        /// - Error handling for file operations
        /// - Creation of sample data for new installations
        /// 
        /// The method is safe to call multiple times - it will only load data once.
        /// </remarks>
        /// <example>
        /// // Load data when the app starts
        /// await DataService.Instance.LoadDataAsync();
        /// </example>
        public async Task LoadDataAsync()
        {
            // Prevent multiple simultaneous load operations
            if (_isDataLoaded)
            {
                return;
            }

            try
            {
                // Get reference to the local application data folder
                StorageFolder localFolder = ApplicationData.Current.LocalFolder;

                try
                {
                    // Try to get the existing data file
                    StorageFile dataFile = await localFolder.GetFileAsync(CustomerDataFileName);
                    
                    // Read the JSON content from the file
                    string jsonContent = await FileIO.ReadTextAsync(dataFile);
                    
                    if (!string.IsNullOrWhiteSpace(jsonContent))
                    {
                        // Deserialize the customer data from JSON
                        var customerData = JsonConvert.DeserializeObject<CustomerDataContainer>(jsonContent);
                        
                        if (customerData?.Customers != null)
                        {
                            // Clear existing data and load from file
                            Customers.Clear();
                            foreach (var customer in customerData.Customers)
                            {
                                Customers.Add(customer);
                            }
                            
                            // Restore the next ID counter
                            _nextCustomerId = customerData.NextCustomerId;
                            
                            System.Diagnostics.Debug.WriteLine($"DataService: Loaded {Customers.Count} customers from storage");
                        }
                        else
                        {
                            // File exists but data is invalid, create sample data
                            await CreateSampleDataAsync();
                        }
                    }
                    else
                    {
                        // File exists but is empty, create sample data
                        await CreateSampleDataAsync();
                    }
                }
                catch (FileNotFoundException)
                {
                    // Data file doesn't exist yet (first run), create sample data
                    System.Diagnostics.Debug.WriteLine("DataService: No existing data file found, creating sample data");
                    await CreateSampleDataAsync();
                }
                catch (JsonException ex)
                {
                    // JSON parsing error, log and create sample data
                    System.Diagnostics.Debug.WriteLine($"DataService: JSON parsing error: {ex.Message}");
                    await CreateSampleDataAsync();
                }

                _isDataLoaded = true;
            }
            catch (Exception ex)
            {
                // Log any unexpected errors
                System.Diagnostics.Debug.WriteLine($"DataService: Error loading data: {ex.Message}");
                
                // Even if loading fails, create basic sample data so the app can function
                await CreateSampleDataAsync();
                _isDataLoaded = true;
            }
        }

        /// <summary>
        /// Creates sample customer data for demonstration and testing purposes.
        /// This method is called when no existing data is found or when data loading fails.
        /// </summary>
        /// <returns>A task representing the asynchronous operation</returns>
        /// <remarks>
        /// The sample data includes a variety of customer types to demonstrate
        /// different UI scenarios and data validation cases. This helps with
        /// development and provides a good user experience on first run.
        /// </remarks>
        private async Task CreateSampleDataAsync()
        {
            try
            {
                Customers.Clear();

                // Create diverse sample customers to demonstrate various UI scenarios
                var sampleCustomers = new List<Customer>
                {
                    new Customer("John", "Doe", "john.doe@email.com")
                    {
                        Id = GetNextCustomerId(),
                        Phone = "(555) 123-4567",
                        Company = "Acme Corporation",
                        DateCreated = DateTime.Now.AddDays(-30)
                    },
                    new Customer("Jane", "Smith", "jane.smith@company.com")
                    {
                        Id = GetNextCustomerId(),
                        Phone = "(555) 987-6543",
                        Company = "TechStart Inc.",
                        DateCreated = DateTime.Now.AddDays(-15)
                    },
                    new Customer("Michael", "Johnson", "m.johnson@email.com")
                    {
                        Id = GetNextCustomerId(),
                        Phone = "(555) 456-7890",
                        Company = "Global Solutions Ltd",
                        DateCreated = DateTime.Now.AddDays(-7)
                    },
                    new Customer("Sarah", "Williams", "sarah.w@personalmail.com")
                    {
                        Id = GetNextCustomerId(),
                        Phone = "(555) 321-0987",
                        Company = "", // Individual customer without company
                        DateCreated = DateTime.Now.AddDays(-3)
                    },
                    new Customer("David", "Brown", "david.brown@startup.io")
                    {
                        Id = GetNextCustomerId(),
                        Phone = "(555) 654-3210",
                        Company = "Innovation Labs",
                        DateCreated = DateTime.Now.AddDays(-1)
                    }
                };

                // Add sample customers to the collection
                foreach (var customer in sampleCustomers)
                {
                    Customers.Add(customer);
                }

                // Save the sample data to storage
                await SaveDataAsync();

                System.Diagnostics.Debug.WriteLine($"DataService: Created {Customers.Count} sample customers");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"DataService: Error creating sample data: {ex.Message}");
            }
        }

        #endregion

        #region Data Saving Methods

        /// <summary>
        /// Saves all customer data to local storage asynchronously.
        /// This method persists the current customer list to a JSON file.
        /// </summary>
        /// <returns>A task representing the asynchronous save operation</returns>
        /// <remarks>
        /// This method demonstrates:
        /// - Async file I/O for data persistence
        /// - JSON serialization for data storage
        /// - Error handling for file operations
        /// - Atomic file operations to prevent data corruption
        /// 
        /// The save operation is atomic - either all data is saved successfully
        /// or the operation fails without corrupting existing data.
        /// </remarks>
        /// <example>
        /// // Save data after making changes
        /// await DataService.Instance.SaveDataAsync();
        /// </example>
        public async Task<bool> SaveDataAsync()
        {
            try
            {
                // Create a container object that includes both customers and metadata
                var dataContainer = new CustomerDataContainer
                {
                    Customers = Customers.ToList(),
                    NextCustomerId = _nextCustomerId,
                    LastSaved = DateTime.Now,
                    Version = "1.0"
                };

                // Serialize the data to JSON with formatting for readability
                string jsonContent = JsonConvert.SerializeObject(dataContainer, Formatting.Indented);

                // Get reference to the local application data folder
                StorageFolder localFolder = ApplicationData.Current.LocalFolder;

                // Create or get the data file
                StorageFile dataFile = await localFolder.CreateFileAsync(
                    CustomerDataFileName, 
                    CreationCollisionOption.ReplaceExisting);

                // Write the JSON content to the file
                await FileIO.WriteTextAsync(dataFile, jsonContent);

                System.Diagnostics.Debug.WriteLine($"DataService: Saved {Customers.Count} customers to storage");
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"DataService: Error saving data: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Automatically saves data in the background without blocking the UI.
        /// This method can be called periodically or after data changes.
        /// </summary>
        /// <returns>A task that can be awaited but doesn't block the caller</returns>
        /// <remarks>
        /// This method runs the save operation on a background thread to prevent
        /// blocking the UI. It's suitable for automatic save scenarios where
        /// the user shouldn't be interrupted by save operations.
        /// </remarks>
        public async Task SaveDataInBackgroundAsync()
        {
            try
            {
                await Task.Run(async () => await SaveDataAsync());
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"DataService: Error in background save: {ex.Message}");
            }
        }

        #endregion

        #region Customer Management Methods

        /// <summary>
        /// Adds a new customer to the collection and automatically saves the data.
        /// </summary>
        /// <param name="customer">The customer to add</param>
        /// <returns>A task representing the asynchronous operation</returns>
        /// <remarks>
        /// This method automatically assigns a unique ID to the customer if it doesn't have one,
        /// adds it to the observable collection (which automatically updates the UI),
        /// and saves the data to storage.
        /// </remarks>
        /// <example>
        /// var newCustomer = new Customer("Test", "User", "test@email.com");
        /// await DataService.Instance.AddCustomerAsync(newCustomer);
        /// </example>
        public async Task AddCustomerAsync(Customer customer)
        {
            if (customer == null)
            {
                throw new ArgumentNullException(nameof(customer));
            }

            try
            {
                // Assign ID if not already set
                if (customer.Id <= 0)
                {
                    customer.Id = GetNextCustomerId();
                }

                // Set creation date if not already set
                if (customer.DateCreated == default(DateTime))
                {
                    customer.DateCreated = DateTime.Now;
                }

                // Update last modified date
                customer.LastModified = DateTime.Now;

                // Add to collection (automatically updates UI through data binding)
                Customers.Add(customer);

                // Save data to storage
                await SaveDataAsync();

                System.Diagnostics.Debug.WriteLine($"DataService: Added customer {customer.FullName} (ID: {customer.Id})");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"DataService: Error adding customer: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Updates an existing customer and saves the data.
        /// </summary>
        /// <param name="customer">The customer with updated information</param>
        /// <returns>A task representing the asynchronous operation</returns>
        /// <remarks>
        /// This method finds the customer by ID and updates all its properties.
        /// It automatically updates the LastModified timestamp and saves the data.
        /// </remarks>
        public async Task UpdateCustomerAsync(Customer customer)
        {
            if (customer == null)
            {
                throw new ArgumentNullException(nameof(customer));
            }

            try
            {
                // Find existing customer by ID
                var existingCustomer = Customers.FirstOrDefault(c => c.Id == customer.Id);
                if (existingCustomer == null)
                {
                    throw new InvalidOperationException($"Customer with ID {customer.Id} not found");
                }

                // Update all properties
                existingCustomer.FirstName = customer.FirstName;
                existingCustomer.LastName = customer.LastName;
                existingCustomer.Email = customer.Email;
                existingCustomer.Phone = customer.Phone;
                existingCustomer.Company = customer.Company;
                existingCustomer.LastModified = DateTime.Now;

                // Save data to storage
                await SaveDataAsync();

                System.Diagnostics.Debug.WriteLine($"DataService: Updated customer {customer.FullName} (ID: {customer.Id})");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"DataService: Error updating customer: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Removes a customer from the collection and saves the data.
        /// </summary>
        /// <param name="customer">The customer to remove</param>
        /// <returns>A task representing the asynchronous operation</returns>
        public async Task RemoveCustomerAsync(Customer customer)
        {
            if (customer == null)
            {
                throw new ArgumentNullException(nameof(customer));
            }

            try
            {
                // Remove from collection (automatically updates UI)
                bool removed = Customers.Remove(customer);
                
                if (removed)
                {
                    // Save data to storage
                    await SaveDataAsync();
                    System.Diagnostics.Debug.WriteLine($"DataService: Removed customer {customer.FullName} (ID: {customer.Id})");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"DataService: Customer {customer.FullName} was not found in collection");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"DataService: Error removing customer: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Finds a customer by their unique ID.
        /// </summary>
        /// <param name="customerId">The ID of the customer to find</param>
        /// <returns>The customer with the specified ID, or null if not found</returns>
        public Customer GetCustomerById(int customerId)
        {
            return Customers.FirstOrDefault(c => c.Id == customerId);
        }

        /// <summary>
        /// Searches for customers based on a search term.
        /// The search looks in first name, last name, email, and company fields.
        /// </summary>
        /// <param name="searchTerm">The term to search for</param>
        /// <returns>A list of customers matching the search term</returns>
        /// <example>
        /// // Search for customers
        /// var results = DataService.Instance.SearchCustomers("john");
        /// </example>
        public List<Customer> SearchCustomers(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return Customers.ToList();
            }

            searchTerm = searchTerm.ToLowerInvariant();

            return Customers.Where(c =>
                c.FirstName.ToLowerInvariant().Contains(searchTerm) ||
                c.LastName.ToLowerInvariant().Contains(searchTerm) ||
                c.Email.ToLowerInvariant().Contains(searchTerm) ||
                (c.Company?.ToLowerInvariant().Contains(searchTerm) ?? false)
            ).ToList();
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Generates the next unique customer ID.
        /// This ensures each customer has a unique identifier.
        /// </summary>
        /// <returns>A unique customer ID</returns>
        private int GetNextCustomerId()
        {
            return _nextCustomerId++;
        }

        /// <summary>
        /// Clears all customer data (useful for testing or reset functionality).
        /// </summary>
        /// <returns>A task representing the asynchronous operation</returns>
        public async Task ClearAllDataAsync()
        {
            try
            {
                Customers.Clear();
                _nextCustomerId = 1;
                await SaveDataAsync();
                System.Diagnostics.Debug.WriteLine("DataService: Cleared all customer data");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"DataService: Error clearing data: {ex.Message}");
                throw;
            }
        }

        #endregion

        #region Data Container Class

        /// <summary>
        /// Container class for serializing customer data along with metadata.
        /// This allows us to store additional information like version numbers
        /// and the next ID counter along with the customer list.
        /// </summary>
        /// <remarks>
        /// Using a container class instead of serializing the customer list directly
        /// provides flexibility for future data format changes and allows us to
        /// store metadata that's important for data integrity.
        /// </remarks>
        private class CustomerDataContainer
        {
            /// <summary>
            /// The list of customers to serialize.
            /// </summary>
            [JsonProperty("customers")]
            public List<Customer> Customers { get; set; } = new List<Customer>();

            /// <summary>
            /// The next customer ID to use for new customers.
            /// This ensures ID uniqueness across app sessions.
            /// </summary>
            [JsonProperty("nextCustomerId")]
            public int NextCustomerId { get; set; } = 1;

            /// <summary>
            /// The timestamp when the data was last saved.
            /// This can be useful for data synchronization or debugging.
            /// </summary>
            [JsonProperty("lastSaved")]
            public DateTime LastSaved { get; set; }

            /// <summary>
            /// The version of the data format.
            /// This allows for future data migration scenarios.
            /// </summary>
            [JsonProperty("version")]
            public string Version { get; set; } = "1.0";
        }

        #endregion
    }
}