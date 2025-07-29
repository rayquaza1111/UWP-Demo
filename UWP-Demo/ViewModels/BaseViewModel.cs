using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.UI.Core;
using Windows.ApplicationModel.Core;

namespace UWP_Demo.ViewModels
{
    /// <summary>
    /// Base class for all ViewModels in the application, providing common functionality
    /// such as property change notification, UI thread marshaling, and common services.
    /// This class implements the foundation of the MVVM pattern for UWP applications.
    /// </summary>
    /// <remarks>
    /// This base class provides several key features for MVVM development:
    /// - INotifyPropertyChanged implementation for data binding
    /// - Thread-safe property updates with UI thread marshaling
    /// - Common validation and error handling patterns
    /// - Lifecycle management for ViewModels
    /// - Integration with UWP-specific features like app lifecycle
    /// 
    /// All ViewModels in the application should inherit from this base class
    /// to ensure consistent behavior and reduce code duplication.
    /// </remarks>
    public abstract class BaseViewModel : INotifyPropertyChanged
    {
        #region Private Fields

        /// <summary>
        /// Flag indicating whether the ViewModel is currently in a busy state.
        /// This can be used to show loading indicators in the UI.
        /// </summary>
        private bool _isBusy;

        /// <summary>
        /// Current error message to display to the user.
        /// This is typically bound to error display controls in the UI.
        /// </summary>
        private string _errorMessage;

        /// <summary>
        /// Flag indicating whether there is currently an error condition.
        /// This can be used to show/hide error UI elements.
        /// </summary>
        private bool _hasError;

        /// <summary>
        /// The title or header text for the current view.
        /// This is often bound to page titles or header controls.
        /// </summary>
        private string _title;

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets whether the ViewModel is currently performing a long-running operation.
        /// When true, the UI typically shows loading indicators and disables interactive elements.
        /// </summary>
        /// <remarks>
        /// This property is commonly bound to ProgressRing.IsActive or used in value converters
        /// to control the visibility of loading UI. It's automatically set to false when
        /// operations complete or encounter errors.
        /// </remarks>
        /// <example>
        /// // In XAML:
        /// // &lt;ProgressRing IsActive="{Binding IsBusy}" /&gt;
        /// // &lt;Button IsEnabled="{Binding IsBusy, Converter={StaticResource InverseBooleanConverter}}" /&gt;
        /// </example>
        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        /// <summary>
        /// Gets or sets the current error message to display to the user.
        /// This should contain user-friendly error descriptions, not technical exception details.
        /// </summary>
        /// <remarks>
        /// Error messages should be localized and provide actionable information to the user.
        /// Setting this property automatically sets HasError to true if the message is not empty.
        /// </remarks>
        /// <example>
        /// // Setting an error message
        /// ErrorMessage = "Unable to save customer data. Please check your connection and try again.";
        /// </example>
        public string ErrorMessage
        {
            get => _errorMessage;
            set
            {
                if (SetProperty(ref _errorMessage, value))
                {
                    // Automatically update HasError based on whether there's an error message
                    HasError = !string.IsNullOrWhiteSpace(value);
                }
            }
        }

        /// <summary>
        /// Gets or sets whether there is currently an error condition.
        /// This is typically used to control the visibility of error UI elements.
        /// </summary>
        /// <remarks>
        /// This property is automatically updated when ErrorMessage changes, but can also
        /// be set manually for error conditions that don't have specific messages.
        /// </remarks>
        /// <example>
        /// // In XAML:
        /// // &lt;TextBlock Text="{Binding ErrorMessage}" Visibility="{Binding HasError, Converter={StaticResource BoolToVisibilityConverter}}" /&gt;
        /// </example>
        public bool HasError
        {
            get => _hasError;
            set => SetProperty(ref _hasError, value);
        }

