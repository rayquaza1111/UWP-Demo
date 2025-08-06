using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using UWP_Demo.Commands;
using UWP_Demo.Models;
using UWP_Demo.Services;
using UWP_Demo.Views;

namespace UWP_Demo.ViewModels
{
    /// <summary>
    /// Home page view model that orchestrates file-based customer management
    /// </summary>
    public class HomeViewModel : INotifyPropertyChanged
    {
        private readonly CustomerService _customerService;
        private readonly DialogService _dialogService;

        private ObservableCollection<Customer> _customers;
        private ObservableCollection<Customer> _filteredCustomers;
        private Customer _selectedCustomer;
        private string _searchText = "";
        private string _fileStatus = "Loading...";
        // 6. Suspension & Resume Handling: Welcome message field
        private string _welcomeMessage = "";

        public HomeViewModel()
        {
            System.Diagnostics.Debug.WriteLine("HomeViewModel constructor - initializing file-based customer management");
            
            _customerService = new CustomerService();
            _dialogService = new DialogService();

            Customers = new ObservableCollection<Customer>();
            FilteredCustomers = new ObservableCollection<Customer>();

            InitializeCommands();
            // 6. Suspension & Resume Handling: Initialize welcome message
            InitializeWelcomeMessage();
            LoadCustomersAsync();
            
            System.Diagnostics.Debug.WriteLine("HomeViewModel constructor completed, file loading initiated");
        }

        public string Title => "Customer Management";

        public string CustomerSummary
        {
            get
            {
                if (Customers == null || Customers.Count == 0)
                    return "No customers yet";
                
                return Customers.Count == 1 ? "1 customer in database" : $"{Customers.Count} customers in database";
            }
        }

        /// <summary>
        /// Enhanced file status for InfoBar display
        /// </summary>
        public string FileStatus
        {
            get => _fileStatus;
            set
            {
                _fileStatus = value;
                OnPropertyChanged();
                UpdateInfoBarSeverity(value);
                System.Diagnostics.Debug.WriteLine($"InfoBar status updated: {value}");
            }
        }

        /// <summary>
        /// InfoBar severity property for different message types
        /// </summary>
        private Microsoft.UI.Xaml.Controls.InfoBarSeverity _infoBarSeverity = Microsoft.UI.Xaml.Controls.InfoBarSeverity.Informational;
        public Microsoft.UI.Xaml.Controls.InfoBarSeverity InfoBarSeverity
        {
            get => _infoBarSeverity;
            set
            {
                _infoBarSeverity = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Update InfoBar severity based on file operation status
        /// </summary>
        private void UpdateInfoBarSeverity(string status)
        {
            if (status.Contains("Error") || status.Contains("Failed"))
            {
                InfoBarSeverity = Microsoft.UI.Xaml.Controls.InfoBarSeverity.Error;
            }
            else if (status.Contains("Loading") || status.Contains("Adding") || status.Contains("Deleting"))
            {
                InfoBarSeverity = Microsoft.UI.Xaml.Controls.InfoBarSeverity.Warning;
            }
            else if (status.Contains("Success") || status.Contains("Added") || status.Contains("Deleted") || status.Contains("Loaded"))
            {
                InfoBarSeverity = Microsoft.UI.Xaml.Controls.InfoBarSeverity.Success;
            }
            else
            {
                InfoBarSeverity = Microsoft.UI.Xaml.Controls.InfoBarSeverity.Informational;
            }
        }

        public ObservableCollection<Customer> Customers
        {
            get => _customers;
            set
            {
                _customers = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CustomerSummary));
                OnPropertyChanged(nameof(HasCustomers));
            }
        }

        public ObservableCollection<Customer> FilteredCustomers
        {
            get => _filteredCustomers;
            set
            {
                _filteredCustomers = value;
                OnPropertyChanged();
            }
        }

        public Customer SelectedCustomer
        {
            get => _selectedCustomer;
            set
            {
                _selectedCustomer = value;
                OnPropertyChanged();
            }
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged();
                FilterCustomers();
            }
        }

        public bool HasCustomers => Customers != null && Customers.Count > 0;

