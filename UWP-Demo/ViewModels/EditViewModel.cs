using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Input;
using UWP_Demo.Models;
using UWP_Demo.Services;
using UWP_Demo.Helpers;

namespace UWP_Demo.ViewModels
{
    /// <summary>
    /// ViewModel for the Edit page that manages customer creation and editing operations.
    /// This ViewModel demonstrates MVVM patterns for form handling, validation, and
    /// data persistence in a UWP application.
    /// </summary>
    /// <remarks>
    /// This ViewModel showcases several important MVVM and UWP concepts:
    /// - Property binding for form controls
    /// - Data validation and error handling
    /// - Command pattern for form actions (Save, Cancel, etc.)
    /// - Navigation parameter handling
    /// - Async operations with progress indication
    /// - Two-way data binding with change tracking
    /// - Service integration for data persistence
    /// - State management during edit operations
    /// 
    /// The EditViewModel serves as the data context for the Edit page and handles
    /// all customer creation and modification operations, including validation,
    /// saving, and navigation coordination.
    /// </remarks>
    public class EditViewModel : BaseViewModel
    {
        #region Private Fields

        /// <summary>
        /// Reference to the data service for customer operations.
        /// </summary>
        private readonly DataService _dataService;

        /// <summary>
        /// Reference to the settings service for state management.
        /// </summary>
        private readonly SettingsService _settingsService;

        /// <summary>
        /// The original customer being edited (null for new customers).
        /// </summary>
        private Customer _originalCustomer;

        /// <summary>
        /// Working copy of the customer being edited.
        /// </summary>
        private Customer _editingCustomer;

        /// <summary>
        /// Flag indicating whether this is a new customer or an existing one.
        /// </summary>
        private bool _isNewCustomer;

        /// <summary>
        /// Flag indicating whether the customer data has been modified.
        /// </summary>
        private bool _hasUnsavedChanges;

        /// <summary>
        /// Flag indicating whether a save operation is in progress.
        /// </summary>
        private bool _isSaving;

        /// <summary>
        /// Validation error message for the first name field.
        /// </summary>
        private string _firstNameError;

        /// <summary>
        /// Validation error message for the last name field.
        /// </summary>
        private string _lastNameError;

        /// <summary>
        /// Validation error message for the email field.
        /// </summary>
        private string _emailError;

        /// <summary>
        /// Validation error message for the phone field.
        /// </summary>
        private string _phoneError;

        /// <summary>
        /// Success message to display after successful operations.
        /// </summary>
        private string _successMessage;

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the customer being edited.
        /// This property provides access to all customer fields for data binding.
        /// </summary>
        /// <remarks>
        /// This property is the main data context for form controls. Changes to
        /// customer properties automatically trigger change tracking and validation.
        /// </remarks>
        public Customer EditingCustomer
        {
            get => _editingCustomer;
            set
            {
                if (SetProperty(ref _editingCustomer, value))
                {
                    // Subscribe to property changes for change tracking
                    if (_editingCustomer != null)
                    {
                        _editingCustomer.PropertyChanged += EditingCustomer_PropertyChanged;
                    }
                    
                    OnPropertyChanged(nameof(PageTitle));
                    OnPropertyChanged(nameof(PageSubtitle));
                }
            }
        }

        /// <summary>
        /// Gets whether this is a new customer being created.
        /// This property affects UI elements and validation behavior.
        /// </summary>
        public bool IsNewCustomer
        {
            get => _isNewCustomer;
            private set
            {
                if (SetProperty(ref _isNewCustomer, value))
                {
                    OnPropertyChanged(nameof(PageTitle));
                    OnPropertyChanged(nameof(PageSubtitle));
                    OnPropertyChanged(nameof(SaveButtonText));
                }
            }
        }

        /// <summary>
        /// Gets whether the customer data has unsaved changes.
        /// This property is used to show save indicators and confirm navigation.
        /// </summary>
        public bool HasUnsavedChanges
        {
            get => _hasUnsavedChanges;
            private set
            {
                if (SetProperty(ref _hasUnsavedChanges, value))
                {
                    OnPropertyChanged(nameof(PageSubtitle));
                }
            }
        }