        /// <summary>
        /// Gets or sets the title or header text for the current view.
        /// This is often bound to page titles, navigation headers, or window titles.
        /// </summary>
        /// <remarks>
        /// The title should be descriptive and help users understand what page or section
        /// they are currently viewing. It may change based on the current state or data.
        /// </remarks>
        /// <example>
        /// // In XAML:
        /// // &lt;TextBlock Text="{Binding Title}" Style="{StaticResource HeaderTextBlockStyle}" /&gt;
        /// </example>
        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the BaseViewModel class.
        /// Sets up default values and initializes common services.
        /// </summary>
        protected BaseViewModel()
        {
            // Initialize default values
            IsBusy = false;
            HasError = false;
            ErrorMessage = string.Empty;
            Title = string.Empty;

            // Perform any derived class initialization
            Initialize();
        }

        #endregion

        #region Lifecycle Methods

        /// <summary>
        /// Virtual method called during ViewModel initialization.
        /// Override this method in derived classes to perform specific initialization logic.
        /// </summary>
        /// <remarks>
        /// This method is called from the constructor and provides a place for derived classes
        /// to perform initialization without having to override the constructor.
        /// It's called after the base properties are initialized but before the ViewModel is used.
        /// </remarks>
        /// <example>
        /// protected override void Initialize()
        /// {
        ///     Title = "Customer Management";
        ///     LoadDataAsync();
        /// }
        /// </example>
        protected virtual void Initialize()
        {
            // Base implementation does nothing
            // Derived classes can override this for specific initialization
        }

        /// <summary>
        /// Virtual method called when the ViewModel should clean up resources.
        /// Override this method in derived classes to dispose of resources, unsubscribe from events, etc.
        /// </summary>
        /// <remarks>
        /// This method should be called when the ViewModel is no longer needed,
        /// typically when navigating away from a page or closing the application.
        /// It helps prevent memory leaks and ensures proper resource cleanup.
        /// </remarks>
        /// <example>
        /// protected override void Cleanup()
        /// {
        ///     // Unsubscribe from events
        ///     DataService.DataChanged -= OnDataChanged;
        ///     
        ///     // Clear collections
        ///     Customers.Clear();
        ///     
        ///     base.Cleanup();
        /// }
        /// </example>
        protected virtual void Cleanup()
        {
            // Clear error state
            ClearError();
            
            // Reset busy state
            IsBusy = false;
        }

        #endregion

        #region Error Handling

        /// <summary>
        /// Sets an error message and busy state based on an exception.
        /// This method extracts user-friendly error messages from exceptions.
        /// </summary>
        /// <param name="exception">The exception that occurred</param>
        /// <param name="userMessage">Optional user-friendly message to display instead of the exception message</param>
        /// <remarks>
        /// This method handles common exception types and provides appropriate user messages.
        /// It also logs the full exception details for debugging purposes while showing
        /// sanitized messages to the user.
        /// </remarks>
        /// <example>
        /// try
        /// {
        ///     await SaveDataAsync();
        /// }
        /// catch (Exception ex)
        /// {
        ///     SetError(ex, "Unable to save your changes. Please try again.");
        /// }
        /// </example>
        protected void SetError(Exception exception, string userMessage = null)
        {
            // Always stop any busy operations when an error occurs
            IsBusy = false;

            // Use provided user message or extract from exception
            string displayMessage = userMessage ?? GetUserFriendlyErrorMessage(exception);
            ErrorMessage = displayMessage;

            // Log the full exception for debugging (in a real app, use proper logging)
            System.Diagnostics.Debug.WriteLine($"ViewModel Error: {exception}");
        }

        /// <summary>
        /// Sets an error message with the specified text.
        /// </summary>
        /// <param name="errorMessage">The error message to display to the user</param>
        /// <remarks>
        /// Use this method when you have a specific error condition that doesn't
        /// involve an exception, such as validation errors or business logic violations.
        /// </remarks>
        /// <example>
        /// if (string.IsNullOrEmpty(CustomerName))
        /// {
        ///     SetError("Customer name is required.");
        ///     return;
        /// }
        /// </example>
        protected void SetError(string errorMessage)
        {
            IsBusy = false;
            ErrorMessage = errorMessage;
        }

        /// <summary>
        /// Clears the current error state.
        /// Call this method when starting new operations or when errors have been resolved.
        /// </summary>
        /// <example>
        /// // Clear errors before starting a new operation
        /// ClearError();
        /// IsBusy = true;
        /// await PerformOperationAsync();
        /// </example>
        protected void ClearError()
        {
            ErrorMessage = string.Empty;
            HasError = false;
        }

