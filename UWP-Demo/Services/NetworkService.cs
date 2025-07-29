using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using UWP_Demo.Models;
using Newtonsoft.Json;
using System.Text;
using System.Diagnostics;

namespace UWP_Demo.Services
{
    /// <summary>
    /// Service class responsible for handling network operations including
    /// HTTP requests to external APIs for data synchronization and remote operations.
    /// This service demonstrates UWP networking capabilities and async patterns.
    /// </summary>
    /// <remarks>
    /// This service demonstrates several important UWP networking concepts:
    /// - HttpClient usage for REST API communication
    /// - Async/await patterns for non-blocking network operations
    /// - JSON serialization/deserialization for API data exchange
    /// - Error handling for network failures and timeouts
    /// - Integration with JSONPlaceholder test API
    /// - Proper resource disposal and memory management
    /// - Network connectivity checking and offline handling
    /// 
    /// The service uses JSONPlaceholder (https://jsonplaceholder.typicode.com) as a test API
    /// to demonstrate real-world network operations without requiring a custom backend.
    /// </remarks>
    public class NetworkService
    {
        #region Private Fields

        /// <summary>
        /// The singleton instance of the NetworkService.
        /// </summary>
        private static NetworkService _instance;

        /// <summary>
        /// Lock object for thread-safe singleton access.
        /// </summary>
        private static readonly object _lock = new object();

        /// <summary>
        /// HttpClient instance for making network requests.
        /// Reusing a single instance improves performance and connection pooling.
        /// </summary>
        private readonly HttpClient _httpClient;

        /// <summary>
        /// Base URL for the JSONPlaceholder test API.
        /// This API provides fake JSON data for testing and prototyping.
        /// </summary>
        private const string API_BASE_URL = "https://jsonplaceholder.typicode.com";

        /// <summary>
        /// Timeout for network requests in seconds.
        /// This prevents requests from hanging indefinitely.
        /// </summary>
        private const int REQUEST_TIMEOUT_SECONDS = 30;

        /// <summary>
        /// Flag indicating whether the service has been initialized.
        /// </summary>
        private bool _isInitialized;

        #endregion

        #region Events