        /// <summary>
        /// Gets whether a save operation is currently in progress.
        /// This property is used to show progress indicators and disable controls.
        /// </summary>
        public bool IsSaving
        {
            get => _isSaving;
            set => SetProperty(ref _isSaving, value);
        }

        /// <summary>
        /// Gets the dynamic page title based on the current operation.
        /// </summary>
        public string PageTitle => IsNewCustomer ? "Add New Customer" : $"Edit {EditingCustomer?.FullName ?? "Customer"}";

        /// <summary>
        /// Gets the dynamic page subtitle with status information.
        /// </summary>
        public string PageSubtitle
        {
            get
            {
                if (HasUnsavedChanges)
                    return "You have unsaved changes";
                if (IsNewCustomer)
                    return "Enter customer information";
                return "Modify customer information";
            }
        }

        /// <summary>
        /// Gets the dynamic save button text based on the current operation.
        /// </summary>
        public string SaveButtonText => IsNewCustomer ? "Add Customer" : "Save Changes";

        /// <summary>
        /// Gets whether the form data is valid and can be saved.
        /// This property checks all validation rules and field requirements.
        /// </summary>
        public bool IsValid => EditingCustomer?.IsValid() == true && !HasValidationErrors;

        /// <summary>
        /// Gets whether there are any validation errors in the form.
        /// </summary>
        public bool HasValidationErrors => 
            !string.IsNullOrEmpty(FirstNameError) ||
            !string.IsNullOrEmpty(LastNameError) ||
            !string.IsNullOrEmpty(EmailError) ||
            !string.IsNullOrEmpty(PhoneError);

        #endregion

        #region Validation Properties

        /// <summary>
        /// Gets or sets the validation error message for the first name field.
        /// </summary>
        public string FirstNameError
        {
            get => _firstNameError;
            set
            {
                if (SetProperty(ref _firstNameError, value))
                {
                    OnPropertyChanged(nameof(HasValidationErrors));
                    OnPropertyChanged(nameof(IsValid));
                }
            }
        }

        /// <summary>
        /// Gets or sets the validation error message for the last name field.
        /// </summary>
        public string LastNameError
        {
            get => _lastNameError;
            set
            {
                if (SetProperty(ref _lastNameError, value))
                {
                    OnPropertyChanged(nameof(HasValidationErrors));
                    OnPropertyChanged(nameof(IsValid));
                }
            }
        }

        /// <summary>
        /// Gets or sets the validation error message for the email field.
        /// </summary>
        public string EmailError
        {
            get => _emailError;
            set
            {
                if (SetProperty(ref _emailError, value))
                {
                    OnPropertyChanged(nameof(HasValidationErrors));
                    OnPropertyChanged(nameof(IsValid));
                }
            }
        }

        /// <summary>
        /// Gets or sets the validation error message for the phone field.
        /// </summary>
        public string PhoneError
        {
            get => _phoneError;
            set
            {
                if (SetProperty(ref _phoneError, value))
                {
                    OnPropertyChanged(nameof(HasValidationErrors));
                    OnPropertyChanged(nameof(IsValid));
                }
            }
        }

        /// <summary>
        /// Gets or sets the success message to display after successful operations.
        /// </summary>
        public string SuccessMessage
        {
            get => _successMessage;
            set => SetProperty(ref _successMessage, value);
        }

        #endregion

        #region Commands

        /// <summary>
        /// Command to save the current customer data.
        /// This command validates the data and persists it to storage.
        /// </summary>
        public ICommand SaveCommand { get; private set; }

        /// <summary>
        /// Command to cancel the edit operation and navigate back.
        /// This command checks for unsaved changes and may show a confirmation dialog.
        /// </summary>
        public ICommand CancelCommand { get; private set; }

        /// <summary>
        /// Command to reset the form to its original state.
        /// This command discards all changes and restores original values.
        /// </summary>
        public ICommand ResetCommand { get; private set; }

        /// <summary>
        /// Command to clear all form fields.
        /// This command creates a new empty customer for data entry.
        /// </summary>
        public ICommand ClearCommand { get; private set; }