        // 6. Suspension & Resume Handling: Welcome message properties
        public string WelcomeMessage
        {
            get => _welcomeMessage;
            set
            {
                _welcomeMessage = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ShowWelcomeMessage));
            }
        }

        public bool ShowWelcomeMessage => !string.IsNullOrEmpty(_welcomeMessage);

        // Commands
        public ICommand AddCustomerCommand { get; private set; }
        public ICommand EditSelectedCustomerCommand { get; private set; }
        public ICommand DeleteSelectedCustomerCommand { get; private set; }
        public ICommand RefreshCommand { get; private set; }
        public ICommand LoadExternalDataCommand { get; private set; }
        public ICommand ShowFileInfoCommand { get; private set; }
        // 6. Suspension & Resume Handling: Commands for welcome message
        public ICommand DismissWelcomeCommand { get; private set; }
        public ICommand RefreshWelcomeCommand { get; private set; }
        public ICommand NavigateToEditCommand { get; private set; }
        public ICommand NavigateToNewCustomerCommand { get; private set; }

        private void InitializeCommands()
        {
            System.Diagnostics.Debug.WriteLine("Initializing commands...");
            
            AddCustomerCommand = new RelayCommand(AddCustomer);
            EditSelectedCustomerCommand = new RelayCommand<Customer>(EditCustomer);
            DeleteSelectedCustomerCommand = new RelayCommand<Customer>(DeleteCustomer);
            RefreshCommand = new RelayCommand(() => LoadCustomersAsync());
            LoadExternalDataCommand = new RelayCommand(LoadExternalData);
            ShowFileInfoCommand = new RelayCommand(ShowFileInfo);
            // 6. Suspension & Resume Handling: Commands for welcome message
            DismissWelcomeCommand = new RelayCommand(DismissWelcomeMessage);
            RefreshWelcomeCommand = new RelayCommand(RefreshWelcomeMessage);
            NavigateToEditCommand = new RelayCommand<Customer>(NavigateToEditCustomer);
            NavigateToNewCustomerCommand = new RelayCommand(NavigateToNewCustomer);
            
            System.Diagnostics.Debug.WriteLine("Commands initialized successfully");
        }

        /// <summary>
        /// Navigation System: Navigate to edit customer with state preservation
        /// Navigation System: Enhanced: Use MainPage's direct navigation methods
        /// Navigation System: Called when user clicks Edit button on customer list
        /// </summary>
        private void NavigateToEditCustomer(Customer customer)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"Navigation System: HomeViewModel navigating to edit customer: {customer?.FullName}");
                
                if (customer != null)
                {
                    // Navigation System: Try to use MainPage's direct navigation method
                    try
                    {
                        var currentFrame = Windows.UI.Xaml.Window.Current.Content as Windows.UI.Xaml.Controls.Frame;
                        if (currentFrame?.Content is MainPage mainPage)
                        {
                            mainPage.NavigateToEditWithCustomer(customer);
                            System.Diagnostics.Debug.WriteLine("Navigation System: Successfully used MainPage.NavigateToEditWithCustomer");
                            return;
                        }
                    }
                    catch (Exception navEx)
                    {
                        System.Diagnostics.Debug.WriteLine($"Navigation System: MainPage navigation failed - {navEx.Message}");
                    }
                    
                    // Navigation System: FALLBACK: Store state and show informative message
                    var stateService = NavigationStateService.Instance;
                    stateService.SetSelectedCustomerForEdit(customer);
                    
                    _dialogService.ShowMessageAsync("Edit Customer", 
                        $"Customer '{customer.FullName}' has been selected for editing.\n\n" +
                        $"Please navigate to 'Customer Editor' from the main menu to edit this customer.\n\n" +
                        $"The customer data has been stored and will be automatically loaded.");
                    
                    System.Diagnostics.Debug.WriteLine("Navigation System: Customer edit navigation completed with fallback");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Navigation System ERROR: Failed to navigate to edit customer - {ex.Message}");
            }
        }

        /// <summary>
        /// Navigation System: Navigate to new customer creation
        /// Navigation System: Enhanced: Use MainPage's direct navigation methods
        /// Navigation System: Called when user wants to create a new customer
        /// </summary>
        private void NavigateToNewCustomer()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("Navigation System: HomeViewModel navigating to new customer");
                
                // Navigation System: Try to use MainPage's direct navigation method
                try
                {
                    var currentFrame = Windows.UI.Xaml.Window.Current.Content as Windows.UI.Xaml.Controls.Frame;
                    if (currentFrame?.Content is MainPage mainPage)
                    {
                        mainPage.NavigateToNewCustomer();
                        System.Diagnostics.Debug.WriteLine("Navigation System: Successfully used MainPage.NavigateToNewCustomer");
                        return;
                    }
                }
                catch (Exception navEx)
                {
                    System.Diagnostics.Debug.WriteLine($"Navigation System: MainPage navigation failed - {navEx.Message}");
                }
                
                // Navigation System: FALLBACK: Clear state and show informative message
                var stateService = NavigationStateService.Instance;
                stateService.ClearAllState();
                
                _dialogService.ShowMessageAsync("New Customer", 
                    "Ready to create a new customer!\n\n" +
                    "Please navigate to 'Customer Editor' from the main menu to create a new customer.\n\n" +
                    "The form has been cleared and is ready for new customer data.");
                
                System.Diagnostics.Debug.WriteLine("Navigation System: New customer navigation completed with fallback");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Navigation System ERROR: Failed to navigate to new customer - {ex.Message}");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #region Missing Methods

        // 6. Suspension & Resume Handling: Initialize welcome message based on app state
        private void InitializeWelcomeMessage()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("Initializing welcome message...");
                
                // Get welcome message from SuspensionService
                WelcomeMessage = SuspensionService.Instance.GetWelcomeBackMessage();
                OnPropertyChanged(nameof(ShowWelcomeMessage));
                
                System.Diagnostics.Debug.WriteLine($"Welcome message initialized: '{WelcomeMessage}' (ShowWelcomeMessage: {ShowWelcomeMessage})");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to initialize welcome message - {ex.Message}");
                WelcomeMessage = "Welcome to Customer Management!";
                OnPropertyChanged(nameof(ShowWelcomeMessage));
            }
        }

        // 6. Suspension & Resume Handling: Dismiss welcome message
        public void DismissWelcomeMessage()
        {
            System.Diagnostics.Debug.WriteLine("Dismissing welcome message");
            
            // Clear suspension flag when user dismisses message
            SuspensionService.Instance.ClearSuspensionFlag();
            
            WelcomeMessage = "";
            OnPropertyChanged(nameof(ShowWelcomeMessage));
            System.Diagnostics.Debug.WriteLine("Welcome message dismissed and suspension flag cleared");
        }

        // 6. Suspension & Resume Handling: Refresh welcome message
        private void RefreshWelcomeMessage()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("Refreshing welcome message...");
                WelcomeMessage = SuspensionService.Instance.GetWelcomeBackMessage();
                OnPropertyChanged(nameof(ShowWelcomeMessage));
                System.Diagnostics.Debug.WriteLine($"Welcome message refreshed: '{WelcomeMessage}'");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to refresh welcome message - {ex.Message}");
            }
        }

        // 6. Suspension & Resume Handling: Debug suspension state
        public void DebugSuspensionState()
        {
            try
            {
                var suspensionService = SuspensionService.Instance;
                var debugInfo = suspensionService.GetDetailedDebugInfo();
                System.Diagnostics.Debug.WriteLine("=== SUSPENSION STATE DEBUG ===");
                System.Diagnostics.Debug.WriteLine(debugInfo);
                System.Diagnostics.Debug.WriteLine("=== END SUSPENSION DEBUG ===");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to debug suspension state - {ex.Message}");
            }
        }

        // 6. Suspension & Resume Handling: Test suspension and resume
        public void TestSuspensionAndResume()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("Testing suspension and resume...");
                
                // Test suspension 30 seconds ago
                SuspensionService.Instance.SetTestSuspensionState(30);
                
                // Refresh welcome message
                InitializeWelcomeMessage();
                
                System.Diagnostics.Debug.WriteLine("Test suspension completed - welcome message should be visible");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to test suspension and resume - {ex.Message}");
            }
        }

        // 6. Suspension & Resume Handling: Test window minimization
        public void TestWindowMinimization()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("Testing window minimization simulation...");
                
                // Simulate the app being away for 2 minutes
                SuspensionService.Instance.SetTestSuspensionState(120);
                
                // Update welcome message
                InitializeWelcomeMessage();
                
                System.Diagnostics.Debug.WriteLine("Window minimization test completed");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to test window minimization - {ex.Message}");
            }
        }

        /// <summary>
        /// FILE I/O: PRIMARY LOAD OPERATION - Loads customers from file and updates UI
        /// </summary>
        private async void LoadCustomersAsync()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("LoadCustomersAsync - requesting customer data from file");
                
                // FILE I/O: Update status to show file loading is in progress
                FileStatus = "Loading customers from file...";
                
                // FILE I/O: Request customer data - this triggers file load in CustomerService
                var customers = await _customerService.GetCustomersAsync();  // FILE I/O: This loads from file
                
                // Update UI with loaded data
                Customers = new ObservableCollection<Customer>(customers);
                FilterCustomers();
                
                // FILE I/O: Get and display file information for user feedback
                var fileInfo = await _customerService.GetFileInfoAsync();  // FILE I/O: Get file details
                FileStatus = $"Loaded {customers.Count} customers. {fileInfo}";
                
                System.Diagnostics.Debug.WriteLine($"FILE I/O: Successfully loaded {customers.Count} customers from file and updated UI");
            }
            catch (Exception ex)
            {
                // FILE I/O: Handle file loading errors gracefully
                System.Diagnostics.Debug.WriteLine($"FILE I/O ERROR: Failed to load customers from file - {ex.Message}");
                FileStatus = $"Error loading customers: {ex.Message}";
            }
        }

        private void FilterCustomers()
        {
            if (Customers == null)
            {
                FilteredCustomers = new ObservableCollection<Customer>();
                return;
            }

            if (string.IsNullOrWhiteSpace(SearchText))
            {
                FilteredCustomers = new ObservableCollection<Customer>(Customers);
            }
            else
            {
                var searchLower = SearchText.ToLower();
                var filtered = Customers.Where(c =>
                    c.FullName.ToLower().Contains(searchLower) ||
                    c.Email.ToLower().Contains(searchLower) ||
                    (!string.IsNullOrEmpty(c.Company) && c.Company.ToLower().Contains(searchLower)));
                FilteredCustomers = new ObservableCollection<Customer>(filtered);
            }
        }

        /// <summary>
        /// FILE I/O: ADD CUSTOMER OPERATION - Creates new customer and triggers auto-save to file
        /// </summary>
        private async void AddCustomer()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("AddCustomer - starting add operation with file save");
                
                // FILE I/O: Update status to show add operation is in progress
                FileStatus = "Adding new customer...";
                
                // Create new customer object
                var newCustomer = new Customer 
                { 
                    FirstName = "New", 
                    LastName = $"Customer {Customers.Count + 1}", 
                    Email = $"customer{Customers.Count + 1}@example.com",
                    Company = "Sample Company"
                };
                
                // FILE I/O: Add customer to service - this triggers automatic save to file
                await _customerService.AddCustomerAsync(newCustomer);  // FILE I/O: Add + auto-save to file
                
                // FILE I/O: Refresh UI by reloading from file to confirm save worked
                LoadCustomersAsync();  // FILE I/O: Reload from file to show updated data
                
                // FILE I/O: Update status to confirm successful save
                FileStatus = $"Added customer: {newCustomer.FullName}";
                
                // FILE I/O: Show user confirmation that data was saved to file
                await _dialogService.ShowMessageAsync(
                    "Customer Added", 
                    $"Successfully added {newCustomer.FullName} and saved to file!");
                
                System.Diagnostics.Debug.WriteLine($"AddCustomer completed - {newCustomer.FullName} added and saved to file");
            }
            catch (Exception ex)
            {
                // FILE I/O: Handle add/save errors with user notification
                System.Diagnostics.Debug.WriteLine($"FILE I/O ERROR: Failed to add customer and save to file - {ex.Message}");
                FileStatus = $"Error adding customer: {ex.Message}";
                
                await _dialogService.ShowMessageAsync("Error", $"Failed to add customer: {ex.Message}");
            }
        }

        /// <summary>
        /// Edit customer command handler
        /// </summary>
        private async void EditCustomer(Customer customer)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"EditCustomer called for {customer?.FullName}");
                
                if (customer != null)
                {
                    // Edit functionality would call UpdateCustomerAsync() which auto-saves to file
                    await _dialogService.ShowMessageAsync("Edit", $"Edit feature for {customer.FullName} coming soon!");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in EditCustomer: {ex.Message}");
            }
        }

        /// <summary>
        /// Delete customer operation - removes customer and triggers auto-save to file
        /// </summary>
        private async void DeleteCustomer(Customer customer)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"DeleteCustomer - starting delete operation for {customer?.FullName}");
                
                if (customer == null) return;

                // Confirm deletion with user
                var result = await _dialogService.ShowConfirmationAsync(
                    "Delete Customer",
                    $"Are you sure you want to delete {customer.FullName}?\n\nThis will permanently remove them from the file.",
                    "Delete",
                    "Cancel");

                if (result == Windows.UI.Xaml.Controls.ContentDialogResult.Primary)
                {
                    FileStatus = $"Deleting {customer.FullName}...";
                    
                    // Delete customer from service - this triggers automatic save to file
                    await _customerService.DeleteCustomerAsync(customer.Id);
                    
                    // Update UI immediately for responsive feel
                    Customers.Remove(customer);
                    FilterCustomers();
                    
                    FileStatus = $"Deleted customer: {customer.FullName}";
                    
                    // Show user confirmation that data was removed from file
                    await _dialogService.ShowMessageAsync(
                        "Customer Deleted", 
                        $"Successfully deleted {customer.FullName} and updated file!");
                
                    System.Diagnostics.Debug.WriteLine($"DeleteCustomer completed - {customer.FullName} deleted and file updated");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to delete customer and update file - {ex.Message}");
                FileStatus = $"Error deleting customer: {ex.Message}";
                
                await _dialogService.ShowMessageAsync("Error", $"Failed to delete customer: {ex.Message}");
            }
        }

        /// <summary>
        /// Load sample data operation
        /// </summary>
        private async void LoadExternalData()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("LoadExternalData - starting bulk add operation with file save");
                
                FileStatus = "Loading sample data...";
                
                var sampleCustomers = new[]
                {
                    new Customer { FirstName = "Alice", LastName = "Johnson", Email = "alice@example.com", Company = "Tech Corp" },
                    new Customer { FirstName = "Bob", LastName = "Wilson", Email = "bob@wilson.net", Company = "Design Studio" },
                    new Customer { FirstName = "Carol", LastName = "Davis", Email = "carol.davis@business.com", Company = "Marketing Solutions" }
                };

                int addedCount = 0;
                foreach (var customer in sampleCustomers)
                {
                    try
                    {
                        await _customerService.AddCustomerAsync(customer);
                        addedCount++;
                        
                        System.Diagnostics.Debug.WriteLine($"Added sample customer: {customer.FullName}");
                    }
                    catch (Exception customerEx)
                    {
                        System.Diagnostics.Debug.WriteLine($"Failed to add sample customer {customer.FullName}: {customerEx.Message}");
                    }
                }

                LoadCustomersAsync();
                
                FileStatus = $"Added {addedCount} sample customers to file";
                
                await _dialogService.ShowMessageAsync(
                    "Sample Data Loaded", 
                    $"Successfully added {addedCount} sample customers!");
                
                System.Diagnostics.Debug.WriteLine($"LoadExternalData completed - {addedCount} customers added and saved to file");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load external data and save to file - {ex.Message}");
                FileStatus = $"Error loading sample data: {ex.Message}";
                
                await _dialogService.ShowMessageAsync("Error", $"Failed to load sample data: {ex.Message}");
            }
        }

        /// <summary>
        /// FILE I/O: FILE INFO OPERATION - Shows detailed file information to user
        /// </summary>
        private async void ShowFileInfo()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("ShowFileInfo - requesting file details for user display");
                
                // FILE I/O: Get file information from service
                var fileInfo = await _customerService.GetFileInfoAsync();  // FILE I/O: Query file system details
                
                // FILE I/O: Display file details to user
                await _dialogService.ShowMessageAsync("File Information", fileInfo);
                
                System.Diagnostics.Debug.WriteLine($"Displayed file info to user: {fileInfo}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to get file info - {ex.Message}");
                await _dialogService.ShowMessageAsync("Error", $"Failed to get file info: {ex.Message}");
            }
        }
        #endregion
    }
}