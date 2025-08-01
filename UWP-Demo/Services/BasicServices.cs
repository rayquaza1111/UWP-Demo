using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UWP_Demo.Models;
using System.Collections.ObjectModel;
// FILE I/O: Required UWP namespaces for file operations
using Windows.Storage;     // FILE I/O: Provides StorageFile, StorageFolder classes for file system access
using Newtonsoft.Json;     // FILE I/O: JSON serialization library for converting objects to/from JSON
// NETWORK API: Required namespaces for HTTP operations
using System.Net.Http;     // NETWORK API: Provides HttpClient for HTTP requests
using System.Text;         // NETWORK API: For encoding HTTP request content

namespace UWP_Demo.Services
{
    /// <summary>
    /// NETWORK API: HTTP service for external API communication
    /// Demonstrates integration with public APIs like JSONPlaceholder
    /// Shows how to make GET, POST requests and handle responses
    /// </summary>
    public class HttpApiService
    {
        private readonly HttpClient _httpClient;
        private readonly HttpLoggingService _logger;
        private const string JSON_PLACEHOLDER_BASE_URL = "https://jsonplaceholder.typicode.com";

        public HttpApiService()
        {
            // NETWORK API: Initialize HttpClient for API requests
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "UWP-Demo-App/1.0");
            
            // NETWORK API PROOF: Initialize logging service for mentor verification
            _logger = new HttpLoggingService();
            
            System.Diagnostics.Debug.WriteLine("NETWORK API: HttpApiService initialized with JSONPlaceholder base URL and logging");
        }

        #region NETWORK API: JSONPlaceholder Demo Endpoints

        /// <summary>
        /// NETWORK API: Enhanced GET request with detailed logging for mentor verification
        /// Demonstrates basic HTTP GET operation with comprehensive debugging output
        /// URL: https://jsonplaceholder.typicode.com/posts
        /// </summary>
        public async Task<List<JsonPlaceholderPost>> GetSamplePostsAsync()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("===========================================");
                System.Diagnostics.Debug.WriteLine("NETWORK API PROOF: Starting GET request to fetch sample posts");
                System.Diagnostics.Debug.WriteLine($"TIMESTAMP: {DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}");
                
                string url = $"{JSON_PLACEHOLDER_BASE_URL}/posts";
                System.Diagnostics.Debug.WriteLine($"NETWORK API PROOF: REQUEST URL = {url}");
                System.Diagnostics.Debug.WriteLine($"NETWORK API PROOF: HTTP METHOD = GET");
                System.Diagnostics.Debug.WriteLine($"NETWORK API PROOF: User-Agent = {_httpClient.DefaultRequestHeaders.UserAgent}");
                
                // NETWORK API: Make HTTP GET request with timing
                var stopwatch = System.Diagnostics.Stopwatch.StartNew();
                HttpResponseMessage response = await _httpClient.GetAsync(url);
                stopwatch.Stop();
                
                // NETWORK API: Read response content as string
                string jsonContent = await response.Content.ReadAsStringAsync();
                
                // NETWORK API PROOF: Log to file for mentor verification
                await _logger.LogHttpRequestAsync(
                    method: "GET",
                    url: url,
                    statusCode: response.StatusCode,
                    responseBody: jsonContent,
                    responseTimeMs: stopwatch.ElapsedMilliseconds
                );
                
                // NETWORK API PROOF: Log detailed response information
                System.Diagnostics.Debug.WriteLine($"NETWORK API PROOF: RESPONSE STATUS = {response.StatusCode} ({(int)response.StatusCode})");
                System.Diagnostics.Debug.WriteLine($"NETWORK API PROOF: RESPONSE TIME = {stopwatch.ElapsedMilliseconds}ms");
                System.Diagnostics.Debug.WriteLine($"NETWORK API PROOF: CONTENT TYPE = {response.Content.Headers.ContentType}");
                System.Diagnostics.Debug.WriteLine($"NETWORK API PROOF: CONTENT LENGTH = {response.Content.Headers.ContentLength} bytes");
                System.Diagnostics.Debug.WriteLine($"NETWORK API PROOF: RECEIVED CONTENT LENGTH = {jsonContent.Length} characters");
                System.Diagnostics.Debug.WriteLine($"NETWORK API PROOF: CONTENT PREVIEW = {jsonContent.Substring(0, Math.Min(200, jsonContent.Length))}...");
                