        /// <summary>
        /// Command to validate a specific field.
        /// This command is used for real-time validation during user input.
        /// </summary>
        public ICommand ValidateFieldCommand { get; private set; }

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the EditViewModel class.
        /// Sets up service references, initializes commands, and prepares for editing.
        /// </summary>
        public EditViewModel()
        {
            // Get service references
            _dataService = DataService.Instance;
            _settingsService = SettingsService.Instance;

            // Initialize properties
            Title = "Edit Customer";
            IsSaving = false;
            HasUnsavedChanges = false;

            // Initialize commands
            InitializeCommands();

            Debug.WriteLine("EditViewModel: Initialized successfully");
        }

        #endregion

        #region Initialization

        /// <summary>
        /// Initializes the ViewModel for editing a specific customer or creating a new one.
        /// This method should be called when the page is navigated to with customer data.
        /// </summary>
        /// <param name="customer">The customer to edit, or null to create a new customer</param>
        public void Initialize(Customer customer)
        {
            try
            {
                ClearError();
                ClearValidationErrors();
                
                if (customer == null)
                {
                    // Create new customer
                    InitializeForNewCustomer();
                }
                else
                {
                    // Edit existing customer
                    InitializeForExistingCustomer(customer);
                }

                // Reset change tracking
                HasUnsavedChanges = false;
                
                Debug.WriteLine($"EditViewModel: Initialized for {(IsNewCustomer ? "new" : "existing")} customer");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"EditViewModel: Error during initialization: {ex.Message}");
                SetError(ex, "Unable to initialize the edit form. Please try again.");
            }
        }

        /// <summary>
        /// Initializes the ViewModel for creating a new customer.
        /// </summary>
        private void InitializeForNewCustomer()
        {
            _originalCustomer = null;
            IsNewCustomer = true;
            
            // Create a new customer with default values
            EditingCustomer = new Customer();
            
            Debug.WriteLine("EditViewModel: Initialized for new customer creation");
        }

        /// <summary>
        /// Initializes the ViewModel for editing an existing customer.
        /// </summary>
        /// <param name="customer">The customer to edit</param>
        private void InitializeForExistingCustomer(Customer customer)
        {
            _originalCustomer = customer;
            IsNewCustomer = false;
            
            // Create a working copy of the customer
            EditingCustomer = new Customer
            {
                Id = customer.Id,
                FirstName = customer.FirstName,
                LastName = customer.LastName,
                Email = customer.Email,
                Phone = customer.Phone,
                Company = customer.Company,
                DateCreated = customer.DateCreated,
                LastModified = customer.LastModified
            };
            
            Debug.WriteLine($"EditViewModel: Initialized for editing customer {customer.FullName}");
        }

        /// <summary>
        /// Creates and configures all command objects used by this ViewModel.
        /// </summary>
        private void InitializeCommands()
        {
            // Save command - available when form is valid and not busy
            SaveCommand = new RelayCommand(
                async () => await SaveCustomerAsync(),
                () => IsValid && !IsBusy && !IsSaving);

            // Cancel command - always available unless saving
            CancelCommand = new RelayCommand(
                () => CancelEdit(),
                () => !IsSaving);

            // Reset command - available when editing existing customer and not busy
            ResetCommand = new RelayCommand(
                () => ResetToOriginal(),
                () => !IsNewCustomer && !IsBusy && !IsSaving);

            // Clear command - available when not busy
            ClearCommand = new RelayCommand(
                () => ClearForm(),
                () => !IsBusy && !IsSaving);

            // Validate field command - for real-time validation
            ValidateFieldCommand = new RelayCommand<string>(
                fieldName => ValidateField(fieldName));
        }

        #endregion

        #region Save Operations

