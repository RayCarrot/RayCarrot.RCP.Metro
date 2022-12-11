namespace RayCarrot.RCP.Metro;

/// <summary>
/// A RelayCommand to use for async Tasks
/// </summary>
public class AsyncRelayCommand : BaseRelayCommand
{
    #region Constructors

    /// <summary>
    /// Constructor for a function with no parameters
    /// </summary>
    /// <param name="func">The function to associate with this RelayCommand</param>
    public AsyncRelayCommand(Func<Task> func)
    {
        CommandFunction = async _ => await func();
    }

    /// <summary>
    /// Constructor for a function with no parameters and a value indicating if it can execute
    /// </summary>
    /// <param name="func">The function to associate with this RelayCommand</param>
    /// <param name="canExecute">Determines if the command can be executed</param>
    public AsyncRelayCommand(Func<Task> func, bool canExecute) : base(canExecute)
    {
        CommandFunction = async _ => await func();
    }

    /// <summary>
    /// Constructor for a function with a parameter
    /// </summary>
    /// <param name="func">The function to associate with this RelayCommand</param>
    public AsyncRelayCommand(Func<object?, Task> func)
    {
        CommandFunction = func;
    }

    /// <summary>
    /// Constructor for a function with a parameter and a value indicating if it can execute
    /// </summary>
    /// <param name="func">The function to associate with this RelayCommand</param>
    /// <param name="canExecute">Determines if the command can be executed</param>
    public AsyncRelayCommand(Func<object?, Task> func, bool canExecute) : base(canExecute)
    {
        CommandFunction = func;
    }

    #endregion

    #region Public Properties

    /// <summary>
    /// The function associated with this RelayCommand
    /// </summary>
    public virtual Func<object?, Task> CommandFunction { get; }

    #endregion

    #region Public Methods

    /// <summary>
    /// Execute the command
    /// </summary>
    /// <param name="parameter">Optional parameters to pass in</param>
    public override async void Execute(object? parameter = null) => await ExecuteAsync(parameter);

    /// <summary>
    /// Execute the command async
    /// </summary>
    /// <param name="parameter">Optional parameters to pass in</param>
    /// <returns>The task</returns>
    public async Task ExecuteAsync(object? parameter = null)
    {
        // Do not run if the command can not execute
        if (!CanExecuteCommand)
            return;

        // The command can not execute while running
        CanExecuteCommand = false;

        try
        {
            // Run the command
            await CommandFunction(parameter);
        }
        finally
        {
            // Flag that the command can now execute again
            CanExecuteCommand = true;
        }
    }

    #endregion
}