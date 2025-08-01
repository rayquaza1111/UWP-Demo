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
using UWP_Demo.Views;  // ?? STATE MANAGEMENT: Import Views namespace for HomePage

namespace UWP_Demo.ViewModels
{
    /// <summary>
    /// FILE I/O: Home page view model that orchestrates file-based customer management
    /// Acts as intermediary between UI and CustomerService file operations
    /// 
    /// FILE I/O RESPONSIBILITIES:
    /// 1. Trigger initial file load when app starts
    /// 2. Handle user actions that require file saves (Add, Delete)
    /// 3. Provide file operation status feedback to UI
    /// 4. Manage file operation error handling and user notifications
    /// </summary>
    public class HomeViewModel : INotifyPropertyChanged
    {
        // FILE I/O: Service that handles all file operations
        private readonly CustomerService _customerService;
        private readonly DialogService _dialogService;

        private ObservableCollection<Customer> _customers;
        private ObservableCollection<Customer> _filteredCustomers;
        private Customer _selectedCustomer;
        private string _searchText = "";
        
        // FILE I/O: Status property to show current file operation to user
        private string _fileStatus = "Loading...";  // FILE I/O: Tracks and displays file operation status
        
        // SUSPENSION & RESUME: Welcome message for users returning to the app
        private string _welcomeMessage = "";

