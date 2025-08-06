using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UWP_Demo.Models;
using System.Collections.ObjectModel;
using Windows.Storage;
using Newtonsoft.Json;

namespace UWP_Demo.Services
{
    /*
    /// <summary>
    /// HTTP service for external API communication - COMMENTED OUT
    /// </summary>
    public class HttpApiService
    {
        // ... [Entire class commented out for demonstration purposes] ...
    }
    */

    /// <summary>
    /// Simple dialog service for showing user messages and confirmations
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
    /// Simple navigation service for page navigation
    /// </summary>
    public class NavigationService
    {
        public void NavigateTo(Type pageType, object parameter = null)
        {
            var frame = Windows.UI.Xaml.Window.Current.Content as Windows.UI.Xaml.Controls.Frame;
            frame?.Navigate(pageType, parameter);
        }
    }

    /// <summary>
    /// 7. Error Simulation & Handling: Service for simulating various file errors and logging them
    /// Features: File access errors, JSON parsing errors, permission errors, disk space simulation
    /// Logging: Both to screen and to persistent log file
    /// </summary>
    public class ErrorSimulationService
    {
        private static ErrorSimulationService _instance;
        private readonly StorageFolder _localFolder;
        private readonly List<ErrorLogEntry> _errorLog;
        private const string ERROR_LOG_FILE = "error_log.txt";
        private const string TEST_FILE_NAME = "nonexistent_file.json";

        public static ErrorSimulationService Instance => _instance ?? (_instance = new ErrorSimulationService());

        private ErrorSimulationService()
        {
            _localFolder = ApplicationData.Current.LocalFolder;
            _errorLog = new List<ErrorLogEntry>();
            System.Diagnostics.Debug.WriteLine("?? ERROR SIMULATION: Service initialized");
        }

        #region Error Log Management

        /// <summary>
        /// Log error to both debug output and persistent file
        /// </summary>
        private async Task LogErrorAsync(string errorType, string operation, Exception exception, string additionalInfo = "")
        {
            var logEntry = new ErrorLogEntry
            {
                Timestamp = DateTime.Now,
                ErrorType = errorType,
                Operation = operation,
                Message = exception?.Message ?? "Unknown error",
                StackTrace = exception?.StackTrace ?? "",
                AdditionalInfo = additionalInfo
            };

            // Add to in-memory log
            _errorLog.Add(logEntry);

            // Log to debug output (screen)
            System.Diagnostics.Debug.WriteLine($"?? === ERROR LOGGED ===");
            System.Diagnostics.Debug.WriteLine($"?? Time: {logEntry.Timestamp:yyyy-MM-dd HH:mm:ss.fff}");
            System.Diagnostics.Debug.WriteLine($"?? Type: {errorType}");
            System.Diagnostics.Debug.WriteLine($"?? Operation: {operation}");
            System.Diagnostics.Debug.WriteLine($"?? Message: {logEntry.Message}");
            if (!string.IsNullOrEmpty(additionalInfo))
            {
                System.Diagnostics.Debug.WriteLine($"?? Info: {additionalInfo}");
            }
            System.Diagnostics.Debug.WriteLine($"?? === END ERROR LOG ===");

            // Save to persistent log file
            await SaveErrorToFileAsync(logEntry);
        }

