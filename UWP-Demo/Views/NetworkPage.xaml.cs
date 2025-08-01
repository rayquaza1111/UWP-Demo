using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml;
using System;
using System.Threading.Tasks;
using UWP_Demo.Services;
using static UWP_Demo.Services.HttpApiService;
using System.Text;
using System.Linq;

namespace UWP_Demo.Views
{
    /// <summary>
    /// NETWORK API: Dedicated page for demonstrating HTTP/API functionality
    /// Purpose: Show JSONPlaceholder API integration and internet capabilities
    /// Features: GET/POST requests, connectivity testing, data display
    /// </summary>
    public sealed partial class NetworkPage : Page
    {
        private HttpApiService _apiService;
        private CustomerService _customerService;
        private DialogService _dialogService;

        public NetworkPage()
        {
            this.InitializeComponent();
            
            // NETWORK API: Initialize services
            _apiService = new HttpApiService();
            _customerService = new CustomerService();
            _dialogService = new DialogService();
            
            System.Diagnostics.Debug.WriteLine("NETWORK API: NetworkPage initialized with HTTP services");
        }

        #region NETWORK API: Event Handlers

        /// <summary>
        /// NETWORK API: Test connectivity to JSONPlaceholder API
        /// Shows connectivity status and response time
        /// </summary>
        private async void TestConnectionButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // NETWORK API: Disable button during test
                TestConnectionButton.IsEnabled = false;
                TestConnectionButton.Content = "Testing...";
                
                ConnectivityInfoBar.Message = "Testing connection to JSONPlaceholder API...";
                ConnectivityInfoBar.Severity = Microsoft.UI.Xaml.Controls.InfoBarSeverity.Warning;
                
                System.Diagnostics.Debug.WriteLine("NETWORK API: Starting connectivity test");
                
                // NETWORK API: Measure response time
                var startTime = DateTime.Now;
                bool isConnected = await _apiService.TestApiConnectivityAsync();
                var responseTime = (DateTime.Now - startTime).TotalMilliseconds;
                
                // NETWORK API: Update UI based on result
                if (isConnected)
                {
                    ConnectivityInfoBar.Title = "? Connected";
                    ConnectivityInfoBar.Message = $"Successfully connected to JSONPlaceholder API (Response time: {responseTime:F0}ms)";
                    ConnectivityInfoBar.Severity = Microsoft.UI.Xaml.Controls.InfoBarSeverity.Success;
                    
                    AppendToResults($"?? CONNECTIVITY TEST SUCCESS\n" +
                                   $"Response Time: {responseTime:F0}ms\n" +
                                   $"API Status: Operational\n" +
                                   $"Time: {DateTime.Now:HH:mm:ss}\n");
                }
                else
                {
                    ConnectivityInfoBar.Title = "? Connection Failed";
                    ConnectivityInfoBar.Message = "Unable to connect to JSONPlaceholder API. Check your internet connection.";
                    ConnectivityInfoBar.Severity = Microsoft.UI.Xaml.Controls.InfoBarSeverity.Error;
                    
                    AppendToResults($"? CONNECTIVITY TEST FAILED\n" +
                                   $"Unable to reach API endpoint\n" +
                                   $"Time: {DateTime.Now:HH:mm:ss}\n");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"NETWORK API ERROR: Connectivity test exception - {ex.Message}");
                
                ConnectivityInfoBar.Title = "? Error";
                ConnectivityInfoBar.Message = $"Connectivity test error: {ex.Message}";
                ConnectivityInfoBar.Severity = Microsoft.UI.Xaml.Controls.InfoBarSeverity.Error;
                