        public HomeViewModel()
        {
            System.Diagnostics.Debug.WriteLine("FILE I/O: HomeViewModel constructor - initializing file-based customer management");
            
            // FILE I/O: Initialize service that handles file persistence
            _customerService = new CustomerService();
            _dialogService = new DialogService();

            Customers = new ObservableCollection<Customer>();
            FilteredCustomers = new ObservableCollection<Customer>();

            InitializeCommands();
            
            // SUSPENSION & RESUME: Initialize welcome message based on app state
            InitializeWelcomeMessage();
            
            // FILE I/O: Start the file loading process immediately when ViewModel is created
            LoadCustomersAsync();  // FILE I/O: This triggers the initial file load
            
            System.Diagnostics.Debug.WriteLine("FILE I/O: HomeViewModel constructor completed, file loading initiated");
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
        /// SUSPENSION & RESUME: Welcome message property for displaying app state information
        /// Shows welcome back messages, time away, and suspension state details
        /// </summary>
        public string WelcomeMessage
        {
            get => _welcomeMessage;
            set
            {
                _welcomeMessage = value;
                OnPropertyChanged();
                System.Diagnostics.Debug.WriteLine($"SUSPENSION & RESUME: Welcome message updated: {value}");
            }
        }

        /// <summary>
        /// SUSPENSION & RESUME: Whether to show the welcome message
        /// </summary>
        public bool ShowWelcomeMessage => !string.IsNullOrEmpty(WelcomeMessage);

        /// <summary>
        /// WINUI 2 UI ENHANCEMENT: Enhanced file status for InfoBar display
        /// Replaces basic text status with rich InfoBar messaging
        /// Shows different severity levels and actionable messages
        /// Automatically updates InfoBar appearance based on operation status
        /// </summary>
        public string FileStatus
        {
            get => _fileStatus;
            set
            {
                _fileStatus = value;
                OnPropertyChanged();  // FILE I/O: Notify UI to update InfoBar

                // WINUI 2 UI ENHANCEMENT: Update InfoBar severity based on message content
                // This provides automatic color coding and iconography for different states
                UpdateInfoBarSeverity(value);
                
                System.Diagnostics.Debug.WriteLine($"WINUI 2 UI ENHANCEMENT: InfoBar status updated: {value}");
            }
        }

        /// <summary>
        /// WINUI 2 UI ENHANCEMENT: InfoBar severity property for different message types
        /// Controls the visual appearance of the InfoBar (color, icon, style)
        /// Provides semantic meaning to status messages through visual indicators
        /// Values: Informational (blue), Success (green), Warning (yellow), Error (red)
        /// </summary>
        private Microsoft.UI.Xaml.Controls.InfoBarSeverity _infoBarSeverity = Microsoft.UI.Xaml.Controls.InfoBarSeverity.Informational;
        public Microsoft.UI.Xaml.Controls.InfoBarSeverity InfoBarSeverity
        {
            get => _infoBarSeverity;
            set
            {
                _infoBarSeverity = value;
                OnPropertyChanged();  // WINUI 2: Notify InfoBar to update visual appearance
            }
        }

        /// <summary>
        /// WINUI 2 UI ENHANCEMENT: Update InfoBar severity based on file operation status
        /// Automatically determines appropriate visual indicator based on message content
        /// Provides intelligent status categorization for better user experience
        /// 
        /// WINUI 2 SEVERITY MAPPING:
        /// - Error (Red): "Error", "Failed" keywords
        /// - Warning (Yellow): "Loading", "Adding", "Deleting" keywords (operations in progress)
        /// - Success (Green): "Success", "Added", "Deleted", "Loaded" keywords (completed operations)  
        /// - Informational (Blue): Default for general information
        /// </summary>
        private void UpdateInfoBarSeverity(string status)
        {
            // WINUI 2 UI ENHANCEMENT: Error states - red InfoBar with error icon
            if (status.Contains("Error") || status.Contains("Failed"))
            {
                InfoBarSeverity = Microsoft.UI.Xaml.Controls.InfoBarSeverity.Error;
            }
            // WINUI 2 UI ENHANCEMENT: Warning states - yellow InfoBar with warning icon
            else if (status.Contains("Loading") || status.Contains("Adding") || status.Contains("Deleting"))
            {
                InfoBarSeverity = Microsoft.UI.Xaml.Controls.InfoBarSeverity.Warning;
            }
            // WINUI 2 UI ENHANCEMENT: Success states - green InfoBar with checkmark icon
            else if (status.Contains("Success") || status.Contains("Added") || status.Contains("Deleted") || status.Contains("Loaded"))
            {
                InfoBarSeverity = Microsoft.UI.Xaml.Controls.InfoBarSeverity.Success;
            }
            // WINUI 2 UI ENHANCEMENT: Default informational state - blue InfoBar with info icon
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

        // FILE I/O: Commands that trigger file operations
        public ICommand AddCustomerCommand { get; private set; }        // FILE I/O: Triggers add + auto-save
        public ICommand EditSelectedCustomerCommand { get; private set; }
        public ICommand DeleteSelectedCustomerCommand { get; private set; }  // FILE I/O: Triggers delete + auto-save
        public ICommand RefreshCommand { get; private set; }                 // FILE I/O: Triggers reload from file
        public ICommand LoadExternalDataCommand { get; private set; }        // FILE I/O: Triggers bulk add + auto-save
        public ICommand ShowFileInfoCommand { get; private set; }            // FILE I/O: Shows file details
        public ICommand DismissWelcomeCommand { get; private set; }          // SUSPENSION & RESUME: Dismisses welcome message
        public ICommand RefreshWelcomeCommand { get; private set; }          // SUSPENSION & RESUME: Refreshes welcome message
        public ICommand NavigateToEditCommand { get; private set; }          // ?? STATE MANAGEMENT: Navigate to edit with state
        public ICommand NavigateToNewCustomerCommand { get; private set; }   // ?? STATE MANAGEMENT: Navigate to new customer

        private void InitializeCommands()
        {
            System.Diagnostics.Debug.WriteLine("FILE I/O: Initializing commands that will trigger file operations...");
            
            // FILE I/O: Wire up commands to methods that perform file operations
            AddCustomerCommand = new RelayCommand(AddCustomer);                    // FILE I/O: Add customer ? save to file
            EditSelectedCustomerCommand = new RelayCommand<Customer>(EditCustomer);
            DeleteSelectedCustomerCommand = new RelayCommand<Customer>(DeleteCustomer);  // FILE I/O: Delete customer ? save to file
            RefreshCommand = new RelayCommand(LoadCustomersAsync);                       // FILE I/O: Reload from file
            LoadExternalDataCommand = new RelayCommand(LoadExternalData);               // FILE I/O: Add sample data ? save to file
            ShowFileInfoCommand = new RelayCommand(ShowFileInfo);                      // FILE I/O: Show file details
            DismissWelcomeCommand = new RelayCommand(DismissWelcomeMessage);           // SUSPENSION & RESUME: Dismiss welcome message
            RefreshWelcomeCommand = new RelayCommand(RefreshWelcomeMessage);           // SUSPENSION & RESUME: Refresh welcome message
            NavigateToEditCommand = new RelayCommand<Customer>(NavigateToEditCustomer);     // ?? STATE MANAGEMENT: Navigate to edit
            NavigateToNewCustomerCommand = new RelayCommand(NavigateToNewCustomer);         // ?? STATE MANAGEMENT: Navigate to new customer
            
            System.Diagnostics.Debug.WriteLine("FILE I/O: Commands initialized - ready for file operations");
        }

        /// <summary>
        /// SUSPENSION & RESUME: Refresh welcome message
        /// Useful for testing and updating message after suspension state changes
        /// </summary>
        public void RefreshWelcomeMessage()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("SUSPENSION & RESUME: Refreshing welcome message");
                
                // SUSPENSION & RESUME: Get fresh welcome message
                WelcomeMessage = SuspensionService.Instance.GetWelcomeBackMessage();
                OnPropertyChanged(nameof(ShowWelcomeMessage));
                
                System.Diagnostics.Debug.WriteLine($"SUSPENSION & RESUME: Welcome message refreshed: '{WelcomeMessage}'");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SUSPENSION & RESUME ERROR: Failed to refresh welcome message - {ex.Message}");
            }
        }