        /// <summary>
        /// Saves the current customer data to storage.
        /// This method performs validation, saves the data, and provides user feedback.
        /// </summary>
        /// <returns>A task representing the asynchronous save operation</returns>
        private async Task SaveCustomerAsync()
        {
            try
            {
                IsSaving = true;
                IsBusy = true;
                ClearError();
                ClearSuccessMessage();

                Debug.WriteLine($"EditViewModel: Starting save operation for {(IsNewCustomer ? "new" : "existing")} customer");

                // Perform final validation
                if (!ValidateAllFields())
                {
                    SetError("Please correct the validation errors before saving.");
                    return;
                }

                // Save the customer
                if (IsNewCustomer)
                {
                    await _dataService.AddCustomerAsync(EditingCustomer);
                    SuccessMessage = $"Customer '{EditingCustomer.FullName}' has been added successfully.";
                    Debug.WriteLine($"EditViewModel: Successfully added new customer {EditingCustomer.FullName}");
                }
                else
                {
                    await _dataService.UpdateCustomerAsync(EditingCustomer);
                    SuccessMessage = $"Customer '{EditingCustomer.FullName}' has been updated successfully.";
                    Debug.WriteLine($"EditViewModel: Successfully updated customer {EditingCustomer.FullName}");
                }

                // Reset change tracking
                HasUnsavedChanges = false;

                // Navigate back after a brief delay to show success message
                await Task.Delay(1500);
                NavigateBack();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"EditViewModel: Error saving customer: {ex.Message}");
                SetError(ex, "Unable to save customer data. Please check your information and try again.");
            }
            finally
            {
                IsSaving = false;
                IsBusy = false;
                RaiseCanExecuteChanged();
            }
        }

        #endregion

        #region Validation

        /// <summary>
        /// Validates all form fields and updates error properties.
        /// </summary>
        /// <returns>True if all fields are valid, false otherwise</returns>
        private bool ValidateAllFields()
        {
            try
            {
                bool isValid = true;

                // Validate first name
                if (string.IsNullOrWhiteSpace(EditingCustomer.FirstName))
                {
                    FirstNameError = "First name is required.";
                    isValid = false;
                }
                else if (EditingCustomer.FirstName.Length > 50)
                {
                    FirstNameError = "First name must be 50 characters or less.";
                    isValid = false;
                }
                else
                {
                    FirstNameError = string.Empty;
                }

                // Validate last name
                if (string.IsNullOrWhiteSpace(EditingCustomer.LastName))
                {
                    LastNameError = "Last name is required.";
                    isValid = false;
                }
                else if (EditingCustomer.LastName.Length > 50)
                {
                    LastNameError = "Last name must be 50 characters or less.";
                    isValid = false;
                }
                else
                {
                    LastNameError = string.Empty;
                }

                // Validate email
                if (string.IsNullOrWhiteSpace(EditingCustomer.Email))
                {
                    EmailError = "Email address is required.";
                    isValid = false;
                }
                else if (!IsValidEmail(EditingCustomer.Email))
                {
                    EmailError = "Please enter a valid email address.";
                    isValid = false;
                }
                else if (EditingCustomer.Email.Length > 100)
                {
                    EmailError = "Email address must be 100 characters or less.";
                    isValid = false;
                }
                else if (IsEmailInUse(EditingCustomer.Email))
                {
                    EmailError = "This email address is already in use by another customer.";
                    isValid = false;
                }
                else
                {
                    EmailError = string.Empty;
                }

                // Validate phone (optional but must be valid if provided)
                if (!string.IsNullOrWhiteSpace(EditingCustomer.Phone))
                {
                    if (!IsValidPhone(EditingCustomer.Phone))
                    {
                        PhoneError = "Please enter a valid phone number.";
                        isValid = false;
                    }
                    else if (EditingCustomer.Phone.Length > 20)
                    {
                        PhoneError = "Phone number must be 20 characters or less.";
                        isValid = false;
                    }
                    else
                    {
                        PhoneError = string.Empty;
                    }
                }
                else
                {
                    PhoneError = string.Empty;
                }

                Debug.WriteLine($"EditViewModel: Validation result: {isValid}");
                return isValid;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"EditViewModel: Error during validation: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Validates a specific field based on its name.
        /// This method is used for real-time validation during user input.
        /// </summary>
        /// <param name="fieldName">The name of the field to validate</param>
        private void ValidateField(string fieldName)
        {
            try
            {
                switch (fieldName?.ToLowerInvariant())
                {
                    case "firstname":
                        ValidateFirstName();
                        break;
                    case "lastname":
                        ValidateLastName();
                        break;
                    case "email":
                        ValidateEmail();
                        break;
                    case "phone":
                        ValidatePhone();
                        break;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"EditViewModel: Error validating field {fieldName}: {ex.Message}");
            }
        }

        /// <summary>
        /// Validates the first name field.
        /// </summary>
        private void ValidateFirstName()
        {
            if (string.IsNullOrWhiteSpace(EditingCustomer.FirstName))
            {
                FirstNameError = "First name is required.";
            }
            else if (EditingCustomer.FirstName.Length > 50)
            {
                FirstNameError = "First name must be 50 characters or less.";
            }
            else
            {
                FirstNameError = string.Empty;
            }
        }

        /// <summary>
        /// Validates the last name field.
        /// </summary>
        private void ValidateLastName()
        {
            if (string.IsNullOrWhiteSpace(EditingCustomer.LastName))
            {
                LastNameError = "Last name is required.";
            }
            else if (EditingCustomer.LastName.Length > 50)
            {
                LastNameError = "Last name must be 50 characters or less.";
            }
            else
            {
                LastNameError = string.Empty;
            }
        }

        /// <summary>
        /// Validates the email field.
        /// </summary>
        private void ValidateEmail()
        {
            if (string.IsNullOrWhiteSpace(EditingCustomer.Email))
            {
                EmailError = "Email address is required.";
            }
            else if (!IsValidEmail(EditingCustomer.Email))
            {
                EmailError = "Please enter a valid email address.";
            }
            else if (EditingCustomer.Email.Length > 100)
            {
                EmailError = "Email address must be 100 characters or less.";
            }
            else if (IsEmailInUse(EditingCustomer.Email))
            {
                EmailError = "This email address is already in use by another customer.";
            }
            else
            {
                EmailError = string.Empty;
            }
        }

        /// <summary>
        /// Validates the phone field.
        /// </summary>
        private void ValidatePhone()
        {
            if (!string.IsNullOrWhiteSpace(EditingCustomer.Phone))
            {
                if (!IsValidPhone(EditingCustomer.Phone))
                {
                    PhoneError = "Please enter a valid phone number.";
                }
                else if (EditingCustomer.Phone.Length > 20)
                {
                    PhoneError = "Phone number must be 20 characters or less.";
                }
                else
                {
                    PhoneError = string.Empty;
                }
            }
            else
            {
                PhoneError = string.Empty;
            }
        }

        /// <summary>
        /// Validates email format using a simple pattern.
        /// </summary>
        /// <param name="email">The email address to validate</param>
        /// <returns>True if the email format is valid</returns>
        private bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Validates phone number format using a simple pattern.
        /// </summary>
        /// <param name="phone">The phone number to validate</param>
        /// <returns>True if the phone format is valid</returns>
        private bool IsValidPhone(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return true; // Phone is optional

            // Remove common formatting characters
            string cleanPhone = phone.Replace(" ", "").Replace("-", "").Replace("(", "").Replace(")", "").Replace("+", "");
            
            // Check if it contains only digits and is a reasonable length
            return cleanPhone.All(char.IsDigit) && cleanPhone.Length >= 10 && cleanPhone.Length <= 15;
        }

        /// <summary>
        /// Checks if the specified email address is already in use by another customer.
        /// </summary>
        /// <param name="email">The email address to check</param>
        /// <returns>True if the email is in use by another customer</returns>
        private bool IsEmailInUse(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                var existingCustomer = _dataService.Customers.FirstOrDefault(c => 
                    string.Equals(c.Email, email, StringComparison.OrdinalIgnoreCase));

                // Email is in use if found and it's not the current customer being edited
                return existingCustomer != null && existingCustomer.Id != EditingCustomer.Id;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"EditViewModel: Error checking email usage: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Clears all validation error messages.
        /// </summary>
        private void ClearValidationErrors()
        {
            FirstNameError = string.Empty;
            LastNameError = string.Empty;
            EmailError = string.Empty;
            PhoneError = string.Empty;
        }

        #endregion

        #region Form Operations

        /// <summary>
        /// Cancels the edit operation and navigates back.
        /// This method checks for unsaved changes and may show a confirmation dialog.
        /// </summary>
        private void CancelEdit()
        {
            try
            {
                Debug.WriteLine("EditViewModel: Canceling edit operation");

                // In a real app, you would check for unsaved changes and show a confirmation dialog
                if (HasUnsavedChanges)
                {
                    // For this demo, we'll just navigate back without confirmation
                    // In production, show a dialog: "You have unsaved changes. Are you sure you want to cancel?"
                }

                NavigateBack();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"EditViewModel: Error canceling edit: {ex.Message}");
                SetError("Unable to cancel the operation. Please try again.");
            }
        }

        /// <summary>
        /// Resets the form to its original state.
        /// This method discards all changes and restores the original customer data.
        /// </summary>
        private void ResetToOriginal()
        {
            try
            {
                if (_originalCustomer == null)
                {
                    Debug.WriteLine("EditViewModel: No original customer to reset to");
                    return;
                }

                Debug.WriteLine("EditViewModel: Resetting form to original state");

                // Restore original values
                EditingCustomer.FirstName = _originalCustomer.FirstName;
                EditingCustomer.LastName = _originalCustomer.LastName;
                EditingCustomer.Email = _originalCustomer.Email;
                EditingCustomer.Phone = _originalCustomer.Phone;
                EditingCustomer.Company = _originalCustomer.Company;

                // Clear validation errors
                ClearValidationErrors();
                
                // Reset change tracking
                HasUnsavedChanges = false;
                
                // Clear any error or success messages
                ClearError();
                ClearSuccessMessage();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"EditViewModel: Error resetting form: {ex.Message}");
                SetError("Unable to reset the form. Please try again.");
            }
        }

        /// <summary>
        /// Clears all form fields.
        /// This method creates a new empty customer for data entry.
        /// </summary>
        private void ClearForm()
        {
            try
            {
                Debug.WriteLine("EditViewModel: Clearing form fields");

                // Create a new empty customer
                EditingCustomer = new Customer();
                
                // Clear validation errors
                ClearValidationErrors();
                
                // Reset change tracking
                HasUnsavedChanges = false;
                
                // Clear any error or success messages
                ClearError();
                ClearSuccessMessage();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"EditViewModel: Error clearing form: {ex.Message}");
                SetError("Unable to clear the form. Please try again.");
            }
        }

        #endregion

        #region Change Tracking

        /// <summary>
        /// Handles property changes on the editing customer to track modifications.
        /// </summary>
        /// <param name="sender">The customer object</param>
        /// <param name="e">Property change event arguments</param>
        private void EditingCustomer_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            try
            {
                // Track changes (except for LastModified which is automatically updated)
                if (e.PropertyName != nameof(Customer.LastModified))
                {
                    HasUnsavedChanges = true;
                    
                    // Perform real-time validation on the changed field
                    ValidateField(e.PropertyName);
                    
                    // Update command availability
                    RaiseCanExecuteChanged();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"EditViewModel: Error handling property change: {ex.Message}");
            }
        }

        #endregion

        #region Navigation

        /// <summary>
        /// Navigates back to the previous page.
        /// </summary>
        private void NavigateBack()
        {
            try
            {
                Debug.WriteLine("EditViewModel: Navigating back");
                
                if (NavigationHelper.CanGoBack)
                {
                    NavigationHelper.GoBack();
                }
                else
                {
                    // Fallback: navigate to Home page
                    NavigationHelper.NavigateTo<Views.HomePage>();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"EditViewModel: Error navigating back: {ex.Message}");
                SetError("Unable to navigate back. Please try again.");
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Clears the success message.
        /// </summary>
        private void ClearSuccessMessage()
        {
            SuccessMessage = string.Empty;
        }

        /// <summary>
        /// Raises CanExecuteChanged for all commands to update UI button states.
        /// </summary>
        private void RaiseCanExecuteChanged()
        {
            try
            {
                (SaveCommand as RelayCommand)?.RaiseCanExecuteChanged();
                (CancelCommand as RelayCommand)?.RaiseCanExecuteChanged();
                (ResetCommand as RelayCommand)?.RaiseCanExecuteChanged();
                (ClearCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"EditViewModel: Error raising CanExecuteChanged: {ex.Message}");
            }
        }

        #endregion

        #region Cleanup

        /// <summary>
        /// Performs cleanup when the ViewModel is no longer needed.
        /// </summary>
        protected override void Cleanup()
        {
            try
            {
                // Unsubscribe from property changes
                if (EditingCustomer != null)
                {
                    EditingCustomer.PropertyChanged -= EditingCustomer_PropertyChanged;
                }
                
                // Clear references
                _originalCustomer = null;
                EditingCustomer = null;
                
                Debug.WriteLine("EditViewModel: Cleanup completed");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"EditViewModel: Error during cleanup: {ex.Message}");
            }
            finally
            {
                base.Cleanup();
            }
        }

        #endregion
    }
}