                response.EnsureSuccessStatusCode(); // NETWORK API: Throw if not success status
                
                // NETWORK API: Parse JSON response to objects
                var posts = JsonConvert.DeserializeObject<List<JsonPlaceholderPost>>(jsonContent);
                System.Diagnostics.Debug.WriteLine($"NETWORK API PROOF: PARSED OBJECTS COUNT = {posts?.Count ?? 0}");
                
                if (posts != null && posts.Count > 0)
                {
                    System.Diagnostics.Debug.WriteLine($"NETWORK API PROOF: FIRST POST ID = {posts[0].Id}");
                    System.Diagnostics.Debug.WriteLine($"NETWORK API PROOF: FIRST POST TITLE = {posts[0].Title}");
                    System.Diagnostics.Debug.WriteLine($"NETWORK API PROOF: LAST POST ID = {posts[posts.Count - 1].Id}");
                }
                
                System.Diagnostics.Debug.WriteLine("NETWORK API PROOF: GET REQUEST COMPLETED SUCCESSFULLY");
                System.Diagnostics.Debug.WriteLine("===========================================");
                
                return posts ?? new List<JsonPlaceholderPost>();
            }
            catch (HttpRequestException ex)
            {
                System.Diagnostics.Debug.WriteLine($"NETWORK API PROOF ERROR: HTTP request failed - {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"NETWORK API PROOF ERROR: Stack trace = {ex.StackTrace}");
                
                // NETWORK API PROOF: Log error to file
                await _logger.LogHttpRequestAsync("GET", $"{JSON_PLACEHOLDER_BASE_URL}/posts", 
                    responseBody: $"ERROR: {ex.Message}");
                
                return new List<JsonPlaceholderPost>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"NETWORK API PROOF ERROR: General error - {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"NETWORK API PROOF ERROR: Stack trace = {ex.StackTrace}");
                return new List<JsonPlaceholderPost>();
            }
        }

        /// <summary>
        /// NETWORK API: GET request - Fetch sample users from JSONPlaceholder
        /// Demonstrates fetching user data that could be integrated with your customer system
        /// URL: https://jsonplaceholder.typicode.com/users
        /// </summary>
        public async Task<List<JsonPlaceholderUser>> GetSampleUsersAsync()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("NETWORK API: Starting GET request to fetch sample users");
                
                string url = $"{JSON_PLACEHOLDER_BASE_URL}/users";
                System.Diagnostics.Debug.WriteLine($"NETWORK API: GET {url}");
                
                // NETWORK API: Make HTTP GET request
                HttpResponseMessage response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();
                
                // NETWORK API: Read and parse JSON response
                string jsonContent = await response.Content.ReadAsStringAsync();
                var users = JsonConvert.DeserializeObject<List<JsonPlaceholderUser>>(jsonContent);
                System.Diagnostics.Debug.WriteLine($"NETWORK API: Successfully fetched {users?.Count ?? 0} users");
                
                return users ?? new List<JsonPlaceholderUser>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"NETWORK API ERROR: Failed to fetch users - {ex.Message}");
                return new List<JsonPlaceholderUser>();
            }
        }

        /// <summary>
        /// NETWORK API: Enhanced POST request with detailed logging for mentor verification
        /// Demonstrates HTTP POST with JSON payload and comprehensive debugging output
        /// URL: https://jsonplaceholder.typicode.com/posts
        /// </summary>
        public async Task<JsonPlaceholderPost> CreateSamplePostAsync(string title, string body, int userId = 1)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("===========================================");
                System.Diagnostics.Debug.WriteLine($"NETWORK API PROOF: Starting POST request to create post");
                System.Diagnostics.Debug.WriteLine($"TIMESTAMP: {DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}");
                
                // NETWORK API: Create request payload
                var newPost = new
                {
                    title = title,
                    body = body,
                    userId = userId
                };
                
                string jsonPayload = JsonConvert.SerializeObject(newPost, Formatting.Indented);
                var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
                
                string url = $"{JSON_PLACEHOLDER_BASE_URL}/posts";
                System.Diagnostics.Debug.WriteLine($"NETWORK API PROOF: REQUEST URL = {url}");
                System.Diagnostics.Debug.WriteLine($"NETWORK API PROOF: HTTP METHOD = POST");
                System.Diagnostics.Debug.WriteLine($"NETWORK API PROOF: CONTENT TYPE = application/json");
                System.Diagnostics.Debug.WriteLine($"NETWORK API PROOF: PAYLOAD SIZE = {jsonPayload.Length} characters");
                System.Diagnostics.Debug.WriteLine($"NETWORK API PROOF: REQUEST PAYLOAD = {jsonPayload}");
                
                // NETWORK API: Make HTTP POST request with timing
                var stopwatch = System.Diagnostics.Stopwatch.StartNew();
                HttpResponseMessage response = await _httpClient.PostAsync(url, content);
                stopwatch.Stop();
                
                // NETWORK API: Parse response
                string responseJson = await response.Content.ReadAsStringAsync();
                
                // NETWORK API PROOF: Log to file for mentor verification
                await _logger.LogHttpRequestAsync(
                    method: "POST",
                    url: url,
                    requestBody: jsonPayload,
                    statusCode: response.StatusCode,
                    responseBody: responseJson,
                    responseTimeMs: stopwatch.ElapsedMilliseconds
                );
                
                // NETWORK API PROOF: Log detailed response information
                System.Diagnostics.Debug.WriteLine($"NETWORK API PROOF: RESPONSE STATUS = {response.StatusCode} ({(int)response.StatusCode})");
                System.Diagnostics.Debug.WriteLine($"NETWORK API PROOF: RESPONSE TIME = {stopwatch.ElapsedMilliseconds}ms");
                System.Diagnostics.Debug.WriteLine($"NETWORK API PROOF: RESPONSE CONTENT TYPE = {response.Content.Headers.ContentType}");
                System.Diagnostics.Debug.WriteLine($"NETWORK API PROOF: RESPONSE CONTENT = {responseJson}");
                
                response.EnsureSuccessStatusCode();
                
                var createdPost = JsonConvert.DeserializeObject<JsonPlaceholderPost>(responseJson);
                System.Diagnostics.Debug.WriteLine($"NETWORK API PROOF: CREATED POST ID = {createdPost?.Id}");
                System.Diagnostics.Debug.WriteLine($"NETWORK API PROOF: SERVER ASSIGNED ID = {createdPost?.Id} (Proves server processing)");
                System.Diagnostics.Debug.WriteLine("NETWORK API PROOF: POST REQUEST COMPLETED SUCCESSFULLY");
                System.Diagnostics.Debug.WriteLine("===========================================");
                
                return createdPost;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"NETWORK API PROOF ERROR: Failed to create post - {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"NETWORK API PROOF ERROR: Stack trace = {ex.StackTrace}");
                
                // NETWORK API PROOF: Log error to file
                await _logger.LogHttpRequestAsync("POST", $"{JSON_PLACEHOLDER_BASE_URL}/posts", 
                    responseBody: $"ERROR: {ex.Message}");
                
                return null;
            }
        }

        /// <summary>
        /// NETWORK API: GET request with specific ID - Fetch single post
        /// Demonstrates parameterized API calls
        /// URL: https://jsonplaceholder.typicode.com/posts/{id}
        /// </summary>
        public async Task<JsonPlaceholderPost> GetPostByIdAsync(int postId)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"NETWORK API: Starting GET request for post ID: {postId}");
                
                string url = $"{JSON_PLACEHOLDER_BASE_URL}/posts/{postId}";
                System.Diagnostics.Debug.WriteLine($"NETWORK API: GET {url}");
                
                HttpResponseMessage response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();
                
                string jsonContent = await response.Content.ReadAsStringAsync();
                var post = JsonConvert.DeserializeObject<JsonPlaceholderPost>(jsonContent);
                System.Diagnostics.Debug.WriteLine($"NETWORK API: Successfully fetched post: {post?.Title}");
                
                return post;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"NETWORK API ERROR: Failed to fetch post {postId} - {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// NETWORK API: Test connectivity to JSONPlaceholder API
        /// Simple health check to verify internet connection and API availability
        /// </summary>
        public async Task<bool> TestApiConnectivityAsync()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("NETWORK API: Testing connectivity to JSONPlaceholder API");
                
                string url = $"{JSON_PLACEHOLDER_BASE_URL}/posts/1";
                HttpResponseMessage response = await _httpClient.GetAsync(url);
                
                bool isConnected = response.IsSuccessStatusCode;
                System.Diagnostics.Debug.WriteLine($"NETWORK API: Connectivity test result: {isConnected}");
                
                return isConnected;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"NETWORK API: Connectivity test failed - {ex.Message}");
                return false;
            }
        }

        #endregion

        #region NETWORK API: Model Classes for JSONPlaceholder

        /// <summary>
        /// NETWORK API: Model for JSONPlaceholder post data
        /// Represents a blog post from the demo API
        /// </summary>
        public class JsonPlaceholderPost
        {
            [JsonProperty("id")]
            public int Id { get; set; }

            [JsonProperty("userId")]
            public int UserId { get; set; }

            [JsonProperty("title")]
            public string Title { get; set; } = "";

            [JsonProperty("body")]
            public string Body { get; set; } = "";

            public override string ToString()
            {
                return $"Post {Id}: {Title}";
            }
        }

        /// <summary>
        /// NETWORK API: Model for JSONPlaceholder user data
        /// Represents a user from the demo API (could be converted to Customer)
        /// </summary>
        public class JsonPlaceholderUser
        {
            [JsonProperty("id")]
            public int Id { get; set; }

            [JsonProperty("name")]
            public string Name { get; set; } = "";

            [JsonProperty("username")]
            public string Username { get; set; } = "";

            [JsonProperty("email")]
            public string Email { get; set; } = "";

            [JsonProperty("phone")]
            public string Phone { get; set; } = "";

            [JsonProperty("website")]
            public string Website { get; set; } = "";

            [JsonProperty("company")]
            public JsonPlaceholderCompany Company { get; set; }

            public override string ToString()
            {
                return $"User {Id}: {Name} ({Email})";
            }

            /// <summary>
            /// NETWORK API: Convert JSONPlaceholder user to local Customer object
            /// Demonstrates data transformation from external API to internal model
            /// </summary>
            public Customer ToCustomer()
            {
                var nameParts = Name.Split(' ');
                return new Customer
                {
                    FirstName = nameParts.Length > 0 ? nameParts[0] : Name,
                    LastName = nameParts.Length > 1 ? string.Join(" ", nameParts.Skip(1)) : "",
                    Email = Email,
                    Phone = Phone,
                    Company = Company?.Name ?? "",
                    DateCreated = DateTime.Now,
                    LastModified = DateTime.Now
                };
            }
        }

        /// <summary>
        /// NETWORK API: Model for JSONPlaceholder company data
        /// Nested object within user data
        /// </summary>
        public class JsonPlaceholderCompany
        {
            [JsonProperty("name")]
            public string Name { get; set; } = "";

            [JsonProperty("catchPhrase")]
            public string CatchPhrase { get; set; } = "";

            [JsonProperty("bs")]
            public string Bs { get; set; } = "";
        }

        #endregion

        #region NETWORK API: Cleanup

        /// <summary>
        /// NETWORK API: Dispose HttpClient resources
        /// Important for proper resource management
        /// </summary>
        public void Dispose()
        {
            _httpClient?.Dispose();
            System.Diagnostics.Debug.WriteLine("NETWORK API: HttpApiService disposed");
        }

        #endregion

        #region NETWORK API PROOF: Access to HTTP Logs

        /// <summary>
        /// NETWORK API PROOF: Get HTTP log file path for mentor verification
        /// Returns the location of the detailed HTTP request/response log
        /// </summary>
        public async Task<string> GetHttpLogFilePathAsync()
        {
            return await _logger.GetLogFilePathAsync();
        }

        /// <summary>
        /// NETWORK API PROOF: Get recent HTTP log entries for display
        /// Returns formatted log entries showing actual HTTP communication
        /// </summary>
        public async Task<string> GetRecentHttpLogsAsync()
        {
            return await _logger.GetRecentLogEntriesAsync();
        }

        /// <summary>
        /// NETWORK API PROOF: Clear HTTP logs for fresh testing
        /// Allows mentor to see clean test results
        /// </summary>
        public async Task ClearHttpLogsAsync()
        {
            await _logger.ClearLogAsync();
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
    };

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
    };

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
    /// NETWORK API: HTTP Logging Service for mentor verification
    /// Logs all HTTP requests and responses to a file for technical proof
    /// Creates detailed audit trail of network operations
    /// </summary>
    public class HttpLoggingService
    {
        private readonly StorageFolder _localFolder;
        private const string LOG_FILE_NAME = "http_api_log.txt";

        public HttpLoggingService()
        {
            _localFolder = ApplicationData.Current.LocalFolder;
        }

        /// <summary>
        /// NETWORK API PROOF: Log HTTP request details to file for mentor verification
        /// Creates permanent record of API communication
        /// </summary>
        public async Task LogHttpRequestAsync(string method, string url, string requestBody = null, 
            System.Net.HttpStatusCode? statusCode = null, string responseBody = null, long responseTimeMs = 0)
        {
            try
            {
                var logEntry = new StringBuilder();
                logEntry.AppendLine("================================");
                logEntry.AppendLine($"HTTP API LOG ENTRY");
                logEntry.AppendLine($"Timestamp: {DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}");
                logEntry.AppendLine($"Method: {method}");
                logEntry.AppendLine($"URL: {url}");
                logEntry.AppendLine($"User-Agent: UWP-Demo-App/1.0");
                
                if (!string.IsNullOrEmpty(requestBody))
                {
                    logEntry.AppendLine($"Request Body: {requestBody}");
                }
                
                if (statusCode.HasValue)
                {
                    logEntry.AppendLine($"Response Status: {statusCode} ({(int)statusCode})");
                    logEntry.AppendLine($"Response Time: {responseTimeMs}ms");
                }
                
                if (!string.IsNullOrEmpty(responseBody))
                {
                    // Log first 500 characters of response
                    var preview = responseBody.Length > 500 ? responseBody.Substring(0, 500) + "..." : responseBody;
                    logEntry.AppendLine($"Response Preview: {preview}");
                    logEntry.AppendLine($"Response Length: {responseBody.Length} characters");
                }
                
                logEntry.AppendLine("================================");
                logEntry.AppendLine();

                // Write to log file
                await AppendToLogFileAsync(logEntry.ToString());
                
                System.Diagnostics.Debug.WriteLine($"HTTP LOGGING: Entry written to {LOG_FILE_NAME}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"HTTP LOGGING ERROR: {ex.Message}");
            }
        }

        /// <summary>
        /// NETWORK API PROOF: Append log entry to persistent file
        /// </summary>
        private async Task AppendToLogFileAsync(string logEntry)
        {
            try
            {
                var logFile = await _localFolder.CreateFileAsync(LOG_FILE_NAME, CreationCollisionOption.OpenIfExists);
                await FileIO.AppendTextAsync(logFile, logEntry);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"HTTP LOGGING FILE ERROR: {ex.Message}");
            }
        }

        /// <summary>
        /// NETWORK API PROOF: Get log file path for mentor verification
        /// Returns the full path to the HTTP log file
        /// </summary>
        public async Task<string> GetLogFilePathAsync()
        {
            try
            {
                var files = await _localFolder.GetFilesAsync();
                var logFile = files.FirstOrDefault(f => f.Name == LOG_FILE_NAME);
                return logFile?.Path ?? "Log file not found";
            }
            catch (Exception ex)
            {
                return $"Error getting log path: {ex.Message}";
            }
        }

        /// <summary>
        /// NETWORK API PROOF: Get recent log entries for display
        /// </summary>
        public async Task<string> GetRecentLogEntriesAsync(int maxEntries = 5)
        {
            try
            {
                var files = await _localFolder.GetFilesAsync();
                var logFile = files.FirstOrDefault(f => f.Name == LOG_FILE_NAME);
                
                if (logFile != null)
                {
                    var content = await FileIO.ReadTextAsync(logFile);
                    var entries = content.Split(new[] { "================================" }, StringSplitOptions.RemoveEmptyEntries);
                    
                    var recentEntries = entries.TakeLast(maxEntries).ToList();
                    return string.Join("\n================================\n", recentEntries);
                }
                
                return "No log entries found";
            }
            catch (Exception ex)
            {
                return $"Error reading log: {ex.Message}";
            }
        }

        /// <summary>
        /// NETWORK API PROOF: Clear log file for fresh testing
        /// </summary>
        public async Task ClearLogAsync()
        {
            try
            {
                var files = await _localFolder.GetFilesAsync();
                var logFile = files.FirstOrDefault(f => f.Name == LOG_FILE_NAME);
                
                if (logFile != null)
                {
                    await logFile.DeleteAsync();
                    System.Diagnostics.Debug.WriteLine("HTTP LOGGING: Log file cleared");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"HTTP LOGGING CLEAR ERROR: {ex.Message}");
            }
        }
    }
}