        /// <summary>
        /// ?? STATE MANAGEMENT: Navigate to edit customer with state preservation
        /// ?? Enhanced: Use MainPage's direct navigation methods
        /// ?? Called when user clicks Edit button on customer list
        /// </summary>
        private void NavigateToEditCustomer(Customer customer)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"?? STATE MANAGEMENT: HomeViewModel navigating to edit customer: {customer?.FullName}");
                
                if (customer != null)
                {
                    // ?? NAVIGATION: Try to use MainPage's direct navigation method
                    try
                    {
                        var currentFrame = Windows.UI.Xaml.Window.Current.Content as Windows.UI.Xaml.Controls.Frame;
                        if (currentFrame?.Content is MainPage mainPage)
                        {
                            mainPage.NavigateToEditWithCustomer(customer);
                            System.Diagnostics.Debug.WriteLine("?? NAVIGATION: Successfully used MainPage.NavigateToEditWithCustomer");
                            return;
                        }
                    }
                    catch (Exception navEx)
                    {
                        System.Diagnostics.Debug.WriteLine($"?? NAVIGATION: MainPage navigation failed - {navEx.Message}");
                    }
                    
                    // ?? FALLBACK: Store state and show informative message
                    var stateService = NavigationStateService.Instance;
                    stateService.SetSelectedCustomerForEdit(customer);
                    
                    _dialogService.ShowMessageAsync("Edit Customer", 
                        $"Customer '{customer.FullName}' has been selected for editing.\n\n" +
                        $"Please navigate to 'Customer Editor' from the main menu to edit this customer.\n\n" +
                        $"The customer data has been stored and will be automatically loaded.");
                    
                    System.Diagnostics.Debug.WriteLine("?? STATE MANAGEMENT: Customer edit navigation completed with fallback");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"?? STATE MANAGEMENT ERROR: Failed to navigate to edit customer - {ex.Message}");
            }
        }

        /// <summary>
        /// ?? STATE MANAGEMENT: Navigate to new customer creation
        /// ?? Enhanced: Use MainPage's direct navigation methods
        /// ?? Called when user wants to create a new customer
        /// </summary>
        private void NavigateToNewCustomer()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("?? STATE MANAGEMENT: HomeViewModel navigating to new customer");
                
                // ?? NAVIGATION: Try to use MainPage's direct navigation method
                try
                {
                    var currentFrame = Windows.UI.Xaml.Window.Current.Content as Windows.UI.Xaml.Controls.Frame;
                    if (currentFrame?.Content is MainPage mainPage)
                    {
                        mainPage.NavigateToNewCustomer();
                        System.Diagnostics.Debug.WriteLine("?? NAVIGATION: Successfully used MainPage.NavigateToNewCustomer");
                        return;
                    }
                }
                catch (Exception navEx)
                {
                    System.Diagnostics.Debug.WriteLine($"?? NAVIGATION: MainPage navigation failed - {navEx.Message}");
                }
                
                // ?? FALLBACK: Clear state and show informative message
                var stateService = NavigationStateService.Instance;
                stateService.ClearAllState();
                
                _dialogService.ShowMessageAsync("New Customer", 
                    "Ready to create a new customer!\n\n" +
                    "Please navigate to 'Customer Editor' from the main menu to create a new customer.\n\n" +
                    "The form has been cleared and is ready for new customer data.");
                
                System.Diagnostics.Debug.WriteLine("?? STATE MANAGEMENT: New customer navigation completed with fallback");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"?? STATE MANAGEMENT ERROR: Failed to navigate to new customer - {ex.Message}");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #region Missing Methods

        /// <summary>
        /// SUSPENSION & RESUME: Initialize welcome message based on app state
        /// </summary>
        private void InitializeWelcomeMessage()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("SUSPENSION & RESUME: Initializing welcome message...");
                
                // SUSPENSION & RESUME: Always get and show welcome message
                WelcomeMessage = SuspensionService.Instance.GetWelcomeBackMessage();
                
                // SUSPENSION & RESUME: Log suspension state details
                var suspensionSummary = SuspensionService.Instance.GetSuspensionSummary();
                System.Diagnostics.Debug.WriteLine($"SUSPENSION & RESUME: {suspensionSummary}");
                
                // SUSPENSION & RESUME: Ensure UI updates
                OnPropertyChanged(nameof(ShowWelcomeMessage));
                
                System.Diagnostics.Debug.WriteLine($"SUSPENSION & RESUME: Welcome message initialized: '{WelcomeMessage}' (ShowWelcomeMessage: {ShowWelcomeMessage})");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SUSPENSION & RESUME ERROR: Failed to initialize welcome message - {ex.Message}");
                WelcomeMessage = "?? Welcome to Customer Management!";
                OnPropertyChanged(nameof(ShowWelcomeMessage));
            }
        }

        /// <summary>
        /// SUSPENSION & RESUME: Dismiss welcome message
        /// </summary>
        public void DismissWelcomeMessage()
        {
            System.Diagnostics.Debug.WriteLine("SUSPENSION & RESUME: Dismissing welcome message");
            
            // SUSPENSION & RESUME: Clear suspension flag when user dismisses message
            SuspensionService.Instance.ClearSuspensionFlag();
            
            WelcomeMessage = "";
            OnPropertyChanged(nameof(ShowWelcomeMessage));
            
            System.Diagnostics.Debug.WriteLine("SUSPENSION & RESUME: Welcome message dismissed and suspension flag cleared");
        }

        /// <summary>
        /// FILE I/O: PRIMARY LOAD OPERATION - Loads customers from file and updates UI
        /// </summary>
        private async void LoadCustomersAsync()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("FILE I/O: LoadCustomersAsync - requesting customer data from file");
                
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
                System.Diagnostics.Debug.WriteLine("FILE I/O: AddCustomer - starting add operation with file save");
                
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
                
                System.Diagnostics.Debug.WriteLine($"FILE I/O: AddCustomer completed - {newCustomer.FullName} added and saved to file");
            }
            catch (Exception ex)
            {
                // FILE I/O: Handle add/save errors with user notification
                System.Diagnostics.Debug.WriteLine($"FILE I/O ERROR: Failed to add customer and save to file - {ex.Message}");
                FileStatus = $"Error adding customer: {ex.Message}";
                
                await _dialogService.ShowMessageAsync("Error", $"Failed to add customer: {ex.Message}");
            }
        }

        private async void EditCustomer(Customer customer)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"FILE I/O: EditCustomer called for {customer?.FullName}");
                
                if (customer != null)
                {
                    // FILE I/O: Edit functionality would call UpdateCustomerAsync() which auto-saves to file
                    await _dialogService.ShowMessageAsync("Edit", $"Edit feature for {customer.FullName} coming soon!");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in EditCustomer: {ex.Message}");
            }
        }

        /// <summary>
        /// FILE I/O: DELETE CUSTOMER OPERATION - Removes customer and triggers auto-save to file
        /// </summary>
        private async void DeleteCustomer(Customer customer)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"FILE I/O: DeleteCustomer - starting delete operation for {customer?.FullName}");
                
                if (customer == null) return;

                // FILE I/O: Confirm deletion with user - emphasize file persistence
                var result = await _dialogService.ShowConfirmationAsync(
                    "Delete Customer",
                    $"Are you sure you want to delete {customer.FullName}?\n\nThis will permanently remove them from the file.",
                    "Delete",
                    "Cancel");

                if (result == Windows.UI.Xaml.Controls.ContentDialogResult.Primary)
                {
                    // FILE I/O: Update status to show delete operation is in progress
                    FileStatus = $"Deleting {customer.FullName}...";
                    
                    // FILE I/O: Delete customer from service - this triggers automatic save to file
                    await _customerService.DeleteCustomerAsync(customer.Id);  // FILE I/O: Delete + auto-save to file
                    
                    // Update UI immediately for responsive feel
                    Customers.Remove(customer);
                    FilterCustomers();
                    
                    // FILE I/O: Update status to confirm successful deletion and save
                    FileStatus = $"Deleted customer: {customer.FullName}";
                    
                    // FILE I/O: Show user confirmation that data was removed from file
                    await _dialogService.ShowMessageAsync(
                        "Customer Deleted", 
                        $"Successfully deleted {customer.FullName} and updated file!");
                    
                    System.Diagnostics.Debug.WriteLine($"FILE I/O: DeleteCustomer completed - {customer.FullName} deleted and file updated");
                }
            }
            catch (Exception ex)
            {
                // FILE I/O: Handle delete/save errors with user notification
                System.Diagnostics.Debug.WriteLine($"FILE I/O ERROR: Failed to delete customer and update file - {ex.Message}");
                FileStatus = $"Error deleting customer: {ex.Message}";
                
                await _dialogService.ShowMessageAsync("Error", $"Failed to delete customer: {ex.Message}");
            }
        }

        /// <summary>
        /// FILE I/O: BULK ADD OPERATION - Loads sample data and triggers auto-save to file
        /// </summary>
        private async void LoadExternalData()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("FILE I/O: LoadExternalData - starting bulk add operation with file save");
                
                // FILE I/O: Update status to show bulk loading is in progress
                FileStatus = "Loading sample data...";
                
                // Create sample customer data
                var sampleCustomers = new[]
                {
                    new Customer { FirstName = "Alice", LastName = "Johnson", Email = "alice@example.com", Company = "Tech Corp" },
                    new Customer { FirstName = "Bob", LastName = "Wilson", Email = "bob@example.com", Phone = "555-0124", Company = "Marketing Inc" },
                    new Customer { FirstName = "Carol", LastName = "Davis", Email = "carol@example.com", Phone = "555-0125", Company = "Design Studio" }
                };

                // FILE I/O: Add each customer - each triggers an auto-save to file
                foreach (var customer in sampleCustomers)
                {
                    await _customerService.AddCustomerAsync(customer);  // FILE I/O: Add + auto-save to file
                    System.Diagnostics.Debug.WriteLine($"FILE I/O: Added sample customer {customer.FullName} and saved to file");
                }

                // FILE I/O: Refresh UI by reloading from file to show all new data
                LoadCustomersAsync();  // FILE I/O: Reload from file to show updated data
                
                // FILE I/O: Show user confirmation that all data was saved to file
                await _dialogService.ShowMessageAsync(
                    "Sample Data Loaded", 
                    $"Added {sampleCustomers.Length} sample customers and saved to file!");
                
                System.Diagnostics.Debug.WriteLine($"FILE I/O: LoadExternalData completed - {sampleCustomers.Length} customers added and saved to file");
            }
            catch (Exception ex)
            {
                // FILE I/O: Handle bulk add/save errors with user notification
                System.Diagnostics.Debug.WriteLine($"FILE I/O ERROR: Failed to load sample data and save to file - {ex.Message}");
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
                System.Diagnostics.Debug.WriteLine("FILE I/O: ShowFileInfo - requesting file details for user display");
                
                // FILE I/O: Get file information from service
                var fileInfo = await _customerService.GetFileInfoAsync();  // FILE I/O: Query file system details
                
                // FILE I/O: Display file details to user
                await _dialogService.ShowMessageAsync("File Information", fileInfo);
                
                System.Diagnostics.Debug.WriteLine($"FILE I/O: Displayed file info to user: {fileInfo}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"FILE I/O ERROR: Failed to get file info - {ex.Message}");
                await _dialogService.ShowMessageAsync("Error", $"Failed to get file info: {ex.Message}");
            }
        }

        #endregion
    }
}