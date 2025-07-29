using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using UWP_Demo.Models;
using UWP_Demo.Services;
using UWP_Demo.Helpers;

namespace UWP_Demo.ViewModels
{
    /// <summary>
    /// ViewModel for the Home page that manages customer list display and basic operations.
    /// This ViewModel demonstrates MVVM patterns for data binding, command handling, and
    /// integration with application services for customer management.
    /// </summary>
    /// <remarks>
    /// This ViewModel showcases several important MVVM and UWP concepts:
    /// - ObservableCollection for dynamic list binding
    /// - ICommand implementation for user actions
    /// - Async operations with proper UI feedback
    /// - Service integration for data and network operations
    /// - Search and filtering functionality
    /// - Selection management and state preservation
    /// - Error handling with user-friendly messages
    /// - Loading states and progress indication
    /// 
    /// The HomeViewModel serves as the data context for the Home page and coordinates
    /// all customer list operations including viewing, searching, selecting, and
    /// initiating edit or delete operations.
    /// </remarks>
    public class HomeViewModel : BaseViewModel
    {
        #region Private Fields

        /// <summary>
        /// Reference to the data service for customer operations.
        /// </summary>
        private readonly DataService _dataService;

        /// <summary>
        /// Reference to the network service for API operations.
        /// </summary>
        private readonly NetworkService _networkService;

        /// <summary>
        /// Reference to the settings service for state persistence.
        /// </summary>
        private readonly SettingsService _settingsService;

        /// <summary>
        /// The currently selected customer in the list.
        /// </summary>
        private Customer _selectedCustomer;

        /// <summary>
        /// The current search/filter text.
        /// </summary>
        private string _searchText;

        /// <summary>
        /// Filtered collection of customers based on search criteria.
        /// </summary>
        private ObservableCollection<Customer> _filteredCustomers;

        /// <summary>
        /// Flag indicating whether a refresh operation is in progress.
        /// </summary>
        private bool _isRefreshing;

        /// <summary>
        /// Flag indicating whether external data is being loaded.
        /// </summary>
        private bool _isLoadingExternalData;

        /// <summary>
        /// Summary text showing customer statistics.
        /// </summary>
        private string _customerSummary;

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the collection of customers from the data service.
        /// This is bound directly to the data service's observable collection.
        /// </summary>
        /// <remarks>
        /// Binding directly to the service's collection ensures that any changes
        /// made through other parts of the application (like the Edit page) are
        /// automatically reflected in the Home page UI.
        /// </remarks>
        public ObservableCollection<Customer> Customers => _dataService.Customers;

        /// <summary>
        /// Gets or sets the filtered collection of customers based on search criteria.
        /// This collection is displayed in the UI when search is active.
        /// </summary>
        /// <remarks>
        /// When no search is active, this collection mirrors the main Customers collection.
        /// When search is active, it contains only customers matching the search criteria.
        /// </remarks>
        public ObservableCollection<Customer> FilteredCustomers
        {
            get => _filteredCustomers;
            set => SetProperty(ref _filteredCustomers, value);
        }

        /// <summary>
        /// Gets or sets the currently selected customer.
        /// This property is bound to the ListView.SelectedItem property.
        /// </summary>
        /// <remarks>
        /// When a customer is selected, the selection is persisted to settings
        /// so it can be restored when navigating back to this page or when
        /// the app is resumed from suspension.
        /// </remarks>
        public Customer SelectedCustomer
        {
            get => _selectedCustomer;
            set
            {
                if (SetProperty(ref _selectedCustomer, value))
                {
                    // Update command availability based on selection
                    OnSelectionChanged();
                    
                    // Persist selection for state restoration
                    PersistSelection();
                }
            }
        }

        /// <summary>
        /// Gets or sets the search text for filtering customers.
        /// Changes to this property trigger automatic filtering of the customer list.
        /// </summary>
        /// <remarks>
        /// The search is performed across customer name, email, and company fields.
        /// The search is case-insensitive and supports partial matches.
        /// </remarks>
        public string SearchText
        {
            get => _searchText;
            set
            {
                if (SetProperty(ref _searchText, value))
                {
                    // Automatically filter when search text changes
                    FilterCustomers();
                }
            }
        }

