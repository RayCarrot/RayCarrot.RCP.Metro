namespace RayCarrot.RCP.Metro;

/// <summary>
/// A RelayCommand to use for standard commands
/// </summary>
public class RelayCommand : BaseRelayCommand
{
    #region Constructors

    /// <summary>
    /// Constructor for an action with no parameters
    /// </summary>
    /// <param name="action">The action to associate with this RelayCommand</param>
    public RelayCommand(Action action)
    {
        CommandAction = _ => action();
    }

    /// <summary>
    /// Constructor for an action with no parameters and a value indicating if it can execute
    /// </summary>
    /// <param name="action">The action to associate with this RelayCommand</param>
    /// <param name="canExecute">Determines if the command can be executed</param>
    public RelayCommand(Action action, bool canExecute) : base(canExecute)
    {
        CommandAction = _ => action();
    }

    /// <summary>
    /// Constructor for an action with a parameter
    /// </summary>
    /// <param name="action">The action to associate with this RelayCommand</param>
    public RelayCommand(Action<object?> action)
    {
        CommandAction = action;
    }

    /// <summary>
    /// Constructor for an action with a parameter and a value indicating if it can execute
    /// </summary>
    /// <param name="action">The action to associate with this RelayCommand</param>
    /// <param name="canExecute">Determines if the command can be executed</param>
    public RelayCommand(Action<object?> action, bool canExecute) : base(canExecute)
    {
        CommandAction = action;
    }

    #endregion

    #region Public Properties

    /// <summary>
    /// The action associated with this RelayCommand
    /// </summary>
    public virtual Action<object?> CommandAction { get; }

    #endregion

    #region Public Methods

    /// <summary>
    /// Execute the command
    /// </summary>
    /// <param name="parameter">Optional parameters to pass in</param>
    public override void Execute(object? parameter = null)
    {
        if (!CanExecuteCommand)
            return;

        // Run the command
        CommandAction(parameter);
    }

    #endregion
}