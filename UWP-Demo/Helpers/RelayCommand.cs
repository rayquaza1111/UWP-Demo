using System;
using System.Windows.Input;

namespace UWP_Demo.Helpers
{
    /// <summary>
    /// A command implementation that implements ICommand for use in MVVM patterns.
    /// This class allows ViewModels to expose commands that can be bound to UI elements
    /// like buttons, menu items, and other interactive controls.
    /// </summary>
    /// <remarks>
    /// RelayCommand is a fundamental building block of MVVM applications:
    /// - It allows business logic to be kept in ViewModels instead of code-behind
    /// - It supports automatic enabling/disabling of UI elements based on CanExecute
    /// - It provides a clean separation between UI and business logic
    /// - It enables unit testing of command logic without UI dependencies
    /// 
    /// This implementation supports both parameterless and parameterized commands,
    /// making it flexible for various UI scenarios.
    /// </remarks>
    public class RelayCommand : ICommand
    {
        #region Private Fields

        /// <summary>
        /// The action to execute when the command is invoked.
        /// This can be either a simple Action or an Action&lt;object&gt; for parameterized commands.
        /// </summary>
        private readonly Action<object> _execute;

        /// <summary>
        /// The function that determines whether the command can be executed.
        /// This is used to enable/disable UI elements bound to this command.
        /// </summary>
        private readonly Func<object, bool> _canExecute;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of RelayCommand with the specified execute action.
        /// The command will always be executable (CanExecute always returns true).
        /// </summary>
        /// <param name="execute">The action to execute when the command is invoked</param>
        /// <exception cref="ArgumentNullException">Thrown if execute is null</exception>
        /// <example>
        /// // Simple command without parameters
        /// public ICommand SaveCommand => new RelayCommand(() => SaveData());
        /// </example>
        public RelayCommand(Action execute) : this(execute, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of RelayCommand with the specified execute action and canExecute function.
        /// </summary>
        /// <param name="execute">The action to execute when the command is invoked</param>
        /// <param name="canExecute">The function that determines whether the command can be executed</param>
        /// <exception cref="ArgumentNullException">Thrown if execute is null</exception>
        /// <example>
        /// // Command with conditional execution
        /// public ICommand DeleteCommand => new RelayCommand(
        ///     () => DeleteSelectedItem(),
        ///     () => SelectedItem != null);
        /// </example>
        public RelayCommand(Action execute, Func<bool> canExecute)
        {
            if (execute == null)
                throw new ArgumentNullException(nameof(execute));

            // Wrap the parameterless actions to match our internal parameterized signatures
            _execute = _ => execute();
            _canExecute = canExecute == null ? null : _ => canExecute();
        }

        /// <summary>
        /// Initializes a new instance of RelayCommand with a parameterized execute action.
        /// The command will always be executable (CanExecute always returns true).
        /// </summary>
        /// <param name="execute">The action to execute when the command is invoked, receiving the command parameter</param>
        /// <exception cref="ArgumentNullException">Thrown if execute is null</exception>
        /// <example>
        /// // Parameterized command
        /// public ICommand SelectCustomerCommand => new RelayCommand(
        ///     parameter => SelectCustomer((Customer)parameter));
        /// </example>
        public RelayCommand(Action<object> execute) : this(execute, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of RelayCommand with a parameterized execute action and canExecute function.
        /// </summary>
        /// <param name="execute">The action to execute when the command is invoked, receiving the command parameter</param>
        /// <param name="canExecute">The function that determines whether the command can be executed, receiving the command parameter</param>
        /// <exception cref="ArgumentNullException">Thrown if execute is null</exception>
        /// <example>
        /// // Parameterized command with conditional execution
        /// public ICommand EditCustomerCommand => new RelayCommand(
        ///     parameter => EditCustomer((Customer)parameter),
        ///     parameter => parameter is Customer customer && customer.IsValid());
        /// </example>
        public RelayCommand(Action<object> execute, Func<object, bool> canExecute)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        #endregion

        #region ICommand Implementation

        /// <summary>
        /// Occurs when changes occur that affect whether the command should execute.
        /// This event is automatically managed by the CommandManager in WPF/UWP applications.
        /// </summary>
        /// <remarks>
        /// UI elements that are bound to this command will automatically subscribe to this event
        /// and call CanExecute when it's raised. This allows buttons and other controls to
        /// automatically enable/disable themselves based on the current application state.
        /// 
        /// To manually trigger this event (for example, when application state changes),
        /// call the RaiseCanExecuteChanged() method.
        /// </remarks>
        public event EventHandler CanExecuteChanged;

        /// <summary>
        /// Determines whether the command can execute in its current state.
        /// </summary>
        /// <param name="parameter">
        /// Data used by the command. This can be null if the command doesn't require data.
        /// The parameter is typically passed from the UI element that triggered the command.
        /// </param>
        /// <returns>
        /// True if the command can be executed; otherwise, false.
        /// If this returns false, UI elements bound to this command will be disabled.
        /// </returns>
        /// <remarks>
        /// This method is called by the UI framework to determine whether interactive elements
        /// (like buttons) should be enabled or disabled. It should be lightweight and fast
        /// since it may be called frequently.
        /// </remarks>
        /// <example>
        /// // In XAML, a button bound to this command will automatically be enabled/disabled
        /// // &lt;Button Command="{Binding MyCommand}" CommandParameter="{Binding SelectedItem}" /&gt;
        /// </example>
        public bool CanExecute(object parameter)
        {
            // If no canExecute function was provided, the command can always execute
            return _canExecute?.Invoke(parameter) ?? true;
        }

        /// <summary>
        /// Executes the command's action with the specified parameter.
        /// </summary>
        /// <param name="parameter">
        /// Data used by the command. This can be null if the command doesn't require data.
        /// The parameter is typically passed from the UI element that triggered the command.
        /// </param>
        /// <remarks>
        /// This method should only be called if CanExecute returns true for the same parameter.
        /// The UI framework typically handles this automatically, but when calling commands
        /// programmatically, you should check CanExecute first.
        /// 
        /// Any exceptions thrown by the execute action will propagate to the caller.
        /// Consider wrapping the action in try-catch blocks if error handling is needed.
        /// </remarks>
        /// <example>
        /// // Manual command execution with parameter checking
        /// if (myCommand.CanExecute(parameter))
        /// {
        ///     myCommand.Execute(parameter);
        /// }
        /// </example>
        public void Execute(object parameter)
        {
            _execute(parameter);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Raises the CanExecuteChanged event to notify the UI that the command's executable state may have changed.
        /// This should be called whenever application state changes in a way that might affect whether the command can execute.
        /// </summary>
        /// <remarks>
        /// Call this method when:
        /// - Properties that affect the CanExecute logic have changed
        /// - Application state that impacts command availability has changed
        /// - You want to force a refresh of UI element enabled/disabled states
        /// 
        /// The UI framework will respond to this event by calling CanExecute on all bound commands
        /// and updating the visual state of associated UI elements.
        /// </remarks>
        /// <example>
        /// // In a ViewModel, when a property changes that affects command availability:
        /// private Customer _selectedCustomer;
        /// public Customer SelectedCustomer
        /// {
        ///     get => _selectedCustomer;
        ///     set
        ///     {
        ///         _selectedCustomer = value;
        ///         OnPropertyChanged();
        ///         // Notify that commands depending on selection may have changed
        ///         DeleteCustomerCommand.RaiseCanExecuteChanged();
        ///         EditCustomerCommand.RaiseCanExecuteChanged();
        ///     }
        /// }
        /// </example>
        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }

        #endregion
    }

    /// <summary>
    /// A strongly-typed version of RelayCommand that provides type safety for the command parameter.
    /// This generic version eliminates the need for casting in the execute and canExecute methods.
    /// </summary>
    /// <typeparam name="T">The type of the command parameter</typeparam>
    /// <remarks>
    /// This generic version is useful when you know the exact type of parameter your command expects.
    /// It provides compile-time type checking and eliminates runtime casting errors.
    /// 
    /// Use this when:
    /// - You have a specific parameter type (Customer, string, int, etc.)
    /// - You want to avoid casting in your command handlers
    /// - You want stronger type safety in your ViewModels
    /// </remarks>
    /// <example>
    /// // Strongly-typed command for Customer objects
    /// public ICommand SelectCustomerCommand => new RelayCommand&lt;Customer&gt;(
    ///     customer => SelectCustomer(customer),
    ///     customer => customer != null && customer.IsValid());
    /// </example>
    public class RelayCommand<T> : ICommand
    {
        #region Private Fields

        /// <summary>
        /// The action to execute when the command is invoked, with a strongly-typed parameter.
        /// </summary>
        private readonly Action<T> _execute;

        /// <summary>
        /// The function that determines whether the command can be executed, with a strongly-typed parameter.
        /// </summary>
        private readonly Func<T, bool> _canExecute;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of RelayCommand&lt;T&gt; with the specified execute action.
        /// The command will always be executable (CanExecute always returns true).
        /// </summary>
        /// <param name="execute">The action to execute when the command is invoked</param>
        /// <exception cref="ArgumentNullException">Thrown if execute is null</exception>
        public RelayCommand(Action<T> execute) : this(execute, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of RelayCommand&lt;T&gt; with the specified execute action and canExecute function.
        /// </summary>
        /// <param name="execute">The action to execute when the command is invoked</param>
        /// <param name="canExecute">The function that determines whether the command can be executed</param>
        /// <exception cref="ArgumentNullException">Thrown if execute is null</exception>
        public RelayCommand(Action<T> execute, Func<T, bool> canExecute)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        #endregion

        #region ICommand Implementation

        /// <summary>
        /// Occurs when changes occur that affect whether the command should execute.
        /// </summary>
        public event EventHandler CanExecuteChanged;

        /// <summary>
        /// Determines whether the command can execute in its current state.
        /// </summary>
        /// <param name="parameter">The command parameter, which will be cast to type T</param>
        /// <returns>True if the command can be executed; otherwise, false</returns>
        public bool CanExecute(object parameter)
        {
            // Handle null parameters gracefully
            if (parameter == null && !typeof(T).IsValueType)
                return _canExecute?.Invoke(default(T)) ?? true;

            // Try to cast the parameter to the expected type
            if (parameter is T typedParameter)
                return _canExecute?.Invoke(typedParameter) ?? true;

            // If parameter is wrong type, command cannot execute
            return false;
        }

        /// <summary>
        /// Executes the command's action with the specified parameter.
        /// </summary>
        /// <param name="parameter">The command parameter, which will be cast to type T</param>
        public void Execute(object parameter)
        {
            // Handle null parameters gracefully
            if (parameter == null && !typeof(T).IsValueType)
            {
                _execute(default(T));
                return;
            }

            // Cast the parameter to the expected type and execute
            if (parameter is T typedParameter)
            {
                _execute(typedParameter);
            }
            else
            {
                throw new InvalidCastException($"Parameter must be of type {typeof(T).Name}, but was {parameter?.GetType().Name ?? "null"}");
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Raises the CanExecuteChanged event to notify the UI that the command's executable state may have changed.
        /// </summary>
        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }

        #endregion
    }
}