        /// <summary>
        /// Gets or sets whether a refresh operation is currently in progress.
        /// This is used to show progress indicators and disable refresh actions.
        /// </summary>
        public bool IsRefreshing
        {
            get => _isRefreshing;
            set => SetProperty(ref _isRefreshing, value);
        }

        /// <summary>
        /// Gets or sets whether external data loading is in progress.
        /// This is used to show progress indicators for network operations.
        /// </summary>
        public bool IsLoadingExternalData
        {
            get => _isLoadingExternalData;
            set => SetProperty(ref _isLoadingExternalData, value);
        }

        /// <summary>
        /// Gets or sets a summary text showing customer statistics.
        /// This provides quick insights about the customer database.
        /// </summary>
        /// <remarks>
        /// The summary includes information like total customer count,
        /// number of customers with companies, recent additions, etc.
        /// </remarks>
        public string CustomerSummary
        {
            get => _customerSummary;
            set => SetProperty(ref _customerSummary, value);
        }

        /// <summary>
        /// Gets whether there are any customers in the database.
        /// This is used to show/hide empty state UI elements.
        /// </summary>
        public bool HasCustomers => Customers?.Count > 0;

        /// <summary>
        /// Gets whether search is currently active.
        /// This is used to show/hide search-related UI elements.
        /// </summary>
        public bool IsSearchActive => !string.IsNullOrWhiteSpace(SearchText);

        /// <summary>
        /// Gets the count of customers in the filtered list.
        /// This is displayed in the UI to show how many customers match the current filter.
        /// </summary>
        public int FilteredCustomerCount => FilteredCustomers?.Count ?? 0;

        #endregion

        #region Commands

        /// <summary>
        /// Command to refresh the customer list from storage and optionally from external sources.
        /// This command reloads data and updates the UI with any changes.
        /// </summary>
        public ICommand RefreshCommand { get; private set; }

        /// <summary>
        /// Command to add a new customer.
        /// This command navigates to the Edit page in "add new" mode.
        /// </summary>
        public ICommand AddCustomerCommand { get; private set; }

        /// <summary>
        /// Command to edit the currently selected customer.
        /// This command navigates to the Edit page with the selected customer data.
        /// </summary>
        public ICommand EditSelectedCustomerCommand { get; private set; }

        /// <summary>
        /// Command to delete the currently selected customer.
        /// This command shows a confirmation dialog and removes the customer if confirmed.
        /// </summary>
        public ICommand DeleteSelectedCustomerCommand { get; private set; }

        /// <summary>
        /// Command to clear the current search filter.
        /// This command resets the search text and shows all customers.
        /// </summary>
        public ICommand ClearSearchCommand { get; private set; }

        /// <summary>
        /// Command to load sample data from the external API.
        /// This command demonstrates network operations and data import functionality.
        /// </summary>
        public ICommand LoadExternalDataCommand { get; private set; }

        /// <summary>
        /// Command to export customer data.
        /// This command demonstrates data export functionality.
        /// </summary>
        public ICommand ExportDataCommand { get; private set; }

        /// <summary>
        /// Command to handle customer selection (e.g., double-click to edit).
        /// This provides an alternative way to initiate editing operations.
        /// </summary>
        public ICommand CustomerSelectedCommand { get; private set; }

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the HomeViewModel class.
        /// Sets up service references, initializes commands, and loads initial data.
        /// </summary>
        public HomeViewModel()
        {
            // Get service references
            _dataService = DataService.Instance;
            _networkService = NetworkService.Instance;
            _settingsService = SettingsService.Instance;

            // Initialize properties
            Title = "Customer Management";
            SearchText = string.Empty;
            FilteredCustomers = new ObservableCollection<Customer>();

            // Initialize commands
            InitializeCommands();

            // Load initial data
            _ = LoadDataAsync();

            Debug.WriteLine("HomeViewModel: Initialized successfully");
        }