        /// <summary>
        /// Extracts a user-friendly error message from an exception.
        /// This method handles common exception types and provides appropriate messages.
        /// </summary>
        /// <param name="exception">The exception to extract a message from</param>
        /// <returns>A user-friendly error message</returns>
        private string GetUserFriendlyErrorMessage(Exception exception)
        {
            return exception switch
            {
                UnauthorizedAccessException => "Access denied. Please check your permissions.",
                System.Net.Http.HttpRequestException => "Network error. Please check your internet connection.",
                System.IO.FileNotFoundException => "Required file not found. Please try again.",
                System.IO.DirectoryNotFoundException => "Required folder not found. Please try again.",
                ArgumentException => "Invalid input provided. Please check your data and try again.",
                InvalidOperationException => "Operation cannot be completed at this time. Please try again.",
                TimeoutException => "Operation timed out. Please try again.",
                _ => "An unexpected error occurred. Please try again."
            };
        }

        #endregion

        #region UI Thread Helpers

        /// <summary>
        /// Executes an action on the UI thread asynchronously.
        /// This is essential for updating UI-bound properties from background threads.
        /// </summary>
        /// <param name="action">The action to execute on the UI thread</param>
        /// <remarks>
        /// In UWP applications, only the UI thread can update UI elements and properties
        /// that are bound to UI controls. This method ensures that property updates
        /// happen on the correct thread, preventing cross-thread exceptions.
        /// </remarks>
        /// <example>
        /// // Update UI properties from a background thread
        /// await RunOnUIThreadAsync(() =>
        /// {
        ///     CustomerCount = newCount;
        ///     IsBusy = false;
        /// });
        /// </example>
        protected async System.Threading.Tasks.Task RunOnUIThreadAsync(Action action)
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => action());
        }

        /// <summary>
        /// Executes an action on the UI thread synchronously if not already on the UI thread.
        /// Use this carefully as it can cause deadlocks if misused.
        /// </summary>
        /// <param name="action">The action to execute on the UI thread</param>
        /// <remarks>
        /// This method should be used sparingly and only when you're certain it won't cause
        /// deadlocks. In most cases, RunOnUIThreadAsync is preferred.
        /// </remarks>
        protected void RunOnUIThread(Action action)
        {
            var dispatcher = CoreApplication.MainView.CoreWindow.Dispatcher;
            if (dispatcher.HasThreadAccess)
            {
                action();
            }
            else
            {
                dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => action()).AsTask().Wait();
            }
        }

        #endregion

        #region INotifyPropertyChanged Implementation

        /// <summary>
        /// Event raised when a property value changes.
        /// This is the core of data binding in WPF/UWP applications.
        /// </summary>
        /// <remarks>
        /// UI controls automatically subscribe to this event when data binding is used.
        /// When a property changes, the UI is automatically updated to reflect the new value.
        /// This enables the separation of UI and business logic that is central to MVVM.
        /// </remarks>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Sets the property value and raises PropertyChanged if the value actually changed.
        /// This method handles all the boilerplate code for property setters in ViewModels.
        /// </summary>
        /// <typeparam name="T">The type of the property being set</typeparam>
        /// <param name="field">Reference to the backing field for the property</param>
        /// <param name="value">The new value to set</param>
        /// <param name="propertyName">
        /// The name of the property (automatically provided by the compiler when called from a property setter)
        /// </param>
        /// <returns>True if the property value was changed, false if it was already the same value</returns>
        /// <remarks>
        /// This method ensures that PropertyChanged is only raised when the value actually changes,
        /// which improves performance and prevents unnecessary UI updates. It also handles
        /// thread-safety by marshaling the PropertyChanged event to the UI thread.
        /// </remarks>
        /// <example>
        /// private string _customerName;
        /// public string CustomerName
        /// {
        ///     get => _customerName;
        ///     set => SetProperty(ref _customerName, value);
        /// }
        /// </example>
        protected virtual bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            // Check if the value actually changed
            if (EqualityComparer<T>.Default.Equals(field, value))
                return false;

            // Update the field with the new value
            field = value;

            // Raise the PropertyChanged event on the UI thread
            OnPropertyChanged(propertyName);

            return true;
        }

        /// <summary>
        /// Raises the PropertyChanged event for the specified property.
        /// This method ensures the event is raised on the UI thread for thread safety.
        /// </summary>
        /// <param name="propertyName">
        /// The name of the property that changed (automatically provided by the compiler when called from a property setter)
        /// </param>
        /// <remarks>
        /// This method can be called manually when a property's value changes due to external factors
        /// or when computed properties need to be updated based on other property changes.
        /// </remarks>
        /// <example>
        /// // Manually raise PropertyChanged for a computed property
        /// private void OnCustomerDataChanged()
        /// {
        ///     OnPropertyChanged(nameof(FullName));
        ///     OnPropertyChanged(nameof(DisplayText));
        /// }
        /// </example>
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                // Ensure the event is raised on the UI thread
                var dispatcher = CoreApplication.MainView.CoreWindow.Dispatcher;
                if (dispatcher.HasThreadAccess)
                {
                    handler(this, new PropertyChangedEventArgs(propertyName));
                }
                else
                {
                    // Marshal to UI thread asynchronously
                    _ = dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                        handler(this, new PropertyChangedEventArgs(propertyName)));
                }
            }
        }

        /// <summary>
        /// Raises PropertyChanged events for multiple properties at once.
        /// This is useful when multiple related properties change simultaneously.
        /// </summary>
        /// <param name="propertyNames">The names of the properties that changed</param>
        /// <example>
        /// // Update multiple related properties
        /// private void ResetCustomer()
        /// {
        ///     _firstName = string.Empty;
        ///     _lastName = string.Empty;
        ///     _email = string.Empty;
        ///     
        ///     OnPropertiesChanged(nameof(FirstName), nameof(LastName), nameof(Email), nameof(FullName));
        /// }
        /// </example>
        protected void OnPropertiesChanged(params string[] propertyNames)
        {
            foreach (string propertyName in propertyNames)
            {
                OnPropertyChanged(propertyName);
            }
        }

        #endregion

        #region Validation Support

        /// <summary>
        /// Dictionary to store validation errors for properties.
        /// This can be used to implement INotifyDataErrorInfo in derived classes.
        /// </summary>
        private readonly Dictionary<string, List<string>> _validationErrors = new Dictionary<string, List<string>>();

        /// <summary>
        /// Gets whether the ViewModel currently has any validation errors.
        /// </summary>
        public bool HasValidationErrors => _validationErrors.Count > 0;

        /// <summary>
        /// Adds a validation error for the specified property.
        /// </summary>
        /// <param name="propertyName">The name of the property with the error</param>
        /// <param name="errorMessage">The validation error message</param>
        protected void AddValidationError(string propertyName, string errorMessage)
        {
            if (!_validationErrors.ContainsKey(propertyName))
            {
                _validationErrors[propertyName] = new List<string>();
            }

            if (!_validationErrors[propertyName].Contains(errorMessage))
            {
                _validationErrors[propertyName].Add(errorMessage);
                OnPropertyChanged(nameof(HasValidationErrors));
            }
        }

        /// <summary>
        /// Removes all validation errors for the specified property.
        /// </summary>
        /// <param name="propertyName">The name of the property to clear errors for</param>
        protected void ClearValidationErrors(string propertyName)
        {
            if (_validationErrors.Remove(propertyName))
            {
                OnPropertyChanged(nameof(HasValidationErrors));
            }
        }

        /// <summary>
        /// Removes all validation errors.
        /// </summary>
        protected void ClearAllValidationErrors()
        {
            if (_validationErrors.Count > 0)
            {
                _validationErrors.Clear();
                OnPropertyChanged(nameof(HasValidationErrors));
            }
        }

        /// <summary>
        /// Gets the validation errors for the specified property.
        /// </summary>
        /// <param name="propertyName">The name of the property</param>
        /// <returns>A list of validation error messages for the property</returns>
        protected List<string> GetValidationErrors(string propertyName)
        {
            return _validationErrors.TryGetValue(propertyName, out List<string> errors) ? errors : new List<string>();
        }

        #endregion
    }
}