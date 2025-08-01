using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
// FILE I/O: JSON serialization support for file persistence
using Newtonsoft.Json;  // FILE I/O: Enables automatic conversion between Customer objects and JSON for file storage

namespace UWP_Demo.Models
{
    /// <summary>
    /// FILE I/O: Customer entity designed for file persistence using JSON serialization
    /// This class implements INotifyPropertyChanged to support data binding in XAML UI.
    /// The customer data can be serialized to/from JSON for file storage and network operations.
    /// 
    /// FILE I/O DESIGN:
    /// - All properties have [JsonProperty] attributes to control JSON field names
    /// - [JsonIgnore] on computed properties that shouldn't be saved to file
    /// - Automatic timestamp tracking for file audit trail
    /// - Parameterless constructor for JSON deserialization
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

        #region FILE I/O: Properties with JSON Serialization Support

        /// <summary>
        /// FILE I/O: Unique identifier for the customer - saved to JSON file
        /// This ID is used for database operations and customer lookup.
        /// </summary>
        [JsonProperty("id")]  // FILE I/O: Maps to "id" field in JSON file
        public int Id
        {
            get => _id;
            set => SetProperty(ref _id, value);
        }

        /// <summary>
        /// FILE I/O: Customer's first name - saved to JSON file
        /// This field is required and cannot be null or empty.
        /// </summary>
        [JsonProperty("firstName")]  // FILE I/O: Maps to "firstName" field in JSON file
        public string FirstName
        {
            get => _firstName;
            set => SetProperty(ref _firstName, value);
        }

        /// <summary>
        /// FILE I/O: Customer's last name - saved to JSON file
        /// This field is required and cannot be null or empty.
        /// </summary>
        [JsonProperty("lastName")]  // FILE I/O: Maps to "lastName" field in JSON file
        public string LastName
        {
            get => _lastName;
            set => SetProperty(ref _lastName, value);
        }

        /// <summary>
        /// FILE I/O: Computed property - NOT saved to JSON file
        /// Gets the customer's full name by combining first and last names.
        /// This is a computed property that automatically updates when either name changes.
        /// </summary>
        [JsonIgnore]  // FILE I/O: Exclude from JSON - computed at runtime
        public string FullName => $"{FirstName} {LastName}".Trim();

        /// <summary>
        /// FILE I/O: Customer's email address - saved to JSON file
        /// This should be validated for proper email format in the UI layer.
        /// </summary>
        [JsonProperty("email")]  // FILE I/O: Maps to "email" field in JSON file
        public string Email
        {
            get => _email;
            set => SetProperty(ref _email, value);
        }

        /// <summary>
        /// FILE I/O: Customer's phone number - saved to JSON file
        /// This field is optional but should be validated for proper format when provided.
        /// </summary>
        [JsonProperty("phone")]  // FILE I/O: Maps to "phone" field in JSON file
        public string Phone
        {
            get => _phone;
            set => SetProperty(ref _phone, value);
        }

        /// <summary>
        /// FILE I/O: Customer's company name - saved to JSON file
        /// This field is optional and can be used for business customers.
        /// </summary>
        [JsonProperty("company")]  // FILE I/O: Maps to "company" field in JSON file
        public string Company
        {
            get => _company;
            set => SetProperty(ref _company, value);
        }

        /// <summary>
        /// FILE I/O: Creation timestamp - saved to JSON file for audit trail
        /// Gets or sets the date when the customer record was created.
        /// This is automatically set when a new customer is created.
        /// </summary>
        [JsonProperty("dateCreated")]  // FILE I/O: Maps to "dateCreated" field in JSON file
        public DateTime DateCreated
        {
            get => _dateCreated;
            set => SetProperty(ref _dateCreated, value);
        }

        /// <summary>
        /// FILE I/O: Modification timestamp - saved to JSON file for audit trail
        /// Gets or sets the date when the customer record was last modified.
        /// This is automatically updated whenever the customer data changes.
        /// </summary>
        [JsonProperty("lastModified")]  // FILE I/O: Maps to "lastModified" field in JSON file
        public DateTime LastModified
        {
            get => _lastModified;
            set => SetProperty(ref _lastModified, value);
        }

        #endregion

        #region FILE I/O: Constructors for JSON Serialization

        /// <summary>
        /// FILE I/O: Parameterless constructor required for JSON deserialization
        /// Initializes a new instance of the Customer class.
        /// Sets default values for creation and modification dates.
        /// This constructor is called when loading customers from JSON file.
        /// </summary>
        public Customer()
        {
            // FILE I/O: Initialize dates to current time when creating a new customer
            DateCreated = DateTime.Now;
            LastModified = DateTime.Now;
            
            // FILE I/O: Initialize string properties to empty strings to avoid null reference issues
            FirstName = string.Empty;
            LastName = string.Empty;
            Email = string.Empty;
            Phone = string.Empty;
            Company = string.Empty;
        }

        /// <summary>
        /// FILE I/O: Convenience constructor for creating customers programmatically
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
        /// FILE I/O: Property setter with automatic timestamp tracking
        /// Sets the property value and raises PropertyChanged event if the value actually changed.
        /// This method ensures that UI controls are automatically updated when data changes.
        /// Also automatically updates LastModified timestamp for file audit trail.
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
            
            // FILE I/O: Update the LastModified timestamp whenever any property changes
            // This creates an audit trail in the JSON file showing when data was last changed
            if (propertyName != nameof(LastModified))
            {
                _lastModified = DateTime.Now;
                OnPropertyChanged(nameof(LastModified));  // FILE I/O: Notify UI of timestamp change
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