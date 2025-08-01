using System;
using System.Windows.Input;

namespace UWP_Demo.Commands
{
    /// <summary>
    /// A basic command implementation that wraps an Action.
    /// This is essential for MVVM command binding in UWP.
    /// Uses the proper System.Windows.Input.ICommand interface.
    /// </summary>
    public class RelayCommand : ICommand
    {
        private readonly Action _execute;
        private readonly Func<bool> _canExecute;

        public RelayCommand(Action execute, Func<bool> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return _canExecute?.Invoke() ?? true;
        }

        public void Execute(object parameter)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("RelayCommand: Execute called");
                _execute();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"RelayCommand: Error executing - {ex.Message}");
            }
        }

        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <summary>
    /// A generic command implementation that wraps an Action with a parameter.
    /// Uses the proper System.Windows.Input.ICommand interface.
    /// </summary>
    public class RelayCommand<T> : ICommand
    {
        private readonly Action<T> _execute;
        private readonly Predicate<T> _canExecute;

        public RelayCommand(Action<T> execute, Predicate<T> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return _canExecute?.Invoke((T)parameter) ?? true;
        }

        public void Execute(object parameter)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"RelayCommand<T>: Execute called with parameter: {parameter}");
                _execute((T)parameter);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"RelayCommand<T>: Error executing - {ex.Message}");
            }
        }

        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}