        #endregion

        #region Initialization

        /// <summary>
        /// Initializes all command properties with their respective implementations.
        /// This method sets up the command bindings that will be used by the UI.
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
            InitializeCommands();
        }

        /// <summary>
        /// Creates and configures all command objects used by this ViewModel.
        /// Each command includes both an execution action and a condition for when it can execute.
        /// </summary>
        private void InitializeCommands()
        {
            // Refresh command - always available
            RefreshCommand = new RelayCommand(
                async () => await RefreshDataAsync(),
                () => !IsRefreshing && !IsBusy);

            // Add customer command - always available
            AddCustomerCommand = new RelayCommand(
                () => NavigateToAddCustomer(),
                () => !IsBusy);

            // Edit selected customer command - requires selection
            EditSelectedCustomerCommand = new RelayCommand(
                () => NavigateToEditCustomer(SelectedCustomer),
                () => SelectedCustomer != null && !IsBusy);

            // Delete selected customer command - requires selection
            DeleteSelectedCustomerCommand = new RelayCommand(
                async () => await DeleteSelectedCustomerAsync(),
                () => SelectedCustomer != null && !IsBusy);

            // Clear search command - available when search is active
            ClearSearchCommand = new RelayCommand(
                () => ClearSearch(),
                () => IsSearchActive);

            // Load external data command - available when network is available
            LoadExternalDataCommand = new RelayCommand(
                async () => await LoadExternalDataAsync(),
                () => _networkService.IsAvailable && !IsLoadingExternalData && !IsBusy);

            // Export data command - available when there are customers
            ExportDataCommand = new RelayCommand(
                async () => await ExportDataAsync(),
                () => HasCustomers && !IsBusy);

            // Customer selected command - for handling selection events
            CustomerSelectedCommand = new RelayCommand<Customer>(
                customer => OnCustomerSelected(customer),
                customer => customer != null);
        }

        #endregion

        #region Data Loading