        /// <summary>
        /// Save error to persistent log file
        /// </summary>
        private async Task SaveErrorToFileAsync(ErrorLogEntry logEntry)
        {
            try
            {
                var logText = $"[{logEntry.Timestamp:yyyy-MM-dd HH:mm:ss}] {logEntry.ErrorType} - {logEntry.Operation}\n" +
                             $"Message: {logEntry.Message}\n" +
                             $"Additional Info: {logEntry.AdditionalInfo}\n" +
                             $"Stack Trace: {logEntry.StackTrace}\n" +
                             $"----------------------------------------\n";

                // Get or create log file
                var logFile = await _localFolder.CreateFileAsync(ERROR_LOG_FILE, CreationCollisionOption.OpenIfExists);
                
                // Append to existing content
                var existingContent = "";
                try
                {
                    existingContent = await FileIO.ReadTextAsync(logFile);
                }
                catch { /* File might not exist yet */ }

                await FileIO.WriteTextAsync(logFile, existingContent + logText);
                
                System.Diagnostics.Debug.WriteLine($"?? ERROR SIMULATION: Error logged to file {ERROR_LOG_FILE}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"?? ERROR SIMULATION: Failed to save error to file - {ex.Message}");
            }
        }

        #endregion

        #region Error Simulation Methods

        /// <summary>
        /// Simulate trying to open a file that doesn't exist
        /// </summary>
        public async Task<string> SimulateFileNotFoundErrorAsync()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("?? ERROR SIMULATION: Attempting to open non-existent file...");
                
                // Try to access a file that doesn't exist
                var nonExistentFile = await _localFolder.GetFileAsync("this_file_does_not_exist.json");
                var content = await FileIO.ReadTextAsync(nonExistentFile);
                
                return "This should never be reached";
            }
            catch (System.IO.FileNotFoundException ex)
            {
                await LogErrorAsync("FileNotFoundException", "Reading non-existent file", ex, 
                    "Attempted to read 'this_file_does_not_exist.json'");
                return $"? Successfully caught FileNotFoundException: {ex.Message}";
            }
            catch (Exception ex)
            {
                await LogErrorAsync("UnexpectedError", "Reading non-existent file", ex,
                    "Expected FileNotFoundException but got different error");
                return $"?? Unexpected error type: {ex.GetType().Name} - {ex.Message}";
            }
        }

        /// <summary>
        /// Simulate JSON parsing error
        /// </summary>
        public async Task<string> SimulateJsonParsingErrorAsync()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("?? ERROR SIMULATION: Creating invalid JSON and attempting to parse...");
                
                // Create a file with invalid JSON content
                var invalidJsonFile = await _localFolder.CreateFileAsync("invalid.json", CreationCollisionOption.ReplaceExisting);
                var invalidJson = "{ invalid json content: missing quotes and brackets";
                await FileIO.WriteTextAsync(invalidJsonFile, invalidJson);
                
                // Try to parse the invalid JSON
                var content = await FileIO.ReadTextAsync(invalidJsonFile);
                var customers = JsonConvert.DeserializeObject<List<Customer>>(content);
                
