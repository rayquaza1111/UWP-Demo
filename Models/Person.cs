using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace UWPDemo.Models
{
    /// <summary>
    /// Model representing a person with data binding support.
    /// </summary>
    public class Person : INotifyPropertyChanged
    {
        private string _firstName = "";
        private string _lastName = "";
        private int _age = 25;
        private string _email = "";
        private string _bio = "";

        public event PropertyChangedEventHandler PropertyChanged;

        public string FirstName
        {
            get => _firstName;
            set
            {
                if (SetProperty(ref _firstName, value))
                {
                    OnPropertyChanged(nameof(FullName));
                }
            }
        }

        public string LastName
        {
            get => _lastName;
            set
            {
                if (SetProperty(ref _lastName, value))
                {
                    OnPropertyChanged(nameof(FullName));
                }
            }
        }

        public int Age
        {
            get => _age;
            set
            {
                if (SetProperty(ref _age, value))
                {
                    OnPropertyChanged(nameof(AgeDescription));
                }
            }
        }

        public string Email
        {
            get => _email;
            set => SetProperty(ref _email, value);
        }

        public string Bio
        {
            get => _bio;
            set => SetProperty(ref _bio, value);
        }

        // Computed properties
        public string FullName => $"{FirstName} {LastName}".Trim();

        public string AgeDescription
        {
            get
            {
                if (Age < 18)
                    return $"{Age} years old (Minor)";
                else if (Age < 65)
                    return $"{Age} years old (Adult)";
                else
                    return $"{Age} years old (Senior)";
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (Equals(field, value))
                return false;

            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}