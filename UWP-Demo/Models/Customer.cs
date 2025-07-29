using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;

namespace UWP_Demo.Models
{
    /// <summary>
    /// Represents a customer entity in the customer management system.
    /// This class implements INotifyPropertyChanged to support data binding in XAML UI.
    /// The customer data can be serialized to/from JSON for file storage and network operations.
    /// </summary>
    /// <remarks>
    /// This model demonstrates:
    /// - Property change notification for UI data binding
    /// - JSON serialization for data persistence
    /// - Input validation for business logic
    /// - Proper encapsulation of customer data
    /// </remarks>
    public class Customer : INotifyPropertyChanged
    {
        #region Private Fields
        
        private int _id;
        private string _firstName;
        private string _lastName;
        private string _email;
        private string _phone;
        private string _company;
        private DateTime _dateCreated;
        private DateTime _lastModified;

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the unique identifier for the customer.
        /// This ID is used for database operations and customer lookup.
        /// </summary>
        [JsonProperty("id")]
        public int Id
        {
            get => _id;
            set => SetProperty(ref _id, value);
        }

        /// <summary>
        /// Gets or sets the customer's first name.
        /// This field is required and cannot be null or empty.
        /// </summary>
        [JsonProperty("firstName")]
        public string FirstName
        {
            get => _firstName;
            set => SetProperty(ref _firstName, value);
        }

        /// <summary>
        /// Gets or sets the customer's last name.
        /// This field is required and cannot be null or empty.
        /// </summary>
        [JsonProperty("lastName")]
        public string LastName
        {
            get => _lastName;
            set => SetProperty(ref _lastName, value);
        }

        /// <summary>
        /// Gets the customer's full name by combining first and last names.
        /// This is a computed property that automatically updates when either name changes.
        /// </summary>
        [JsonIgnore]
        public string FullName => $"{FirstName} {LastName}".Trim();

        /// <summary>
        /// Gets or sets the customer's email address.
        /// This should be validated for proper email format in the UI layer.
        /// </summary>
        [JsonProperty("email")]
        public string Email
        {
            get => _email;
            set => SetProperty(ref _email, value);
        }

        /// <summary>
        /// Gets or sets the customer's phone number.
        /// This field is optional but should be validated for proper format when provided.
        /// </summary>
        [JsonProperty("phone")]
        public string Phone
        {
            get => _phone;
            set => SetProperty(ref _phone, value);
        }

        /// <summary>
        /// Gets or sets the customer's company name.
        /// This field is optional and can be used for business customers.
        /// </summary>
        [JsonProperty("company")]
        public string Company
        {
            get => _company;
            set => SetProperty(ref _company, value);
        }

        /// <summary>
        /// Gets or sets the date when the customer record was created.
        /// This is automatically set when a new customer is created.
        /// </summary>
        [JsonProperty("dateCreated")]
        public DateTime DateCreated
        {
            get => _dateCreated;
            set => SetProperty(ref _dateCreated, value);
        }

        /// <summary>
        /// Gets or sets the date when the customer record was last modified.
        /// This is automatically updated whenever the customer data changes.
        /// </summary>
        [JsonProperty("lastModified")]
        public DateTime LastModified
        {
            get => _lastModified;
            set => SetProperty(ref _lastModified, value);
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the Customer class.
        /// Sets default values for creation and modification dates.
        /// </summary>
        public Customer()
        {
            // Initialize dates to current time when creating a new customer
            DateCreated = DateTime.Now;
            LastModified = DateTime.Now;
            
            // Initialize string properties to empty strings to avoid null reference issues
            FirstName = string.Empty;
            LastName = string.Empty;
            Email = string.Empty;
            Phone = string.Empty;
            Company = string.Empty;
        }

        /// <summary>
        /// Initializes a new instance of the Customer class with specified details.
        /// </summary>
        /// <param name="firstName">The customer's first name</param>
        /// <param name="lastName">The customer's last name</param>
        /// <param name="email">The customer's email address</param>
        public Customer(string firstName, string lastName, string email) : this()
        {
            FirstName = firstName ?? string.Empty;
            LastName = lastName ?? string.Empty;
            Email = email ?? string.Empty;
        }

        #endregion

        #region Validation Methods

        /// <summary>
        /// Validates the customer data to ensure all required fields are properly filled.
        /// This method can be used by the UI to show validation errors.
        /// </summary>
        /// <returns>True if the customer data is valid, false otherwise</returns>
        public bool IsValid()
        {
            return !string.IsNullOrWhiteSpace(FirstName) &&
                   !string.IsNullOrWhiteSpace(LastName) &&
                   !string.IsNullOrWhiteSpace(Email) &&
                   IsValidEmail(Email);
        }

        /// <summary>
        /// Validates the email address format using a simple regex pattern.
        /// This is a basic validation that can be enhanced with more sophisticated patterns.
        /// </summary>
        /// <param name="email">The email address to validate</param>
        /// <returns>True if the email format is valid, false otherwise</returns>
        private bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                // Use built-in email validation from System.Net.Mail
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region INotifyPropertyChanged Implementation

        /// <summary>
        /// Event raised when a property value changes.
        /// This is essential for data binding in XAML UI controls.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Sets the property value and raises PropertyChanged event if the value actually changed.
        /// This method ensures that UI controls are automatically updated when data changes.
        /// </summary>
        /// <typeparam name="T">The type of the property being set</typeparam>
        /// <param name="field">Reference to the backing field</param>
        /// <param name="value">The new value to set</param>
        /// <param name="propertyName">Name of the property (automatically provided by compiler)</param>
        /// <returns>True if the property value was changed, false if it was the same</returns>
        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            // Only update if the value actually changed
            if (Equals(field, value))
                return false;

            field = value;
            
            // Update the LastModified timestamp whenever any property changes
            if (propertyName != nameof(LastModified))
            {
                _lastModified = DateTime.Now;
                OnPropertyChanged(nameof(LastModified));
            }
            
            // Notify that this property changed
            OnPropertyChanged(propertyName);
            
            // If first or last name changed, also notify that FullName changed
            if (propertyName == nameof(FirstName) || propertyName == nameof(LastName))
            {
                OnPropertyChanged(nameof(FullName));
            }
            
            return true;
        }

        /// <summary>
        /// Raises the PropertyChanged event for the specified property.
        /// This notifies the UI that a property value has changed and binding should be updated.
        /// </summary>
        /// <param name="propertyName">The name of the property that changed</param>
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        #region Object Overrides

        /// <summary>
        /// Returns a string representation of the customer for debugging and logging purposes.
        /// </summary>
        /// <returns>A formatted string containing customer information</returns>
        public override string ToString()
        {
            return $"Customer {Id}: {FullName} ({Email})";
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current customer.
        /// Two customers are considered equal if they have the same ID.
        /// </summary>
        /// <param name="obj">The object to compare with the current customer</param>
        /// <returns>True if the objects are equal, false otherwise</returns>
        public override bool Equals(object obj)
        {
            if (obj is Customer other)
            {
                return Id == other.Id;
            }
            return false;
        }

        /// <summary>
        /// Returns a hash code for the current customer based on the ID.
        /// This is used for efficient storage in collections like HashSet and Dictionary.
        /// </summary>
        /// <returns>A hash code for the current customer</returns>
        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        #endregion
    }
}