                return "This should never be reached";
            }
            catch (JsonException ex)
            {
                await LogErrorAsync("JsonParsingError", "Parsing invalid JSON", ex,
                    "Attempted to parse malformed JSON content");
                return $"? Successfully caught JsonException: {ex.Message}";
            }
            catch (Exception ex)
            {
                await LogErrorAsync("UnexpectedError", "Parsing invalid JSON", ex,
                    "Expected JsonException but got different error");
                return $"?? Unexpected error type: {ex.GetType().Name} - {ex.Message}";
            }
        }

        /// <summary>
        /// Simulate access denied error
        /// </summary>
        public async Task<string> SimulateAccessDeniedErrorAsync()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("?? ERROR SIMULATION: Attempting to access restricted location...");
                
                // Try to access a restricted folder (this will fail in UWP)
                var restrictedFolder = await StorageFolder.GetFolderFromPathAsync("C:\\Windows\\System32");
                var files = await restrictedFolder.GetFilesAsync();
                
                return "This should never be reached";
            }
            catch (UnauthorizedAccessException ex)
            {
                await LogErrorAsync("UnauthorizedAccessException", "Accessing restricted folder", ex,
                    "Attempted to access C:\\Windows\\System32");
                return $"? Successfully caught UnauthorizedAccessException: {ex.Message}";
            }
            catch (ArgumentException ex)
            {
                await LogErrorAsync("ArgumentException", "Invalid path access", ex,
                    "UWP apps cannot access arbitrary file system paths");
                return $"? UWP Security: Path access denied - {ex.Message}";
            }
            catch (Exception ex)
            {
                await LogErrorAsync("UnexpectedError", "Accessing restricted folder", ex,
                    "Expected UnauthorizedAccessException but got different error");
                return $"?? Unexpected error type: {ex.GetType().Name} - {ex.Message}";
            }
        }

        /// <summary>
        /// Simulate corrupted file error
        /// </summary>
        public async Task<string> SimulateCorruptedFileErrorAsync()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("?? ERROR SIMULATION: Creating corrupted JSON file...");
                
                // Create a file with partially corrupted JSON
                var corruptedFile = await _localFolder.CreateFileAsync("corrupted_customers.json", CreationCollisionOption.ReplaceExisting);
                var corruptedJson = "[{\"Id\":1,\"FirstName\":\"John\",\"LastName\":null,\"Email\":\"invalid-email\",\"DateCreated\":\"not-a-date\"}]";
                await FileIO.WriteTextAsync(corruptedFile, corruptedJson);
                
                // Try to parse and validate the corrupted data
                var content = await FileIO.ReadTextAsync(corruptedFile);
                var customers = JsonConvert.DeserializeObject<List<Customer>>(content);
                
                // Validate the data (this will find issues)
                foreach (var customer in customers)
                {
                    if (string.IsNullOrEmpty(customer.FirstName) || string.IsNullOrEmpty(customer.LastName))
                    {
                        throw new InvalidDataException($"Customer {customer.Id} has invalid name data");
                    }
                    if (!customer.Email.Contains("@"))
                    {
                        throw new InvalidDataException($"Customer {customer.Id} has invalid email format");
                    }
                }
                
                return "Data validation passed (unexpected)";
            }
            catch (InvalidDataException ex)
            {
                await LogErrorAsync("InvalidDataException", "Validating corrupted customer data", ex,
                    "Customer data failed to validate against schema");
                return $"? Successfully caught InvalidDataException: {ex.Message}";
            }
            catch (Exception ex)
            {
                await LogErrorAsync("UnexpectedError", "Processing corrupted file", ex,
                    "Unexpected error during corrupted file processing");
                return $"?? Unexpected error type: {ex.GetType().Name} - {ex.Message}";
            }
        }

        /// <summary>
        /// Simulate network-related file error
        /// </summary>
        public async Task<string> SimulateNetworkFileErrorAsync()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("?? ERROR SIMULATION: Attempting to access network location...");
                
                // Try to access a network path (will fail in UWP sandbox)
                var networkPath = "\\\\networkserver\\shared\\file.txt";
                var networkFile = await StorageFile.GetFileFromPathAsync(networkPath);
                var content = await FileIO.ReadTextAsync(networkFile);
                
                return "This should never be reached";
            }
            catch (ArgumentException ex)
            {
                await LogErrorAsync("ArgumentException", "Accessing network path", ex,
                    $"UWP cannot access network paths directly");
                return $"? UWP Limitation: Network path access denied - {ex.Message}";
            }
            catch (Exception ex)
            {
                await LogErrorAsync("UnexpectedError", "Accessing network path", ex,
                    "Unexpected error during network path access");
                return $"?? Unexpected error type: {ex.GetType().Name} - {ex.Message}";
            }
        }

        #endregion

        #region Simple File Access Examples

        /// <summary>
        /// Simple example: Try to open a file that doesn't exist and gracefully catch the exception
        /// This is a basic implementation for educational purposes
        /// </summary>
        public async Task<string> TryOpenNonExistentFileAsync(string fileName = "missing_file.txt")
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"?? Attempting to open file: {fileName}");
                
                // Try to access a file that doesn't exist
                var file = await _localFolder.GetFileAsync(fileName);
                var content = await FileIO.ReadTextAsync(file);
                
                // If we get here, the file actually exists (unexpected)
                return $"? File found! Content length: {content.Length} characters";
            }
            catch (System.IO.FileNotFoundException ex)
            {
                // This is the expected exception when file doesn't exist
                System.Diagnostics.Debug.WriteLine($"? File not found (as expected): {ex.Message}");
                return $"? File '{fileName}' not found: {ex.Message}";
            }
            catch (Exception ex)
            {
                // Handle any other unexpected exceptions
                System.Diagnostics.Debug.WriteLine($"?? Unexpected error: {ex.GetType().Name} - {ex.Message}");
                return $"?? Unexpected error: {ex.GetType().Name} - {ex.Message}";
            }
        }

        /// <summary>
        /// Create a test file and then delete it to demonstrate file operations
        /// </summary>
        public async Task<string> CreateAndDeleteFileExampleAsync()
        {
            var fileName = "temp_test_file.txt";
            try
            {
                System.Diagnostics.Debug.WriteLine($"?? Creating test file: {fileName}");
                
                // Create a test file
                var testFile = await _localFolder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);
                await FileIO.WriteTextAsync(testFile, "This is a test file created for demonstration.");
                
                System.Diagnostics.Debug.WriteLine($"? File created successfully");
                
                // Read the file to confirm it exists
                var content = await FileIO.ReadTextAsync(testFile);
                System.Diagnostics.Debug.WriteLine($"?? File content: {content}");
                
                // Delete the file
                await testFile.DeleteAsync();
                System.Diagnostics.Debug.WriteLine($"??? File deleted successfully");
                
                // Now try to access the deleted file (should throw FileNotFoundException)
                return await TryOpenNonExistentFileAsync(fileName);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"? Error in create/delete example: {ex.Message}");
                return $"? Error: {ex.Message}";
            }
        }

        /// <summary>
        /// Test file existence before attempting to open
        /// This shows a defensive programming approach
        /// </summary>
        public async Task<string> SafeFileAccessExampleAsync(string fileName = "safe_test_file.txt")
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"?? Checking if file exists: {fileName}");
                
                // Method 1: Try to get files and check if our file is in the list
                var files = await _localFolder.GetFilesAsync();
                var targetFile = files.FirstOrDefault(f => f.Name.Equals(fileName, StringComparison.OrdinalIgnoreCase));
                
                if (targetFile != null)
                {
                    // File exists, safe to read
                    var content = await FileIO.ReadTextAsync(targetFile);
                    System.Diagnostics.Debug.WriteLine($"? File found and read successfully");
                    return $"? File exists! Content length: {content.Length} characters";
                }
                else
                {
                    // File doesn't exist, handle gracefully
                    System.Diagnostics.Debug.WriteLine($"?? File not found in directory listing");
                    return $"?? File '{fileName}' does not exist (checked safely)";
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"? Error in safe file access: {ex.Message}");
                return $"? Error: {ex.Message}";
            }
        }

        #endregion

        #region Error Log Retrieval

        /// <summary>
        /// Get all errors from current session
        /// </summary>
        public List<ErrorLogEntry> GetSessionErrors()
        {
            return _errorLog.ToList();
        }

        /// <summary>
        /// Get error log content from file
        /// </summary>
        public async Task<string> GetErrorLogFileContentAsync()
        {
            try
            {
                var logFile = await _localFolder.GetFileAsync(ERROR_LOG_FILE);
                var content = await FileIO.ReadTextAsync(logFile);
                return content;
            }
            catch (Exception ex)
            {
                return $"Error reading log file: {ex.Message}";
            }
        }

        /// <summary>
        /// Clear error log
        /// </summary>
        public async Task ClearErrorLogAsync()
        {
            try
            {
                _errorLog.Clear();
                var logFile = await _localFolder.CreateFileAsync(ERROR_LOG_FILE, CreationCollisionOption.ReplaceExisting);
                await FileIO.WriteTextAsync(logFile, "Error log cleared at " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "\n");
                System.Diagnostics.Debug.WriteLine("?? ERROR SIMULATION: Error log cleared");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"?? ERROR SIMULATION: Failed to clear error log - {ex.Message}");
            }
        }

        /// <summary>
        /// Get error statistics
        /// </summary>
        public string GetErrorStatistics()
        {
            var errorTypes = _errorLog.GroupBy(e => e.ErrorType)
                                    .Select(g => $"{g.Key}: {g.Count()}")
                                    .ToList();

            return $"Total Errors: {_errorLog.Count}\n" +
                   $"Error Types:\n{string.Join("\n", errorTypes)}\n" +
                   $"Last Error: {(_errorLog.LastOrDefault()?.Timestamp.ToString("HH:mm:ss") ?? "None")}";
        }

        #endregion
    }

    /// <summary>
    /// 7. Error Simulation & Handling: Error log entry model
    /// </summary>
    public class ErrorLogEntry
    {
        public DateTime Timestamp { get; set; }
        public string ErrorType { get; set; }
        public string Operation { get; set; }
        public string Message { get; set; }
        public string StackTrace { get; set; }
        public string AdditionalInfo { get; set; }
    }

    /// <summary>
    /// Custom exception for data validation errors
    /// </summary>
    public class InvalidDataException : Exception
    {
        public InvalidDataException(string message) : base(message) { }
        public InvalidDataException(string message, Exception innerException) : base(message, innerException) { }
    }

    /// <summary>
    /// Customer service with file persistence functionality.
    /// Automatically saves and loads customer data to/from JSON file using UWP Storage APIs.
    /// </summary>
    public class CustomerService
    {
        private readonly List<Customer> _customers = new List<Customer>();
        private const string CUSTOMERS_FILE_NAME = "customers.json";
        private StorageFolder _localFolder;
        private bool _isLoaded = false;

        public CustomerService()
        {
            _localFolder = ApplicationData.Current.LocalFolder;
            System.Diagnostics.Debug.WriteLine("CustomerService: Initialized with file persistence support");
        }

        #region Core File Operations

        /// <summary>
        /// Writes customer list to JSON file
        /// </summary>
        private async Task SaveCustomersToFileAsync()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"Starting save of {_customers.Count} customers to file...");
                
                string customersJson = JsonConvert.SerializeObject(_customers, Formatting.Indented);
                System.Diagnostics.Debug.WriteLine($"Serialized customers to JSON ({customersJson.Length} characters)");
                
                StorageFile customersFile = await _localFolder.CreateFileAsync(
                    CUSTOMERS_FILE_NAME, 
                    CreationCollisionOption.ReplaceExisting);
                System.Diagnostics.Debug.WriteLine($"Created/accessed file: {customersFile.Path}");
                
                await FileIO.WriteTextAsync(customersFile, customersJson);
                
                System.Diagnostics.Debug.WriteLine($"Successfully saved {_customers.Count} customers to {CUSTOMERS_FILE_NAME}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to save customers - {ex.Message}");
            }
        }

        /// <summary>
        /// Reads customer list from JSON file
        /// </summary>
        private async Task LoadCustomersFromFileAsync()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("Starting load of customers from file...");
                
                var files = await _localFolder.GetFilesAsync();
                var customersFile = files.FirstOrDefault(f => f.Name == CUSTOMERS_FILE_NAME);
                
                if (customersFile != null)
                {
                    System.Diagnostics.Debug.WriteLine($"Found existing file: {customersFile.Path}");
                    
                    string customersJson = await FileIO.ReadTextAsync(customersFile);
                    System.Diagnostics.Debug.WriteLine($"Read JSON from file ({customersJson.Length} characters)");
                    
                    if (!string.IsNullOrEmpty(customersJson))
                    {
                        var loadedCustomers = JsonConvert.DeserializeObject<List<Customer>>(customersJson);
                        
                        if (loadedCustomers != null)
                        {
                            _customers.Clear();
                            _customers.AddRange(loadedCustomers);
                            System.Diagnostics.Debug.WriteLine($"Successfully loaded {_customers.Count} customers from file");
                        }
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("No customers file found, creating initial sample data");
                    CreateSampleData();
                    await SaveCustomersToFileAsync();
                }
                
                _isLoaded = true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load customers - {ex.Message}");
                CreateSampleData();
                _isLoaded = true;
            }
        }

        /// <summary>
        /// Creates initial sample data when no file exists
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
                System.Diagnostics.Debug.WriteLine("Created initial sample data (2 customers)");
            }
        }

        #endregion

        #region CRUD Operations with Automatic File Persistence

        /// <summary>
        /// Returns all customers, loading from file if needed
        /// </summary>
        public async Task<List<Customer>> GetCustomersAsync()
        {
            if (!_isLoaded)
            {
                System.Diagnostics.Debug.WriteLine("Data not loaded yet, loading from file...");
                await LoadCustomersFromFileAsync();
            }
            
            System.Diagnostics.Debug.WriteLine($"Returning {_customers.Count} customers to UI");
            return _customers.ToList();
        }

        /// <summary>
        /// Adds new customer and auto-saves to file
        /// </summary>
        public async Task AddCustomerAsync(Customer customer)
        {
            if (!_isLoaded)
            {
                await LoadCustomersFromFileAsync();
            }
            
            customer.Id = _customers.Any() ? _customers.Max(c => c.Id) + 1 : 1;
            customer.DateCreated = DateTime.Now;
            customer.LastModified = DateTime.Now;
            
            _customers.Add(customer);
            System.Diagnostics.Debug.WriteLine($"Added customer {customer.FullName} to memory (ID: {customer.Id})");
            
            await SaveCustomersToFileAsync();
            
            System.Diagnostics.Debug.WriteLine($"Customer {customer.FullName} added and saved to file successfully");
        }

        /// <summary>
        /// Removes customer and auto-saves to file
        /// </summary>
        public async Task DeleteCustomerAsync(int customerId)
        {
            if (!_isLoaded)
            {
                await LoadCustomersFromFileAsync();
            }
            
            var customer = _customers.FirstOrDefault(c => c.Id == customerId);
            if (customer != null)
            {
                _customers.Remove(customer);
                System.Diagnostics.Debug.WriteLine($"Removed customer {customer.FullName} from memory (ID: {customerId})");
                
                await SaveCustomersToFileAsync();
                
                System.Diagnostics.Debug.WriteLine($"Customer {customer.FullName} deleted and file updated successfully");
            }
        }

        /// <summary>
        /// Modifies existing customer and auto-saves to file
        /// </summary>
        public async Task UpdateCustomerAsync(Customer updatedCustomer)
        {
            if (!_isLoaded)
            {
                await LoadCustomersFromFileAsync();
            }
            
            var existingCustomer = _customers.FirstOrDefault(c => c.Id == updatedCustomer.Id);
            if (existingCustomer != null)
            {
                existingCustomer.FirstName = updatedCustomer.FirstName;
                existingCustomer.LastName = updatedCustomer.LastName;
                existingCustomer.Email = updatedCustomer.Email;
                existingCustomer.Phone = updatedCustomer.Phone;
                existingCustomer.Company = updatedCustomer.Company;
                existingCustomer.LastModified = DateTime.Now;
                System.Diagnostics.Debug.WriteLine($"Updated customer {existingCustomer.FullName} in memory (ID: {existingCustomer.Id})");
                
                await SaveCustomersToFileAsync();
                
                System.Diagnostics.Debug.WriteLine($"Customer {existingCustomer.FullName} updated and saved to file successfully");
            }
        }

        /// <summary>
        /// Gets single customer by ID
        /// </summary>
        public async Task<Customer> GetCustomerByIdAsync(int customerId)
        {
            if (!_isLoaded)
            {
                await LoadCustomersFromFileAsync();
            }
            
            return _customers.FirstOrDefault(c => c.Id == customerId);
        }

        /// <summary>
        /// Manual save operation for bulk changes
        /// </summary>
        public async Task SaveChangesAsync()
        {
            System.Diagnostics.Debug.WriteLine("Manual save operation requested");
            await SaveCustomersToFileAsync();
        }

        /// <summary>
        /// Gets file information for debugging and user feedback
        /// </summary>
        public async Task<string> GetFileInfoAsync()
        {
            try
            {
                var files = await _localFolder.GetFilesAsync();
                var customersFile = files.FirstOrDefault(f => f.Name == CUSTOMERS_FILE_NAME);
                
                if (customersFile != null)
                {
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
}