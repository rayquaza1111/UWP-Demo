using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Navigation;
using UWP_Demo.Services;
using UWP_Demo.Models;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;

namespace UWP_Demo.Views
{
    /// <summary>
    /// ?? CUSTOMER SELECTION & STATE MANAGEMENT: EditPage with comprehensive functionality
    /// ?? Features: Auto-save form data, restore customer state, navigation tracking, customer dropdown
    /// ?? Purpose: Demonstrate state management and customer selection across navigation and app sessions
    /// </summary>
    public sealed partial class EditPage : Page
    {
        // ?? CUSTOMER SELECTION & STATE MANAGEMENT: Services and state
        private NavigationStateService _stateService;
        private CustomerService _customerService;
        private DialogService _dialogService;

        // ?? CUSTOMER SELECTION: Customer list and selection management
        private ObservableCollection<Customer> _allCustomers;
        private Customer _originalCustomer;
        private bool _isFormDirty = false;
        private bool _isLoadingState = false;

        public EditPage()
        {
            this.InitializeComponent();
            
            // ?? CUSTOMER SELECTION: Initialize services and collections
            _stateService = NavigationStateService.Instance;
            _customerService = new CustomerService();
            _dialogService = new DialogService();
            _allCustomers = new ObservableCollection<Customer>();

            System.Diagnostics.Debug.WriteLine("?? CUSTOMER SELECTION: EditPage initialized with dropdown support");
        }