        /// <summary>
        /// Loads customer data and initializes the ViewModel state.
        /// This method is called during ViewModel initialization and when explicitly refreshing data.
        /// </summary>
        /// <returns>A task representing the asynchronous load operation</returns>
        private async Task LoadDataAsync()
        {
            try
            {
                IsBusy = true;
                ClearError();

                Debug.WriteLine("HomeViewModel: Loading customer data");

                // Ensure data service is loaded
                if (!_dataService.IsDataLoaded)
                {
                    await _dataService.LoadDataAsync();
                }

                // Initialize filtered customers
                FilterCustomers();

                // Update summary
                UpdateCustomerSummary();

                // Restore previous selection if any
                RestoreSelection();

                Debug.WriteLine($"HomeViewModel: Loaded {Customers.Count} customers");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"HomeViewModel: Error loading data: {ex.Message}");
                SetError(ex, "Unable to load customer data. Please try again.");
            }
            finally
            {
                IsBusy = false;
            }
        }

        /// <summary>
        /// Refreshes customer data from storage and updates the UI.
        /// This method can be called by the user to get the latest data.
        /// </summary>
        /// <returns>A task representing the asynchronous refresh operation</returns>
        private async Task RefreshDataAsync()
        {
            try
            {
                IsRefreshing = true;
                ClearError();

                Debug.WriteLine("HomeViewModel: Refreshing customer data");

                // Reload data from storage
                await _dataService.LoadDataAsync();

                // Update filtered customers
                FilterCustomers();

                // Update summary
                UpdateCustomerSummary();

                // Refresh UI
                OnPropertiesChanged(nameof(HasCustomers), nameof(FilteredCustomerCount));

                Debug.WriteLine("HomeViewModel: Data refresh completed");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"HomeViewModel: Error refreshing data: {ex.Message}");
                SetError(ex, "Unable to refresh customer data. Please check your connection and try again.");
            }
            finally
            {
                IsRefreshing = false;
                RaiseCanExecuteChanged();
            }
        }

        /// <summary>
        /// Loads sample data from the external JSONPlaceholder API.
        /// This method demonstrates network operations and data import functionality.
        /// </summary>
        /// <returns>A task representing the asynchronous load operation</returns>
        private async Task LoadExternalDataAsync()
        {
            try
            {
                IsLoadingExternalData = true;
                ClearError();

                Debug.WriteLine("HomeViewModel: Loading external data from API");

                // Check network availability
                if (!_networkService.IsAvailable)
                {
                    SetError("Network not available. Please check your connection and try again.");
                    return;
                }

                // Test API connectivity
                bool apiAvailable = await _networkService.TestApiConnectivityAsync();
                if (!apiAvailable)
                {
                    SetError("API not available. Please try again later.");
                    return;
                }

                // Load users from API
                var externalUsers = await _networkService.GetUsersAsync();
                
                if (externalUsers?.Count > 0)
                {
                    int importedCount = 0;
                    
                    foreach (var user in externalUsers)
                    {
                        // Convert to Customer and check if already exists
                        var customer = _networkService.ConvertToCustomer(user);
                        
                        // Check if customer already exists (by email)
                        bool exists = Customers.Any(c => 
                            string.Equals(c.Email, customer.Email, StringComparison.OrdinalIgnoreCase));
                        
                        if (!exists)
                        {
                            await _dataService.AddCustomerAsync(customer);
                            importedCount++;
                        }
                    }

                    // Update UI
                    FilterCustomers();
                    UpdateCustomerSummary();
                    OnPropertiesChanged(nameof(HasCustomers), nameof(FilteredCustomerCount));

                    Debug.WriteLine($"HomeViewModel: Imported {importedCount} new customers from external API");
                    
                    if (importedCount > 0)
                    {
                        // Could show success message here
                    }
                    else
                    {
                        SetError("No new customers to import. All external customers already exist in the database.");
                    }
                }
                else
                {
                    SetError("No data received from external API. Please try again later.");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"HomeViewModel: Error loading external data: {ex.Message}");
                SetError(ex, "Unable to load data from external API. Please check your connection and try again.");
            }
            finally
            {
                IsLoadingExternalData = false;
                RaiseCanExecuteChanged();
            }
        }

        #endregion

        #region Search and Filtering

        /// <summary>
        /// Filters the customer list based on the current search text.
        /// This method updates the FilteredCustomers collection to show only matching customers.
        /// </summary>
        /// <remarks>
        /// The search is performed across multiple customer fields:
        /// - First Name
        /// - Last Name
        /// - Email
        /// - Company
        /// 
        /// The search is case-insensitive and supports partial matches.
        /// When no search text is provided, all customers are shown.
        /// </remarks>
        private void FilterCustomers()
        {
            try
            {
                FilteredCustomers.Clear();

                if (string.IsNullOrWhiteSpace(SearchText))
                {
                    // No search - show all customers
                    foreach (var customer in Customers)
                    {
                        FilteredCustomers.Add(customer);
                    }
                }
                else
                {
                    // Filter based on search criteria
                    var filteredResults = _dataService.SearchCustomers(SearchText);
                    foreach (var customer in filteredResults)
                    {
                        FilteredCustomers.Add(customer);
                    }
                }

                // Update related properties
                OnPropertiesChanged(
                    nameof(FilteredCustomerCount),
                    nameof(IsSearchActive));

                Debug.WriteLine($"HomeViewModel: Filtered customers - showing {FilteredCustomers.Count} of {Customers.Count}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"HomeViewModel: Error filtering customers: {ex.Message}");
            }
        }

        /// <summary>
        /// Clears the current search filter and shows all customers.
        /// </summary>
        private void ClearSearch()
        {
            try
            {
                SearchText = string.Empty;
                // FilterCustomers() is automatically called by the SearchText setter
                Debug.WriteLine("HomeViewModel: Search cleared");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"HomeViewModel: Error clearing search: {ex.Message}");
            }
        }

        #endregion

        #region Customer Operations

        /// <summary>
        /// Navigates to the Edit page to add a new customer.
        /// </summary>
        private void NavigateToAddCustomer()
        {
            try
            {
                Debug.WriteLine("HomeViewModel: Navigating to add new customer");
                
                // Navigate to Edit page without a customer (add mode)
                NavigationHelper.NavigateTo<Views.EditPage>(null);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"HomeViewModel: Error navigating to add customer: {ex.Message}");
                SetError("Unable to open the add customer page. Please try again.");
            }
        }

        /// <summary>
        /// Navigates to the Edit page to edit the specified customer.
        /// </summary>
        /// <param name="customer">The customer to edit</param>
        private void NavigateToEditCustomer(Customer customer)
        {
            try
            {
                if (customer == null)
                {
                    SetError("Please select a customer to edit.");
                    return;
                }

                Debug.WriteLine($"HomeViewModel: Navigating to edit customer {customer.FullName}");
                
                // Navigate to Edit page with the selected customer
                NavigationHelper.NavigateTo<Views.EditPage>(customer);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"HomeViewModel: Error navigating to edit customer: {ex.Message}");
                SetError("Unable to open the edit customer page. Please try again.");
            }
        }

        /// <summary>
        /// Deletes the currently selected customer after confirmation.
        /// This method shows a confirmation dialog before performing the delete operation.
        /// </summary>
        /// <returns>A task representing the asynchronous delete operation</returns>
        private async Task DeleteSelectedCustomerAsync()
        {
            try
            {
                if (SelectedCustomer == null)
                {
                    SetError("Please select a customer to delete.");
                    return;
                }

                var customerToDelete = SelectedCustomer;
                
                Debug.WriteLine($"HomeViewModel: Deleting customer {customerToDelete.FullName}");

                // In a real app, you would show a confirmation dialog here
                // For this demo, we'll proceed with the deletion
                
                // Clear selection before deletion
                SelectedCustomer = null;

                // Delete the customer
                await _dataService.RemoveCustomerAsync(customerToDelete);

                // Update UI
                FilterCustomers();
                UpdateCustomerSummary();
                OnPropertiesChanged(nameof(HasCustomers), nameof(FilteredCustomerCount));

                Debug.WriteLine($"HomeViewModel: Successfully deleted customer {customerToDelete.FullName}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"HomeViewModel: Error deleting customer: {ex.Message}");
                SetError(ex, "Unable to delete the selected customer. Please try again.");
            }
            finally
            {
                RaiseCanExecuteChanged();
            }
        }

        /// <summary>
        /// Handles customer selection events (e.g., double-click to edit).
        /// </summary>
        /// <param name="customer">The customer that was selected</param>
        private void OnCustomerSelected(Customer customer)
        {
            try
            {
                if (customer != null)
                {
                    SelectedCustomer = customer;
                    
                    // For double-click behavior, could navigate to edit
                    // NavigateToEditCustomer(customer);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"HomeViewModel: Error handling customer selection: {ex.Message}");
            }
        }

        #endregion

        #region Selection Management

        /// <summary>
        /// Handles changes to the selected customer.
        /// This method updates command availability and performs related actions.
        /// </summary>
        private void OnSelectionChanged()
        {
            try
            {
                // Update command availability
                RaiseCanExecuteChanged();
                
                Debug.WriteLine($"HomeViewModel: Selection changed to {SelectedCustomer?.FullName ?? "None"}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"HomeViewModel: Error handling selection change: {ex.Message}");
            }
        }

        /// <summary>
        /// Persists the current customer selection to settings for state restoration.
        /// </summary>
        private void PersistSelection()
        {
            try
            {
                string customerId = SelectedCustomer?.Id.ToString() ?? string.Empty;
                _settingsService.SetSelectedCustomerId(customerId);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"HomeViewModel: Error persisting selection: {ex.Message}");
            }
        }

        /// <summary>
        /// Restores the previously selected customer from settings.
        /// </summary>
        private void RestoreSelection()
        {
            try
            {
                string savedCustomerId = _settingsService.GetSelectedCustomerId();
                if (!string.IsNullOrEmpty(savedCustomerId) && int.TryParse(savedCustomerId, out int customerId))
                {
                    var customer = _dataService.GetCustomerById(customerId);
                    if (customer != null)
                    {
                        SelectedCustomer = customer;
                        Debug.WriteLine($"HomeViewModel: Restored selection to {customer.FullName}");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"HomeViewModel: Error restoring selection: {ex.Message}");
            }
        }

        #endregion

        #region UI Updates

        /// <summary>
        /// Updates the customer summary text with current statistics.
        /// This method generates a summary of the customer database for display in the UI.
        /// </summary>
        private void UpdateCustomerSummary()
        {
            try
            {
                if (!HasCustomers)
                {
                    CustomerSummary = "No customers in the database. Add some customers to get started.";
                    return;
                }

                int totalCustomers = Customers.Count;
                int customersWithCompany = Customers.Count(c => !string.IsNullOrWhiteSpace(c.Company));
                int recentCustomers = Customers.Count(c => (DateTime.Now - c.DateCreated).TotalDays <= 7);

                CustomerSummary = $"{totalCustomers} customer{(totalCustomers != 1 ? "s" : "")} total, " +
                                 $"{customersWithCompany} with company info, " +
                                 $"{recentCustomers} added this week";

                Debug.WriteLine($"HomeViewModel: Updated customer summary: {CustomerSummary}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"HomeViewModel: Error updating customer summary: {ex.Message}");
                CustomerSummary = "Unable to generate customer summary.";
            }
        }

        /// <summary>
        /// Raises CanExecuteChanged for all commands to update UI button states.
        /// This method should be called whenever conditions that affect command availability change.
        /// </summary>
        private void RaiseCanExecuteChanged()
        {
            try
            {
                (RefreshCommand as RelayCommand)?.RaiseCanExecuteChanged();
                (AddCustomerCommand as RelayCommand)?.RaiseCanExecuteChanged();
                (EditSelectedCustomerCommand as RelayCommand)?.RaiseCanExecuteChanged();
                (DeleteSelectedCustomerCommand as RelayCommand)?.RaiseCanExecuteChanged();
                (ClearSearchCommand as RelayCommand)?.RaiseCanExecuteChanged();
                (LoadExternalDataCommand as RelayCommand)?.RaiseCanExecuteChanged();
                (ExportDataCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"HomeViewModel: Error raising CanExecuteChanged: {ex.Message}");
            }
        }

        #endregion

        #region Export Operations

        /// <summary>
        /// Exports customer data to a file.
        /// This method demonstrates data export functionality.
        /// </summary>
        /// <returns>A task representing the asynchronous export operation</returns>
        private async Task ExportDataAsync()
        {
            try
            {
                IsBusy = true;
                ClearError();

                Debug.WriteLine("HomeViewModel: Starting data export");

                // In a real app, you would show a file picker and export to the selected location
                // For this demo, we'll just save to the app's local folder with a timestamp
                
                bool success = await _dataService.SaveDataAsync();
                
                if (success)
                {
                    Debug.WriteLine("HomeViewModel: Data export completed successfully");
                    // Could show success message here
                }
                else
                {
                    SetError("Unable to export customer data. Please try again.");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"HomeViewModel: Error exporting data: {ex.Message}");
                SetError(ex, "Unable to export customer data. Please try again.");
            }
            finally
            {
                IsBusy = false;
                RaiseCanExecuteChanged();
            }
        }

        #endregion

        #region Cleanup

        /// <summary>
        /// Performs cleanup when the ViewModel is no longer needed.
        /// This method ensures proper resource disposal and event unsubscription.
        /// </summary>
        protected override void Cleanup()
        {
            try
            {
                // Clear collections
                FilteredCustomers?.Clear();
                
                // Clear selection
                SelectedCustomer = null;
                
                // Clear search
                SearchText = string.Empty;
                
                Debug.WriteLine("HomeViewModel: Cleanup completed");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"HomeViewModel: Error during cleanup: {ex.Message}");
            }
            finally
            {
                base.Cleanup();
            }
        }

        #endregion
    }
}