                AppendToResults($"?? CONNECTIVITY TEST ERROR\n" +
                               $"Exception: {ex.Message}\n" +
                               $"Time: {DateTime.Now:HH:mm:ss}\n");
            }
            finally
            {
                // NETWORK API: Re-enable button
                TestConnectionButton.IsEnabled = true;
                TestConnectionButton.Content = "Test Connection";
            }
        }

        /// <summary>
        /// NETWORK API: Fetch and display sample posts from JSONPlaceholder
        /// Demonstrates HTTP GET request with JSON response parsing
        /// </summary>
        private async void GetPostsButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // NETWORK API: Update UI state
                GetPostsButton.IsEnabled = false;
                GetPostsButton.Content = "Loading...";
                
                System.Diagnostics.Debug.WriteLine("NETWORK API: Starting GET posts request");
                AppendToResults($"?? FETCHING POSTS...\n" +
                               $"URL: https://jsonplaceholder.typicode.com/posts\n" +
                               $"Time: {DateTime.Now:HH:mm:ss}\n");
                
                // NETWORK API: Make GET request
                var posts = await _apiService.GetSamplePostsAsync();
                
                if (posts != null && posts.Count > 0)
                {
                    // NETWORK API: Display first 5 posts
                    var displayPosts = posts.Take(5).ToList();
                    var resultBuilder = new StringBuilder();
                    resultBuilder.AppendLine($"? POSTS RECEIVED ({posts.Count} total, showing first {displayPosts.Count}):");
                    resultBuilder.AppendLine();
                    
                    foreach (var post in displayPosts)
                    {
                        resultBuilder.AppendLine($"ID: {post.Id} | User: {post.UserId}");
                        resultBuilder.AppendLine($"Title: {post.Title}");
                        resultBuilder.AppendLine($"Body: {post.Body.Substring(0, Math.Min(100, post.Body.Length))}...");
                        resultBuilder.AppendLine("---");
                    }
                    
                    AppendToResults(resultBuilder.ToString());
                    
                    System.Diagnostics.Debug.WriteLine($"NETWORK API: Successfully retrieved {posts.Count} posts");
                }
                else
                {
                    AppendToResults("? No posts received or request failed\n");
                    System.Diagnostics.Debug.WriteLine("NETWORK API: No posts received");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"NETWORK API ERROR: GET posts failed - {ex.Message}");
                AppendToResults($"?? GET POSTS ERROR: {ex.Message}\n");
            }
            finally
            {
                // NETWORK API: Restore button state
                GetPostsButton.IsEnabled = true;
                GetPostsButton.Content = "GET Posts";
            }
        }

        /// <summary>
        /// NETWORK API: Fetch and display sample users from JSONPlaceholder
        /// Demonstrates user data retrieval that could be converted to customers
        /// </summary>
        private async void GetUsersButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // NETWORK API: Update UI state
                GetUsersButton.IsEnabled = false;
                GetUsersButton.Content = "Loading...";
                
                System.Diagnostics.Debug.WriteLine("NETWORK API: Starting GET users request");
                AppendToResults($"?? FETCHING USERS...\n" +
                               $"URL: https://jsonplaceholder.typicode.com/users\n" +
                               $"Time: {DateTime.Now:HH:mm:ss}\n");
                
                // NETWORK API: Make GET request
                var users = await _apiService.GetSampleUsersAsync();
                
                if (users != null && users.Count > 0)
                {
                    var resultBuilder = new StringBuilder();
                    resultBuilder.AppendLine($"? USERS RECEIVED ({users.Count} total):");
                    resultBuilder.AppendLine();
                    
                    foreach (var user in users)
                    {
                        resultBuilder.AppendLine($"ID: {user.Id} | Name: {user.Name}");
                        resultBuilder.AppendLine($"Email: {user.Email}");
                        resultBuilder.AppendLine($"Phone: {user.Phone}");
                        resultBuilder.AppendLine($"Company: {user.Company?.Name ?? "N/A"}");
                        resultBuilder.AppendLine("---");
                    }
                    
                    AppendToResults(resultBuilder.ToString());
                    
                    System.Diagnostics.Debug.WriteLine($"NETWORK API: Successfully retrieved {users.Count} users");
                }
                else
                {
                    AppendToResults("? No users received or request failed\n");
                    System.Diagnostics.Debug.WriteLine("NETWORK API: No users received");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"NETWORK API ERROR: GET users failed - {ex.Message}");
                AppendToResults($"?? GET USERS ERROR: {ex.Message}\n");
            }
            finally
            {
                // NETWORK API: Restore button state
                GetUsersButton.IsEnabled = true;
                GetUsersButton.Content = "GET Users";
            }
        }

        /// <summary>
        /// NETWORK API: Create a new post using POST request
        /// Demonstrates HTTP POST with JSON payload
        /// </summary>
        private async void CreatePostButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // NETWORK API: Update UI state
                CreatePostButton.IsEnabled = false;
                CreatePostButton.Content = "Posting...";
                
                System.Diagnostics.Debug.WriteLine("NETWORK API: Starting POST request to create post");
                
                // NETWORK API: Create sample post data
                string title = $"UWP Demo Post - {DateTime.Now:HH:mm:ss}";
                string body = $"This post was created from the UWP Demo app at {DateTime.Now:yyyy-MM-dd HH:mm:ss}. " +
                             "It demonstrates HTTP POST functionality with JSONPlaceholder API.";
                
                AppendToResults($"?? CREATING POST...\n" +
                               $"URL: https://jsonplaceholder.typicode.com/posts\n" +
                               $"Title: {title}\n" +
                               $"Body: {body.Substring(0, Math.Min(50, body.Length))}...\n" +
                               $"Time: {DateTime.Now:HH:mm:ss}\n");
                
                // NETWORK API: Make POST request
                var createdPost = await _apiService.CreateSamplePostAsync(title, body);
                
                if (createdPost != null)
                {
                    AppendToResults($"? POST CREATED SUCCESSFULLY:\n" +
                                   $"ID: {createdPost.Id}\n" +
                                   $"User ID: {createdPost.UserId}\n" +
                                   $"Title: {createdPost.Title}\n" +
                                   $"Body: {createdPost.Body}\n");
                    
                    System.Diagnostics.Debug.WriteLine($"NETWORK API: Successfully created post with ID: {createdPost.Id}");
                }
                else
                {
                    AppendToResults("? Failed to create post\n");
                    System.Diagnostics.Debug.WriteLine("NETWORK API: Failed to create post");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"NETWORK API ERROR: POST request failed - {ex.Message}");
                AppendToResults($"?? CREATE POST ERROR: {ex.Message}\n");
            }
            finally
            {
                // NETWORK API: Restore button state
                CreatePostButton.IsEnabled = true;
                CreatePostButton.Content = "POST Data";
            }
        }

        /// <summary>
        /// NETWORK API: Import JSONPlaceholder users as customers
        /// Demonstrates data transformation from external API to internal model
        /// </summary>
        private async void ImportUsersButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // NETWORK API: Update UI state
                ImportUsersButton.IsEnabled = false;
                ImportUsersButton.Content = "Importing...";
                
                System.Diagnostics.Debug.WriteLine("NETWORK API: Starting user import process");
                AppendToResults($"?? IMPORTING USERS AS CUSTOMERS...\n" +
                               $"Time: {DateTime.Now:HH:mm:ss}\n");
                
                // NETWORK API: Fetch users from API
                var users = await _apiService.GetSampleUsersAsync();
                
                if (users != null && users.Count > 0)
                {
                    int importedCount = 0;
                    var resultBuilder = new StringBuilder();
                    resultBuilder.AppendLine($"Processing {users.Count} users...");
                    
                    foreach (var user in users)
                    {
                        try
                        {
                            // NETWORK API: Convert API user to Customer object
                            var customer = user.ToCustomer();
                            
                            // FILE I/O: Add to local customer database
                            await _customerService.AddCustomerAsync(customer);
                            
                            importedCount++;
                            resultBuilder.AppendLine($"? Imported: {customer.FullName} ({customer.Email})");
                            
                            System.Diagnostics.Debug.WriteLine($"NETWORK API: Imported user {user.Name} as customer");
                        }
                        catch (Exception userEx)
                        {
                            resultBuilder.AppendLine($"? Failed to import: {user.Name} - {userEx.Message}");
                            System.Diagnostics.Debug.WriteLine($"NETWORK API ERROR: Failed to import user {user.Name} - {userEx.Message}");
                        }
                    }
                    
                    resultBuilder.AppendLine();
                    resultBuilder.AppendLine($"?? IMPORT COMPLETE: {importedCount}/{users.Count} users imported as customers");
                    AppendToResults(resultBuilder.ToString());
                    
                    // Show success dialog
                    await _dialogService.ShowMessageAsync(
                        "Import Complete", 
                        $"Successfully imported {importedCount} users as customers!\n\n" +
                        "You can view them in the Customer Management section.");
                    
                    System.Diagnostics.Debug.WriteLine($"NETWORK API: Import completed - {importedCount} users imported");
                }
                else
                {
                    AppendToResults("? No users available to import\n");
                    System.Diagnostics.Debug.WriteLine("NETWORK API: No users available for import");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"NETWORK API ERROR: Import process failed - {ex.Message}");
                AppendToResults($"?? IMPORT ERROR: {ex.Message}\n");
                
                await _dialogService.ShowMessageAsync("Import Error", $"Failed to import users: {ex.Message}");
            }
            finally
            {
                // NETWORK API: Restore button state
                ImportUsersButton.IsEnabled = true;
                ImportUsersButton.Content = "Import Users";
            }
        }

        /// <summary>
        /// NETWORK API: Clear the results display
        /// </summary>
        private void ClearResultsButton_Click(object sender, RoutedEventArgs e)
        {
            ResultsTextBlock.Text = "Results cleared. Ready for new API calls.";
            System.Diagnostics.Debug.WriteLine("NETWORK API: Results display cleared");
        }

        /// <summary>
        /// NETWORK API PROOF: Show HTTP logs for mentor verification
        /// Displays actual HTTP request/response data from log file
        /// </summary>
        private async void ViewHttpLogsButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("NETWORK API PROOF: Displaying HTTP logs for mentor verification");
                
                var logs = await _apiService.GetRecentHttpLogsAsync();
                
                // Display logs in results area
                ResultsTextBlock.Text = $"?? HTTP LOGS FOR MENTOR VERIFICATION\n" +
                                       $"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}\n\n" +
                                       $"{logs}";
                
                System.Diagnostics.Debug.WriteLine("NETWORK API PROOF: HTTP logs displayed");
            }
            catch (Exception ex)
            {
                AppendToResults($"? Error loading HTTP logs: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"NETWORK API PROOF ERROR: {ex.Message}");
            }
        }

        /// <summary>
        /// NETWORK API PROOF: Show log file path for mentor verification
        /// Provides exact file location for technical review
        /// </summary>
        private async void ShowLogPathButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var logPath = await _apiService.GetHttpLogFilePathAsync();
                
                await _dialogService.ShowMessageAsync(
                    "HTTP Log File Location", 
                    $"For mentor verification, the HTTP request/response log file is located at:\n\n" +
                    $"{logPath}\n\n" +
                    "This file contains detailed technical proof of all HTTP API communications including:\n" +
                    "• Request URLs and methods\n" +
                    "• Request/response headers\n" +
                    "• JSON payloads\n" +
                    "• HTTP status codes\n" +
                    "• Response times\n" +
                    "• Timestamps");
                
                System.Diagnostics.Debug.WriteLine($"NETWORK API PROOF: Log file path = {logPath}");
            }
            catch (Exception ex)
            {
                await _dialogService.ShowMessageAsync("Error", $"Failed to get log path: {ex.Message}");
            }
        }

        /// <summary>
        /// NETWORK API PROOF: Clear HTTP logs for fresh testing
        /// Allows mentor to start with clean logs for demonstration
        /// </summary>
        private async void ClearLogsButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await _apiService.ClearHttpLogsAsync();
                
                AppendToResults("??? HTTP LOGS CLEARED\n" +
                               "Ready for fresh testing and mentor verification\n");
                
                await _dialogService.ShowMessageAsync(
                    "Logs Cleared", 
                    "HTTP logs have been cleared.\n\n" +
                    "You can now perform fresh API calls and the mentor can review clean log entries.");
                
                System.Diagnostics.Debug.WriteLine("NETWORK API PROOF: HTTP logs cleared for fresh testing");
            }
            catch (Exception ex)
            {
                AppendToResults($"? Error clearing logs: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"NETWORK API PROOF ERROR: {ex.Message}");
            }
        }

        #endregion

        #region NETWORK API: Helper Methods

        /// <summary>
        /// NETWORK API: Append text to results display with timestamp
        /// </summary>
        private void AppendToResults(string text)
        {
            try
            {
                if (string.IsNullOrEmpty(ResultsTextBlock.Text) || 
                    ResultsTextBlock.Text == "No API calls made yet. Click a button above to test the network functionality." ||
                    ResultsTextBlock.Text == "Results cleared. Ready for new API calls.")
                {
                    ResultsTextBlock.Text = text + "\n";
                }
                else
                {
                    ResultsTextBlock.Text += "\n" + text + "\n";
                }
                
                // Auto-scroll to bottom (simulate)
                // In a real app, you'd use a ScrollViewer.ScrollToVerticalOffset
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"NETWORK API ERROR: Failed to append results - {ex.Message}");
            }
        }

        #endregion

        #region NETWORK API: Cleanup

        /// <summary>
        /// NETWORK API: Clean up resources when page is unloaded
        /// </summary>
        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            try
            {
                _apiService?.Dispose();
                System.Diagnostics.Debug.WriteLine("NETWORK API: NetworkPage resources cleaned up");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"NETWORK API ERROR: Cleanup failed - {ex.Message}");
            }
        }

        #endregion
    }
}