        /// <summary>
        /// ?? CUSTOMER SELECTION: Handle page navigation and setup customer dropdown
        /// ?? Features: Load customer list, auto-select first customer, restore state
        /// Navigation System: Called: When navigating to this page with or without parameters
        /// </summary>
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            try
            {
                System.Diagnostics.Debug.WriteLine("?? CUSTOMER SELECTION: EditPage navigated to, starting setup");
                
                // ?? CUSTOMER SELECTION: Load all customers first
                await LoadCustomersAsync();
                
                // Navigation System: Add navigation entry
                _stateService.AddNavigationEntry("EditPage", "Edit Customer", e.Parameter?.ToString() ?? "");
                
                // ?? CUSTOMER SELECTION: Check for passed customer parameter
                Customer customerFromParameter = null;
                if (e.Parameter is Customer customer)
                {
                    customerFromParameter = customer;
                    System.Diagnostics.Debug.WriteLine($"?? CUSTOMER SELECTION: Received customer parameter: {customer.FullName}");
                }

                // ?? CUSTOMER SELECTION: Setup customer selection and restore state
                await SetupCustomerSelectionAsync(customerFromParameter);
                
                // ?? STATE MANAGEMENT: Update state info display
                UpdateStateInfoDisplay();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"?? CUSTOMER SELECTION ERROR: Failed during OnNavigatedTo - {ex.Message}");
            }
        }

        /// <summary>
        /// Navigation System: Handle page navigation away (save state)
        /// Navigation System: Saves: Current form data if user has made changes
        /// Navigation System: Called: When navigating away from this page
        /// </summary>
        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("Navigation System: EditPage navigating away, saving state");
                
                // ?? STATE MANAGEMENT: Save current form state if dirty
                if (_isFormDirty && !_isLoadingState)
                {
                    SaveCurrentFormState();
                    System.Diagnostics.Debug.WriteLine("Navigation System: Form state saved due to navigation away");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Navigation System ERROR: Failed during OnNavigatingFrom - {ex.Message}");
            }

            base.OnNavigatingFrom(e);
        }

        /// <summary>
        /// ?? STATE MANAGEMENT: Restore page state from stored data
        /// ?? Enhanced: Works with customer selection and auto-binding
        /// ?? Priority: Stored form data > Customer data > Clean form
        /// </summary>
        private async Task RestorePageStateAsync(Customer customerToEdit, bool stateRestored, bool autoSelected)
        {
            try
            {
                bool hasUnsavedChanges = _stateService.HasUnsavedChanges;
                var formData = _stateService.CurrentFormData;
                bool hasStoredFormData = formData != null && !string.IsNullOrEmpty(formData.FirstName);

                if (hasStoredFormData && hasUnsavedChanges)
                {
                    // ?? STATE MANAGEMENT: Restore form data from previous session
                    PopulateFormFromData(formData);
                    stateRestored = true;
                    UnsavedChangesInfoBar.IsOpen = true;
                    System.Diagnostics.Debug.WriteLine("?? STATE MANAGEMENT: Restored unsaved form data");
                }
                else if (customerToEdit != null)
                {
                    // ?? CUSTOMER SELECTION: Populate form with customer data
                    PopulateFormFromCustomer(customerToEdit);
                    System.Diagnostics.Debug.WriteLine("?? CUSTOMER SELECTION: Populated form with customer data");
                }

                // ?? STATE MANAGEMENT: Update UI based on restoration type
                if (stateRestored && !autoSelected)
                {
                    StateInfoBar.IsOpen = true;
                }

                // ?? CUSTOMER SELECTION: Update page title based on mode
                if (customerToEdit != null)
                {
                    PageTitle.Text = $"Edit {customerToEdit.FullName}";
                    PageSubtitle.Text = autoSelected ? 
                        $"Auto-selected for editing: {customerToEdit.FullName}" : 
                        $"Modify information for {customerToEdit.FullName}";
                }
                else
                {
                    PageTitle.Text = "New Customer";
                    PageSubtitle.Text = "Create a new customer record";
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"?? STATE MANAGEMENT ERROR: Failed to restore page state - {ex.Message}");
            }
        }

        /// <summary>
        /// ?? STATE MANAGEMENT: Populate form from customer object
        /// ?? Sets: All form fields from customer data
        /// </summary>
        private void PopulateFormFromCustomer(Customer customer)
        {
            if (customer == null) return;

            FirstNameTextBox.Text = customer.FirstName ?? "";
            LastNameTextBox.Text = customer.LastName ?? "";
            EmailTextBox.Text = customer.Email ?? "";
            PhoneTextBox.Text = customer.Phone ?? "";
            CompanyTextBox.Text = customer.Company ?? "";
            
            _isFormDirty = false;
        }

        /// <summary>
        /// ?? STATE MANAGEMENT: Populate form from stored form data
        /// ?? Sets: All form fields from saved form state
        /// </summary>
        private void PopulateFormFromData(NavigationStateService.FormData formData)
        {
            if (formData == null) return;

            FirstNameTextBox.Text = formData.FirstName ?? "";
            LastNameTextBox.Text = formData.LastName ?? "";
            EmailTextBox.Text = formData.Email ?? "";
            PhoneTextBox.Text = formData.Phone ?? "";
            CompanyTextBox.Text = formData.Company ?? "";
            
            _isFormDirty = true;
        }

        /// <summary>
        /// ?? STATE MANAGEMENT: Save current form state
        /// ?? Saves: All form field values to state service
        /// </summary>
        private void SaveCurrentFormState()
        {
            try
            {
                var customerId = _originalCustomer?.Id ?? 0;
                var isEditing = _originalCustomer != null;

                _stateService.SaveFormState(
                    FirstNameTextBox.Text,
                    LastNameTextBox.Text,
                    EmailTextBox.Text,
                    PhoneTextBox.Text,
                    CompanyTextBox.Text,
                    isEditing,
                    customerId
                );

                System.Diagnostics.Debug.WriteLine("?? STATE MANAGEMENT: Current form state saved");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"?? STATE MANAGEMENT ERROR: Failed to save form state - {ex.Message}");
            }
        }

        /// <summary>
        /// ?? STATE MANAGEMENT: Handle form field changes
        /// ?? Tracks: When user modifies form data
        /// </summary>
        private void OnFormFieldChanged(object sender, TextChangedEventArgs e)
        {
            if (!_isLoadingState)
            {
                _isFormDirty = true;
                System.Diagnostics.Debug.WriteLine("?? STATE MANAGEMENT: Form marked as dirty due to user input");
                
                // ?? STATE MANAGEMENT: Auto-save form state periodically
                SaveCurrentFormState();
            }
        }

        /// <summary>
        /// ?? STATE MANAGEMENT: Handle save button click
        /// ?? Saves: Customer data and clears state
        /// </summary>
        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("?? STATE MANAGEMENT: Save button clicked");

                // Validate form
                if (!ValidateForm())
                {
                    return;
                }

                // Create or update customer
                var customer = CreateCustomerFromForm();
                
                if (_originalCustomer != null)
                {
                    // Update existing customer
                    customer.Id = _originalCustomer.Id;
                    customer.DateCreated = _originalCustomer.DateCreated;
                    // Note: LastModified is automatically updated in the Customer model
                    
                    await _customerService.UpdateCustomerAsync(customer);
                    await _dialogService.ShowMessageAsync("Success", $"Customer {customer.FullName} updated successfully!");
                }
                else
                {
                    // Add new customer
                    await _customerService.AddCustomerAsync(customer);
                    await _dialogService.ShowMessageAsync("Success", $"Customer {customer.FullName} added successfully!");
                }

                // ?? STATE MANAGEMENT: Clear state after successful save
                _stateService.MarkFormClean();
                _stateService.ClearAllState();
                _isFormDirty = false;

                // Navigate back
                NavigateBack();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"?? STATE MANAGEMENT ERROR: Failed to save customer - {ex.Message}");
                await _dialogService.ShowMessageAsync("Error", $"Failed to save customer: {ex.Message}");
            }
        }

        /// <summary>
        /// ?? STATE MANAGEMENT: Handle cancel button click
        /// ?? Shows: Unsaved changes warning if needed
        /// </summary>
        private async void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("?? STATE MANAGEMENT: Cancel button clicked");

                bool shouldNavigateBack = true;

                // ?? STATE MANAGEMENT: Warn about unsaved changes
                if (_isFormDirty)
                {
                    var result = await _dialogService.ShowConfirmationAsync(
                        "Unsaved Changes",
                        "You have unsaved changes. Your work will be saved for the next time you return.\n\nDo you want to continue?",
                        "Continue",
                        "Stay Here"
                    );

                    shouldNavigateBack = (result == Windows.UI.Xaml.Controls.ContentDialogResult.Primary);
                }

                if (shouldNavigateBack)
                {
                    // ?? STATE MANAGEMENT: Save state if dirty, but don't clear it (user might return)
                    if (_isFormDirty)
                    {
                        SaveCurrentFormState();
                    }

                    NavigateBack();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"?? STATE MANAGEMENT ERROR: Failed during cancel - {ex.Message}");
            }
        }

        /// <summary>
        /// ?? STATE MANAGEMENT: Handle reset button click
        /// ?? Restores: Original customer data
        /// </summary>
        private async void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("?? STATE MANAGEMENT: Reset button clicked");

                var result = await _dialogService.ShowConfirmationAsync(
                    "Reset Form",
                    "This will restore the original customer data and discard your changes. Continue?",
                    "Reset",
                    "Cancel"
                );

                if (result == Windows.UI.Xaml.Controls.ContentDialogResult.Primary)
                {
                    // ?? STATE MANAGEMENT: Reset to original customer data
                    if (_originalCustomer != null)
                    {
                        PopulateFormFromCustomer(_originalCustomer);
                    }
                    else
                    {
                        // Clear form for new customer
                        FirstNameTextBox.Text = "";
                        LastNameTextBox.Text = "";
                        EmailTextBox.Text = "";
                        PhoneTextBox.Text = "";
                        CompanyTextBox.Text = "";
                    }

                    _isFormDirty = false;
                    ValidationMessage.Visibility = Visibility.Collapsed;
                    
                    System.Diagnostics.Debug.WriteLine("?? STATE MANAGEMENT: Form reset to original values");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"?? STATE MANAGEMENT ERROR: Failed during reset - {ex.Message}");
            }
        }

        /// <summary>
        /// ?? STATE MANAGEMENT: Refresh state info display
        /// ?? Updates: State information panel
        /// </summary>
        private void RefreshStateButton_Click(object sender, RoutedEventArgs e)
        {
            UpdateStateInfoDisplay();
        }

        /// <summary>
        /// ?? STATE MANAGEMENT: Clear all state
        /// ?? Clears: All stored state and resets form
        /// </summary>
        private async void ClearStateButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var result = await _dialogService.ShowConfirmationAsync(
                    "Clear State",
                    "This will clear all stored state and reset the form. Continue?",
                    "Clear",
                    "Cancel"
                );

                if (result == Windows.UI.Xaml.Controls.ContentDialogResult.Primary)
                {
                    _stateService.ClearAllState();
                    _isFormDirty = false;
                    
                    // Reset form
                    FirstNameTextBox.Text = "";
                    LastNameTextBox.Text = "";
                    EmailTextBox.Text = "";
                    PhoneTextBox.Text = "";
                    CompanyTextBox.Text = "";
                    
                    UpdateStateInfoDisplay();
                    
                    System.Diagnostics.Debug.WriteLine("?? STATE MANAGEMENT: All state cleared");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"?? STATE MANAGEMENT ERROR: Failed to clear state - {ex.Message}");
            }
        }

        /// <summary>
        /// ?? STATE MANAGEMENT: Update state information display
        /// ?? Shows: Current state details for debugging
        /// </summary>
        private void UpdateStateInfoDisplay()
        {
            try
            {
                var stateSummary = _stateService.GetStateSummary();
                var selectedCustomer = _stateService.SelectedCustomer;
                var formData = _stateService.CurrentFormData;
                
                var detailedInfo = $"State Summary: {stateSummary}\n\n";
                
                if (selectedCustomer != null)
                {
                    detailedInfo += $"Selected Customer: {selectedCustomer.FullName} (ID: {selectedCustomer.Id})\n";
                    detailedInfo += $"Email: {selectedCustomer.Email}\n";
                    detailedInfo += $"Created: {selectedCustomer.DateCreated:MM/dd/yyyy}\n\n";
                }
                
                if (formData != null)
                {
                    detailedInfo += $"Form Data: {formData.FirstName} {formData.LastName}\n";
                    detailedInfo += $"Editing: {formData.IsEditing}, Customer ID: {formData.CustomerId}\n";
                    detailedInfo += $"Saved At: {formData.SavedAt:MM/dd/yyyy HH:mm:ss}\n\n";
                }
                
                detailedInfo += $"Form Dirty: {_isFormDirty}\n";
                detailedInfo += $"Has Unsaved Changes: {_stateService.HasUnsavedChanges}";
                
                StateInfoDisplay.Text = detailedInfo;
                
                System.Diagnostics.Debug.WriteLine("?? STATE MANAGEMENT: State info display updated");
            }
            catch (Exception ex)
            {
                StateInfoDisplay.Text = $"Error loading state info: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"?? STATE MANAGEMENT ERROR: Failed to update state display - {ex.Message}");
            }
        }

        /// <summary>
        /// Validate form input
        /// </summary>
        private bool ValidateForm()
        {
            if (string.IsNullOrWhiteSpace(FirstNameTextBox.Text) ||
                string.IsNullOrWhiteSpace(LastNameTextBox.Text) ||
                string.IsNullOrWhiteSpace(EmailTextBox.Text))
            {
                ValidationMessage.Text = "First Name, Last Name, and Email are required fields.";
                ValidationMessage.Visibility = Visibility.Visible;
                return false;
            }

            ValidationMessage.Visibility = Visibility.Collapsed;
            return true;
        }

        /// <summary>
        /// Create customer object from form data
        /// </summary>
        private Customer CreateCustomerFromForm()
        {
            return new Customer
            {
                FirstName = FirstNameTextBox.Text.Trim(),
                LastName = LastNameTextBox.Text.Trim(),
                Email = EmailTextBox.Text.Trim(),
                Phone = PhoneTextBox.Text.Trim(),
                Company = CompanyTextBox.Text.Trim()
            };
        }

        /// <summary>
        /// Navigation System: Navigate back to previous page
        /// </summary>
        private void NavigateBack()
        {
            try
            {
                if (Frame.CanGoBack)
                {
                    Frame.GoBack(); // Navigation System: Use frame's back navigation
                }
                else
                {
                    Frame.Navigate(typeof(HomePage)); // Navigation System: Fallback to HomePage
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Navigation System ERROR: Failed to navigate back - {ex.Message}");
            }
        }

        /// <summary>
        /// ?? CUSTOMER SELECTION: Load all customers and populate dropdown
        /// ?? Loads: Customer list from service and populates ComboBox
        /// </summary>
        private async Task LoadCustomersAsync()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("?? CUSTOMER SELECTION: Loading customers for dropdown");
                
                // Load customers from service
                var customers = await _customerService.GetCustomersAsync();
                
                // Clear and populate the collection
                _allCustomers.Clear();
                foreach (var customer in customers)
                {
                    _allCustomers.Add(customer);
                }
                
                // Bind to ComboBox
                CustomerSelectionComboBox.ItemsSource = _allCustomers;
                
                System.Diagnostics.Debug.WriteLine($"?? CUSTOMER SELECTION: Loaded {_allCustomers.Count} customers");
                
                // Update info bar
                if (_allCustomers.Count == 0)
                {
                    CustomerCountInfoBar.Title = "No Customers Found";
                    CustomerCountInfoBar.Message = "No customers in database. Create a new customer to get started.";
                    CustomerCountInfoBar.Severity = Microsoft.UI.Xaml.Controls.InfoBarSeverity.Warning;
                    CustomerCountInfoBar.IsOpen = true;
                }
                else
                {
                    CustomerCountInfoBar.Title = "Customer Database";
                    CustomerCountInfoBar.Message = $"Found {_allCustomers.Count} customer(s) in database";
                    CustomerCountInfoBar.Severity = Microsoft.UI.Xaml.Controls.InfoBarSeverity.Informational;
                    CustomerCountInfoBar.IsOpen = true;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"?? CUSTOMER SELECTION ERROR: Failed to load customers - {ex.Message}");
                CustomerCountInfoBar.Title = "Error Loading Customers";
                CustomerCountInfoBar.Message = $"Failed to load customer list: {ex.Message}";
                CustomerCountInfoBar.Severity = Microsoft.UI.Xaml.Controls.InfoBarSeverity.Error;
                CustomerCountInfoBar.IsOpen = true;
            }
        }

        /// <summary>
        /// ?? CUSTOMER SELECTION: Setup customer selection with auto-binding
        /// ?? Priority: Parameter customer > Stored customer > First customer > New customer
        /// </summary>
        private async Task SetupCustomerSelectionAsync(Customer parameterCustomer = null)
        {
            try
            {
                _isLoadingState = true;
                
                Customer customerToEdit = null;
                bool stateRestored = false;
                bool autoSelected = false;

                // ?? CUSTOMER SELECTION: Priority 1 - Use parameter customer if provided
                if (parameterCustomer != null)
                {
                    customerToEdit = parameterCustomer;
                    _stateService.SetSelectedCustomerForEdit(customerToEdit);
                    System.Diagnostics.Debug.WriteLine("?? CUSTOMER SELECTION: Using customer from navigation parameter");
                }
                // ?? STATE MANAGEMENT: Priority 2 - Use stored selected customer
                else if (_stateService.SelectedCustomer != null)
                {
                    customerToEdit = _stateService.SelectedCustomer;
                    stateRestored = true;
                    System.Diagnostics.Debug.WriteLine("?? STATE MANAGEMENT: Restored customer from stored state");
                }
                // ?? CUSTOMER SELECTION: Priority 3 - Auto-select first customer
                else if (_allCustomers.Count > 0)
                {
                    customerToEdit = _allCustomers.First();
                    autoSelected = true;
                    _stateService.SetSelectedCustomerForEdit(customerToEdit);
                    System.Diagnostics.Debug.WriteLine($"?? CUSTOMER SELECTION: Auto-selected first customer: {customerToEdit.FullName}");
                    
                    CustomerCountInfoBar.Title = "Auto-Selected Customer";
                    CustomerCountInfoBar.Message = $"Automatically selected '{customerToEdit.FullName}' for editing. Use dropdown to switch.";
                    CustomerCountInfoBar.Severity = Microsoft.UI.Xaml.Controls.InfoBarSeverity.Success;
                    CustomerCountInfoBar.IsOpen = true;
                }
                // ?? CUSTOMER SELECTION: Priority 4 - No customers found, offer to load sample data
                else
                {
                    System.Diagnostics.Debug.WriteLine("?? CUSTOMER SELECTION: No customers found, switching to new customer mode");
                    
                    CustomerCountInfoBar.Title = "No Customers Found";
                    CustomerCountInfoBar.Message = "No customers in database. Click 'Load Sample Data' or create a new customer.";
                    CustomerCountInfoBar.Severity = Microsoft.UI.Xaml.Controls.InfoBarSeverity.Warning;
                    CustomerCountInfoBar.IsOpen = true;
                    
                    // Show sample data dialog
                    await ShowSampleDataOfferAsync();
                }

                // Set ComboBox selection
                if (customerToEdit != null)
                {
                    CustomerSelectionComboBox.SelectedItem = customerToEdit;
                }

                // Store original customer for reset functionality
                _originalCustomer = customerToEdit;

                // ?? STATE MANAGEMENT: Check for stored form data and populate form
                await RestorePageStateAsync(customerToEdit, stateRestored, autoSelected);

                _isLoadingState = false;
            }
            catch (Exception ex)
            {
                _isLoadingState = false;
                System.Diagnostics.Debug.WriteLine($"?? CUSTOMER SELECTION ERROR: Failed to setup customer selection - {ex.Message}");
            }
        }

        /// <summary>
        /// ?? CUSTOMER SELECTION: Handle customer selection change from dropdown
        /// ?? Updates: Form with selected customer data
        /// </summary>
        private void CustomerSelectionComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isLoadingState) return;

            try
            {
                var selectedCustomer = CustomerSelectionComboBox.SelectedItem as Customer;
                if (selectedCustomer != null)
                {
                    System.Diagnostics.Debug.WriteLine($"?? CUSTOMER SELECTION: User selected customer: {selectedCustomer.FullName}");
                    
                    // Check for unsaved changes
                    if (_isFormDirty)
                    {
                        ShowUnsavedChangesWarning(selectedCustomer);
                    }
                    else
                    {
                        LoadSelectedCustomer(selectedCustomer);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"?? CUSTOMER SELECTION ERROR: Failed to handle selection change - {ex.Message}");
            }
        }

        /// <summary>
        /// ?? CUSTOMER SELECTION: Load selected customer into form
        /// </summary>
        private void LoadSelectedCustomer(Customer customer)
        {
            try
            {
                _originalCustomer = customer;
                _stateService.SetSelectedCustomerForEdit(customer);
                
                // Update form
                PopulateFormFromCustomer(customer);
                
                // Update page title
                PageTitle.Text = $"Edit {customer.FullName}";
                PageSubtitle.Text = $"Modify information for {customer.FullName}";
                
                // Clear any unsaved changes flag
                _isFormDirty = false;
                
                System.Diagnostics.Debug.WriteLine($"?? CUSTOMER SELECTION: Loaded customer {customer.FullName} into form");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"?? CUSTOMER SELECTION ERROR: Failed to load selected customer - {ex.Message}");
            }
        }

        /// <summary>
        /// ?? CUSTOMER SELECTION: Show warning about unsaved changes when switching customers
        /// </summary>
        private async void ShowUnsavedChangesWarning(Customer newCustomer)
        {
            try
            {
                var result = await _dialogService.ShowConfirmationAsync(
                    "Unsaved Changes",
                    "You have unsaved changes. Do you want to save them before switching to another customer?",
                    "Save & Switch",
                    "Discard & Switch"
                );

                if (result == Windows.UI.Xaml.Controls.ContentDialogResult.Primary)
                {
                    // Save current changes first
                    await SaveCurrentCustomer();
                }
                
                // Switch to new customer
                LoadSelectedCustomer(newCustomer);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"?? CUSTOMER SELECTION ERROR: Failed to handle unsaved changes warning - {ex.Message}");
            }
        }

        /// <summary>
        /// ?? CUSTOMER SELECTION: Save current customer changes
        /// </summary>
        private async Task SaveCurrentCustomer()
        {
            try
            {
                if (_originalCustomer != null && ValidateForm())
                {
                    var customer = CreateCustomerFromForm();
                    customer.Id = _originalCustomer.Id;
                    customer.DateCreated = _originalCustomer.DateCreated;
                    
                    await _customerService.UpdateCustomerAsync(customer);
                    
                    // Update the customer in the list
                    var index = _allCustomers.IndexOf(_originalCustomer);
                    if (index >= 0)
                    {
                        _allCustomers[index] = customer;
                    }
                    
                    _isFormDirty = false;
                    System.Diagnostics.Debug.WriteLine($"?? CUSTOMER SELECTION: Saved changes for {customer.FullName}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"?? CUSTOMER SELECTION ERROR: Failed to save current customer - {ex.Message}");
                throw; // Re-throw to handle in calling method
            }
        }

        /// <summary>
        /// ?? CUSTOMER SELECTION: Handle refresh customers button
        /// </summary>
        private async void RefreshCustomersButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Store currently selected customer
                var currentSelectedCustomer = CustomerSelectionComboBox.SelectedItem as Customer;
                
                // Reload customers
                await LoadCustomersAsync();
                
                // Try to restore selection
                if (currentSelectedCustomer != null)
                {
                    var matchingCustomer = _allCustomers.FirstOrDefault(c => c.Id == currentSelectedCustomer.Id);
                    if (matchingCustomer != null)
                    {
                        CustomerSelectionComboBox.SelectedItem = matchingCustomer;
                    }
                    else if (_allCustomers.Count > 0)
                    {
                        // If previously selected customer no longer exists, select first available
                        CustomerSelectionComboBox.SelectedItem = _allCustomers.First();
                        LoadSelectedCustomer(_allCustomers.First());
                    }
                }
                
                System.Diagnostics.Debug.WriteLine("?? CUSTOMER SELECTION: Customer list refreshed");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"?? CUSTOMER SELECTION ERROR: Failed to refresh customers - {ex.Message}");
            }
        }

        /// <summary>
        /// ?? CUSTOMER SELECTION: Handle add new customer button
        /// </summary>
        private void AddNewCustomerButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Clear selection and form for new customer
                CustomerSelectionComboBox.SelectedItem = null;
                _originalCustomer = null;
                
                // Clear form
                FirstNameTextBox.Text = "";
                LastNameTextBox.Text = "";
                EmailTextBox.Text = "";
                PhoneTextBox.Text = "";
                CompanyTextBox.Text = "";
                
                // Update UI
                PageTitle.Text = "New Customer";
                PageSubtitle.Text = "Create a new customer record";
                
                _isFormDirty = false;
                
                System.Diagnostics.Debug.WriteLine("?? CUSTOMER SELECTION: Switched to new customer mode");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"?? CUSTOMER SELECTION ERROR: Failed to switch to new customer mode - {ex.Message}");
            }
        }

        /// <summary>
        /// ?? CUSTOMER SELECTION: Offer to load sample data when no customers exist
        /// </summary>
        private async Task ShowSampleDataOfferAsync()
        {
            try
            {
                var result = await _dialogService.ShowConfirmationAsync(
                    "No Customers Found",
                    "Would you like to load some sample customers to get started? This will add 3 demo customers that you can edit and delete.",
                    "Load Sample Data",
                    "Create New Customer"
                );

                if (result == Windows.UI.Xaml.Controls.ContentDialogResult.Primary)
                {
                    // Load sample data
                    await LoadSampleDataAsync();
                    
                    // Refresh the customer list
                    await LoadCustomersAsync();
                    
                    // Auto-select first customer after loading sample data
                    if (_allCustomers.Count > 0)
                    {
                        var firstCustomer = _allCustomers.First();
                        CustomerSelectionComboBox.SelectedItem = firstCustomer;
                        LoadSelectedCustomer(firstCustomer);
                        
                        CustomerCountInfoBar.Title = "Sample Data Loaded";
                        CustomerCountInfoBar.Message = $"Loaded sample customers. Now editing: {firstCustomer.FullName}";
                        CustomerCountInfoBar.Severity = Microsoft.UI.Xaml.Controls.InfoBarSeverity.Success;
                        CustomerCountInfoBar.IsOpen = true;
                    }
                }
                else
                {
                    // Switch to new customer mode
                    AddNewCustomerButton_Click(null, null);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"?? CUSTOMER SELECTION ERROR: Failed to show sample data offer - {ex.Message}");
            }
        }

        /// <summary>
        /// ?? CUSTOMER SELECTION: Load sample customer data
        /// </summary>
        private async Task LoadSampleDataAsync()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("?? CUSTOMER SELECTION: Loading sample customer data");
                
                // Create sample customer data
                var sampleCustomers = new[]
                {
                    new Customer { FirstName = "Alice", LastName = "Johnson", Email = "alice.johnson@example.com", Phone = "555-0123", Company = "Tech Solutions Inc." },
                    new Customer { FirstName = "Bob", LastName = "Wilson", Email = "bob.wilson@example.com", Phone = "555-0124", Company = "Marketing Pros LLC" },
                    new Customer { FirstName = "Carol", LastName = "Davis", Email = "carol.davis@example.com", Phone = "555-0125", Company = "Design Studio Co." }
                };

                // Add each customer
                foreach (var customer in sampleCustomers)
                {
                    await _customerService.AddCustomerAsync(customer);
                    System.Diagnostics.Debug.WriteLine($"?? CUSTOMER SELECTION: Added sample customer {customer.FullName}");
                }
                
                System.Diagnostics.Debug.WriteLine($"?? CUSTOMER SELECTION: Successfully loaded {sampleCustomers.Length} sample customers");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"?? CUSTOMER SELECTION ERROR: Failed to load sample data - {ex.Message}");
                throw;
            }
        }
    }
}