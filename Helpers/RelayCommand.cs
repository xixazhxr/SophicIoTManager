using System;
using System.Windows.Input;

namespace SophicIoTManager.Helpers
{
    /// <summary>
    /// A basic implementation of ICommand for use when CommunityToolkit.Mvvm's RelayCommand is not sufficient.
    /// Note: CommunityToolkit.Mvvm's [RelayCommand] attribute is preferred for most use cases.
    /// This class is provided for scenarios requiring manual command creation or custom behavior.
    /// </summary>
    public class RelayCommand : ICommand
    {
        #region Fields

        private readonly Action<object?> _execute;
        private readonly Func<object?, bool>? _canExecute;

        #endregion

        #region Constructor

        /// <summary>
        /// Creates a new RelayCommand.
        /// </summary>
        /// <param name="execute">The action to execute when the command is invoked.</param>
        /// <param name="canExecute">Optional function to determine if the command can execute.</param>
        public RelayCommand(Action<object?> execute, Func<object?, bool>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        /// <summary>
        /// Creates a new RelayCommand with a parameterless action.
        /// </summary>
        /// <param name="execute">The action to execute when the command is invoked.</param>
        /// <param name="canExecute">Optional function to determine if the command can execute.</param>
        public RelayCommand(Action execute, Func<bool>? canExecute = null)
            : this(_ => execute(), canExecute != null ? _ => canExecute() : null)
        {
        }

        #endregion

        #region ICommand Implementation

        /// <summary>
        /// Event raised when the ability to execute changes.
        /// </summary>
        public event EventHandler? CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        /// <summary>
        /// Determines whether the command can execute with the given parameter.
        /// </summary>
        public bool CanExecute(object? parameter)
        {
            return _canExecute?.Invoke(parameter) ?? true;
        }

        /// <summary>
        /// Executes the command with the given parameter.
        /// </summary>
        public void Execute(object? parameter)
        {
            _execute(parameter);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Raises the CanExecuteChanged event to reevaluate command availability.
        /// </summary>
        public void RaiseCanExecuteChanged()
        {
            CommandManager.InvalidateRequerySuggested();
        }

        #endregion
    }

    /// <summary>
    /// Generic version of RelayCommand for strongly-typed parameters.
    /// </summary>
    /// <typeparam name="T">The type of the command parameter.</typeparam>
    public class RelayCommand<T> : ICommand
    {
        #region Fields

        private readonly Action<T?> _execute;
        private readonly Func<T?, bool>? _canExecute;

        #endregion

        #region Constructor

        /// <summary>
        /// Creates a new RelayCommand with a typed parameter.
        /// </summary>
        /// <param name="execute">The action to execute when the command is invoked.</param>
        /// <param name="canExecute">Optional function to determine if the command can execute.</param>
        public RelayCommand(Action<T?> execute, Func<T?, bool>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        #endregion

        #region ICommand Implementation

        public event EventHandler? CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        public bool CanExecute(object? parameter)
        {
            if (parameter == null && typeof(T).IsValueType)
                return _canExecute?.Invoke(default) ?? true;

            return _canExecute?.Invoke((T?)parameter) ?? true;
        }

        public void Execute(object? parameter)
        {
            _execute((T?)parameter);
        }

        #endregion

        #region Methods

        public void RaiseCanExecuteChanged()
        {
            CommandManager.InvalidateRequerySuggested();
        }

        #endregion
    }
}