        /// <summary>
        /// Event raised when a network operation completes (successfully or with error).
        /// This can be used for logging, analytics, or UI feedback.
        /// </summary>
        /// <remarks>
        /// Subscribe to this event to track network activity, update UI indicators,
        /// or implement custom retry logic based on operation results.
        /// </remarks>
        public event EventHandler<NetworkOperationEventArgs> NetworkOperationCompleted;

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the singleton instance of the NetworkService.
        /// Creates and initializes the instance if it doesn't exist (thread-safe).
        /// </summary>
        public static NetworkService Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new NetworkService();
                        }
                    }
                }
                return _instance;
            }
        }

        /// <summary>
        /// Gets whether the network service is available and ready for operations.
        /// This checks both initialization status and network connectivity.
        /// </summary>
        public bool IsAvailable => _isInitialized && IsConnectedToInternet();

        #endregion

        #region Constructor

        /// <summary>
        /// Private constructor for singleton pattern.
        /// Initializes the HttpClient with appropriate configuration.
        /// </summary>
        private NetworkService()
        {
            try
            {
                // Initialize HttpClient with timeout and headers
                _httpClient = new HttpClient();
                _httpClient.Timeout = TimeSpan.FromSeconds(REQUEST_TIMEOUT_SECONDS);
                
                // Set common headers for API requests
                _httpClient.DefaultRequestHeaders.Add("User-Agent", "UWP-Demo/1.0");
                _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");

                _isInitialized = true;
                Debug.WriteLine("NetworkService: Initialized successfully");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"NetworkService: Initialization error: {ex.Message}");
                _isInitialized = false;
            }
        }

        #endregion

        #region Connectivity Methods

        /// <summary>
        /// Checks if the device is connected to the internet.
        /// This is a basic connectivity check and doesn't guarantee API availability.
        /// </summary>
        /// <returns>True if connected to internet, false otherwise</returns>
        /// <remarks>
        /// This method checks the network connectivity profile to determine if
        /// the device has internet access. It's a quick check but doesn't verify
        /// that specific APIs are reachable.
        /// </remarks>
        public bool IsConnectedToInternet()
        {
            try
            {
                var profile = Windows.Networking.Connectivity.NetworkInformation.GetInternetConnectionProfile();
                return profile?.GetNetworkConnectivityLevel() == 
                       Windows.Networking.Connectivity.NetworkConnectivityLevel.InternetAccess;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"NetworkService: Error checking connectivity: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Tests connectivity to the JSONPlaceholder API by making a simple request.
        /// This verifies that the API is reachable and responding.
        /// </summary>
        /// <returns>True if API is reachable, false otherwise</returns>
        /// <example>
        /// // Check API availability before making requests
        /// if (await NetworkService.Instance.TestApiConnectivityAsync())
        /// {
        ///     var users = await NetworkService.Instance.GetUsersAsync();
        /// }
        /// </example>
        public async Task<bool> TestApiConnectivityAsync()
        {
            if (!_isInitialized)
                return false;

            try
            {
                var response = await _httpClient.GetAsync($"{API_BASE_URL}/posts/1");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"NetworkService: API connectivity test failed: {ex.Message}");
                return false;
            }
        }

        #endregion

        #region JSONPlaceholder API Methods

        /// <summary>
        /// Retrieves a list of users from the JSONPlaceholder API.
        /// This demonstrates GET requests and JSON deserialization.
        /// </summary>
        /// <returns>A list of users, or empty list if the request fails</returns>
        /// <remarks>
        /// The JSONPlaceholder API returns fake user data that includes:
        /// - ID, name, username, email
        /// - Address information (can be mapped to customer company)
        /// - Phone and website information
        /// 
        /// This method demonstrates how to fetch external data that could
        /// supplement or synchronize with local customer data.
        /// </remarks>
        /// <example>
        /// // Fetch users from external API
        /// var externalUsers = await NetworkService.Instance.GetUsersAsync();
        /// foreach (var user in externalUsers)
        /// {
        ///     // Convert to local Customer format if needed
        ///     var customer = ConvertUserToCustomer(user);
        ///     await DataService.Instance.AddCustomerAsync(customer);
        /// }
        /// </example>
        public async Task<List<JsonPlaceholderUser>> GetUsersAsync()
        {
            var operationName = "GetUsers";
            var stopwatch = Stopwatch.StartNew();

            try
            {
                if (!IsAvailable)
                {
                    throw new InvalidOperationException("Network service is not available");
                }

                Debug.WriteLine("NetworkService: Fetching users from API...");

                var response = await _httpClient.GetAsync($"{API_BASE_URL}/users");
                response.EnsureSuccessStatusCode();

                var jsonContent = await response.Content.ReadAsStringAsync();
                var users = JsonConvert.DeserializeObject<List<JsonPlaceholderUser>>(jsonContent);

                stopwatch.Stop();
                Debug.WriteLine($"NetworkService: Successfully fetched {users?.Count ?? 0} users in {stopwatch.ElapsedMilliseconds}ms");

                // Raise completion event
                NetworkOperationCompleted?.Invoke(this, new NetworkOperationEventArgs
                {
                    OperationName = operationName,
                    Success = true,
                    Duration = stopwatch.Elapsed,
                    ResultCount = users?.Count ?? 0
                });

                return users ?? new List<JsonPlaceholderUser>();
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                Debug.WriteLine($"NetworkService: Error fetching users: {ex.Message}");

                // Raise completion event with error
                NetworkOperationCompleted?.Invoke(this, new NetworkOperationEventArgs
                {
                    OperationName = operationName,
                    Success = false,
                    Duration = stopwatch.Elapsed,
                    ErrorMessage = ex.Message
                });

                return new List<JsonPlaceholderUser>();
            }
        }

        /// <summary>
        /// Retrieves a specific user by ID from the JSONPlaceholder API.
        /// This demonstrates parameterized GET requests.
        /// </summary>
        /// <param name="userId">The ID of the user to retrieve</param>
        /// <returns>The user object, or null if not found or request fails</returns>
        /// <example>
        /// // Get specific user details
        /// var user = await NetworkService.Instance.GetUserByIdAsync(1);
        /// if (user != null)
        /// {
        ///     DisplayUserDetails(user);
        /// }
        /// </example>
        public async Task<JsonPlaceholderUser> GetUserByIdAsync(int userId)
        {
            var operationName = $"GetUser_{userId}";
            var stopwatch = Stopwatch.StartNew();

            try
            {
                if (!IsAvailable)
                {
                    throw new InvalidOperationException("Network service is not available");
                }

                Debug.WriteLine($"NetworkService: Fetching user {userId} from API...");

                var response = await _httpClient.GetAsync($"{API_BASE_URL}/users/{userId}");
                response.EnsureSuccessStatusCode();

                var jsonContent = await response.Content.ReadAsStringAsync();
                var user = JsonConvert.DeserializeObject<JsonPlaceholderUser>(jsonContent);

                stopwatch.Stop();
                Debug.WriteLine($"NetworkService: Successfully fetched user {userId} in {stopwatch.ElapsedMilliseconds}ms");

                // Raise completion event
                NetworkOperationCompleted?.Invoke(this, new NetworkOperationEventArgs
                {
                    OperationName = operationName,
                    Success = true,
                    Duration = stopwatch.Elapsed,
                    ResultCount = user != null ? 1 : 0
                });

                return user;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                Debug.WriteLine($"NetworkService: Error fetching user {userId}: {ex.Message}");

                // Raise completion event with error
                NetworkOperationCompleted?.Invoke(this, new NetworkOperationEventArgs
                {
                    OperationName = operationName,
                    Success = false,
                    Duration = stopwatch.Elapsed,
                    ErrorMessage = ex.Message
                });

                return null;
            }
        }

        /// <summary>
        /// Posts data to the JSONPlaceholder API to simulate creating a new user.
        /// This demonstrates POST requests and JSON serialization.
        /// </summary>
        /// <param name="user">The user data to post</param>
        /// <returns>The created user with server-assigned ID, or null if failed</returns>
        /// <remarks>
        /// Note: JSONPlaceholder is a fake API, so this doesn't actually create
        /// persistent data. It simulates the response you would get from a real API.
        /// This is useful for testing POST operations without affecting real data.
        /// </remarks>
        /// <example>
        /// // Create a new user on the server
        /// var newUser = new JsonPlaceholderUser 
        /// { 
        ///     Name = "John Doe", 
        ///     Email = "john@example.com" 
        /// };
        /// var createdUser = await NetworkService.Instance.CreateUserAsync(newUser);
        /// </example>
        public async Task<JsonPlaceholderUser> CreateUserAsync(JsonPlaceholderUser user)
        {
            var operationName = "CreateUser";
            var stopwatch = Stopwatch.StartNew();

            try
            {
                if (!IsAvailable)
                {
                    throw new InvalidOperationException("Network service is not available");
                }

                if (user == null)
                {
                    throw new ArgumentNullException(nameof(user));
                }

                Debug.WriteLine($"NetworkService: Creating user '{user.Name}' on API...");

                // Serialize user data to JSON
                var jsonContent = JsonConvert.SerializeObject(user, Formatting.Indented);
                var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"{API_BASE_URL}/users", httpContent);
                response.EnsureSuccessStatusCode();

                var responseContent = await response.Content.ReadAsStringAsync();
                var createdUser = JsonConvert.DeserializeObject<JsonPlaceholderUser>(responseContent);

                stopwatch.Stop();
                Debug.WriteLine($"NetworkService: Successfully created user in {stopwatch.ElapsedMilliseconds}ms");

                // Raise completion event
                NetworkOperationCompleted?.Invoke(this, new NetworkOperationEventArgs
                {
                    OperationName = operationName,
                    Success = true,
                    Duration = stopwatch.Elapsed,
                    ResultCount = 1
                });

                return createdUser;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                Debug.WriteLine($"NetworkService: Error creating user: {ex.Message}");

                // Raise completion event with error
                NetworkOperationCompleted?.Invoke(this, new NetworkOperationEventArgs
                {
                    OperationName = operationName,
                    Success = false,
                    Duration = stopwatch.Elapsed,
                    ErrorMessage = ex.Message
                });

                return null;
            }
        }

        /// <summary>
        /// Updates an existing user on the JSONPlaceholder API.
        /// This demonstrates PUT requests for updating resources.
        /// </summary>
        /// <param name="userId">The ID of the user to update</param>
        /// <param name="user">The updated user data</param>
        /// <returns>The updated user object, or null if failed</returns>
        public async Task<JsonPlaceholderUser> UpdateUserAsync(int userId, JsonPlaceholderUser user)
        {
            var operationName = $"UpdateUser_{userId}";
            var stopwatch = Stopwatch.StartNew();

            try
            {
                if (!IsAvailable)
                {
                    throw new InvalidOperationException("Network service is not available");
                }

                if (user == null)
                {
                    throw new ArgumentNullException(nameof(user));
                }

                Debug.WriteLine($"NetworkService: Updating user {userId} on API...");

                // Serialize user data to JSON
                var jsonContent = JsonConvert.SerializeObject(user, Formatting.Indented);
                var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var response = await _httpClient.PutAsync($"{API_BASE_URL}/users/{userId}", httpContent);
                response.EnsureSuccessStatusCode();

                var responseContent = await response.Content.ReadAsStringAsync();
                var updatedUser = JsonConvert.DeserializeObject<JsonPlaceholderUser>(responseContent);

                stopwatch.Stop();
                Debug.WriteLine($"NetworkService: Successfully updated user {userId} in {stopwatch.ElapsedMilliseconds}ms");

                // Raise completion event
                NetworkOperationCompleted?.Invoke(this, new NetworkOperationEventArgs
                {
                    OperationName = operationName,
                    Success = true,
                    Duration = stopwatch.Elapsed,
                    ResultCount = 1
                });

                return updatedUser;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                Debug.WriteLine($"NetworkService: Error updating user {userId}: {ex.Message}");

                // Raise completion event with error
                NetworkOperationCompleted?.Invoke(this, new NetworkOperationEventArgs
                {
                    OperationName = operationName,
                    Success = false,
                    Duration = stopwatch.Elapsed,
                    ErrorMessage = ex.Message
                });

                return null;
            }
        }

        /// <summary>
        /// Deletes a user from the JSONPlaceholder API.
        /// This demonstrates DELETE requests for removing resources.
        /// </summary>
        /// <param name="userId">The ID of the user to delete</param>
        /// <returns>True if deletion was successful, false otherwise</returns>
        public async Task<bool> DeleteUserAsync(int userId)
        {
            var operationName = $"DeleteUser_{userId}";
            var stopwatch = Stopwatch.StartNew();

            try
            {
                if (!IsAvailable)
                {
                    throw new InvalidOperationException("Network service is not available");
                }

                Debug.WriteLine($"NetworkService: Deleting user {userId} from API...");

                var response = await _httpClient.DeleteAsync($"{API_BASE_URL}/users/{userId}");
                response.EnsureSuccessStatusCode();

                stopwatch.Stop();
                Debug.WriteLine($"NetworkService: Successfully deleted user {userId} in {stopwatch.ElapsedMilliseconds}ms");

                // Raise completion event
                NetworkOperationCompleted?.Invoke(this, new NetworkOperationEventArgs
                {
                    OperationName = operationName,
                    Success = true,
                    Duration = stopwatch.Elapsed,
                    ResultCount = 1
                });

                return true;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                Debug.WriteLine($"NetworkService: Error deleting user {userId}: {ex.Message}");

                // Raise completion event with error
                NetworkOperationCompleted?.Invoke(this, new NetworkOperationEventArgs
                {
                    OperationName = operationName,
                    Success = false,
                    Duration = stopwatch.Elapsed,
                    ErrorMessage = ex.Message
                });

                return false;
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Converts a JSONPlaceholder user to a local Customer object.
        /// This demonstrates data transformation between external and internal formats.
        /// </summary>
        /// <param name="user">The JSONPlaceholder user to convert</param>
        /// <returns>A Customer object with mapped data</returns>
        /// <example>
        /// // Convert external user data to local format
        /// var externalUsers = await NetworkService.Instance.GetUsersAsync();
        /// var localCustomers = externalUsers.Select(u => 
        ///     NetworkService.Instance.ConvertToCustomer(u)).ToList();
        /// </example>
        public Customer ConvertToCustomer(JsonPlaceholderUser user)
        {
            if (user == null)
                return null;

            // Split name into first and last name (basic approach)
            var nameParts = user.Name?.Split(' ') ?? new[] { "", "" };
            var firstName = nameParts.Length > 0 ? nameParts[0] : "";
            var lastName = nameParts.Length > 1 ? string.Join(" ", nameParts.Skip(1)) : "";

            return new Customer
            {
                Id = user.Id,
                FirstName = firstName,
                LastName = lastName,
                Email = user.Email ?? "",
                Phone = user.Phone ?? "",
                Company = user.Company?.Name ?? "",
                DateCreated = DateTime.Now,
                LastModified = DateTime.Now
            };
        }

        /// <summary>
        /// Converts a local Customer to a JSONPlaceholder user format.
        /// This demonstrates data transformation for external API submission.
        /// </summary>
        /// <param name="customer">The customer to convert</param>
        /// <returns>A JSONPlaceholder user object</returns>
        public JsonPlaceholderUser ConvertFromCustomer(Customer customer)
        {
            if (customer == null)
                return null;

            return new JsonPlaceholderUser
            {
                Id = customer.Id,
                Name = customer.FullName,
                Email = customer.Email,
                Phone = customer.Phone,
                Company = !string.IsNullOrEmpty(customer.Company) 
                    ? new JsonPlaceholderCompany { Name = customer.Company }
                    : null
            };
        }

        #endregion

        #region Resource Management

        /// <summary>
        /// Disposes of network resources when the service is no longer needed.
        /// This should be called during application shutdown.
        /// </summary>
        /// <remarks>
        /// Proper disposal of HttpClient resources prevents memory leaks and
        /// ensures clean shutdown of network connections.
        /// </remarks>
        public void Dispose()
        {
            try
            {
                _httpClient?.Dispose();
                _isInitialized = false;
                Debug.WriteLine("NetworkService: Disposed successfully");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"NetworkService: Error during disposal: {ex.Message}");
            }
        }

        #endregion

        #region Data Models for JSONPlaceholder API

        /// <summary>
        /// Represents a user object from the JSONPlaceholder API.
        /// This matches the structure of the external API response.
        /// </summary>
        public class JsonPlaceholderUser
        {
            [JsonProperty("id")]
            public int Id { get; set; }

            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("username")]
            public string Username { get; set; }

            [JsonProperty("email")]
            public string Email { get; set; }

            [JsonProperty("phone")]
            public string Phone { get; set; }

            [JsonProperty("website")]
            public string Website { get; set; }

            [JsonProperty("company")]
            public JsonPlaceholderCompany Company { get; set; }

            [JsonProperty("address")]
            public JsonPlaceholderAddress Address { get; set; }
        }

        /// <summary>
        /// Represents a company object from the JSONPlaceholder API.
        /// </summary>
        public class JsonPlaceholderCompany
        {
            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("catchPhrase")]
            public string CatchPhrase { get; set; }

            [JsonProperty("bs")]
            public string BusinessSummary { get; set; }
        }

        /// <summary>
        /// Represents an address object from the JSONPlaceholder API.
        /// </summary>
        public class JsonPlaceholderAddress
        {
            [JsonProperty("street")]
            public string Street { get; set; }

            [JsonProperty("suite")]
            public string Suite { get; set; }

            [JsonProperty("city")]
            public string City { get; set; }

            [JsonProperty("zipcode")]
            public string ZipCode { get; set; }
        }

        #endregion
    }

    /// <summary>
    /// Event arguments for network operation completion events.
    /// Provides details about the operation result for logging and monitoring.
    /// </summary>
    public class NetworkOperationEventArgs : EventArgs
    {
        /// <summary>
        /// The name of the network operation that completed.
        /// </summary>
        public string OperationName { get; set; }

        /// <summary>
        /// Whether the operation completed successfully.
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// The duration of the operation.
        /// </summary>
        public TimeSpan Duration { get; set; }

        /// <summary>
        /// The number of items returned by the operation (for successful operations).
        /// </summary>
        public int ResultCount { get; set; }

        /// <summary>
        /// Error message for failed operations.
        /// </summary>
        public string ErrorMessage { get; set; }
    }
}