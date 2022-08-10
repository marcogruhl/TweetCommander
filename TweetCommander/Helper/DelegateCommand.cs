using System;
using System.Windows.Input;

namespace TweetCommander.Helper;

/// <summary>
/// Generic implementation of <see cref="ICommand"/>.
/// </summary>
/// <typeparam name="T">
/// Type of the parameter the command expects.
/// </typeparam>
/// <remarks>
/// Copied from https://instance-factory.com/?p=785
/// Copied from http://social.msdn.microsoft.com/Forums/en-US/f457c906-56d3-49c7-91c4-cc35a6ec5d35/icommand-and-mvvm
/// Copied from https://stackoverflow.com/a/31807633/9624651
/// </remarks>
public class DelegateCommand<T> : BaseExecute, ICommand
{
    /// <summary>
    /// Gets / sets the action to be executed.
    /// </summary>
    private Action<T> ExecuteAction { get; set; }

    /// <summary>
    /// Initializes a new instance of <see cref="DelegateCommand"/>
    /// with the action to be executed.
    /// </summary>
    /// <param name="executeAction">
    /// Action to be executed.
    /// </param>
    public DelegateCommand(Action<T> executeAction)
    {
        ExecuteAction = executeAction;
    }

    public DelegateCommand(Action<T> executeAction,Func<bool> canExecuteFunc)
    {
        ExecuteAction = executeAction;
        _canExecuteFunc = canExecuteFunc;
    }

    /// <summary>
    /// Invokes the method to be called.
    /// </summary>
    /// <param name="parameter">Data used by the command.</param>
    public void Execute(object parameter)
    {
        ExecuteAction((T) Convert.ChangeType(parameter, typeof(T)));
    }
}

public class DelegateCommand :  BaseExecute, ICommand
{
    /// <summary>
    /// Gets / sets the action to be executed.
    /// </summary>
    private Action ExecuteAction { get; set; }

    /// <summary>
    /// Initializes a new instance of <see cref="DelegateCommand"/>
    /// with the action to be executed.
    /// </summary>
    /// <param name="executeAction">
    /// Action to be executed.
    /// </param>
    public DelegateCommand(Action executeAction)
    {
        ExecuteAction = executeAction;
    }

    public DelegateCommand(Action executeAction,Func<bool> canExecuteFunc)
    {
        ExecuteAction = executeAction;
        _canExecuteFunc = canExecuteFunc;
    }

    /// <summary>
    /// Invokes the method to be called.
    /// </summary>
    /// <param name="parameter">Data used by the command.</param>
    public void Execute(object? parameter)
    {
        ExecuteAction();
    }
}

public class BaseExecute
{
    protected Func<bool> _canExecuteFunc;
    
    /// <summary>
    /// Occurs when changes occur that affect whether 
    /// the command should execute.
    /// </summary>
    public event EventHandler CanExecuteChanged;

    /// <summary>
    /// Determines whether the command can execute in its current state.
    /// </summary>
    /// <param name="parameter">Data used by the command.</param>
    /// <returns>
    /// <c>true</c> if this command can be executed; 
    /// otherwise, <c>false</c>.
    /// </returns>
    public bool CanExecute(object parameter)
    {
        if (_canExecuteFunc != null)
            return _canExecuteFunc();
        return true;
    }

    public void RaiseCanExecuteChanged()
    {
        if(CanExecuteChanged != null)
            CanExecuteChanged(this, new EventArgs());
    }
}