using System.Windows.Input;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// A base RelayCommand to use for RelayCommands
/// </summary>
public abstract class BaseRelayCommand : ICommand
{
    #region Constructors

    /// <summary>
    /// Default constructor
    /// </summary>
    protected BaseRelayCommand()
    {
        _canExecute = true;
    }

    /// <summary>
    /// Default constructor
    /// </summary>
    /// <param name="canExecute">Determines if the command can be executed</param>
    protected BaseRelayCommand(bool canExecute)
    {
        _canExecute = canExecute;
    }

    #endregion

    #region Private Fields

    /// <summary>
    /// Determines if the command can execute
    /// </summary>
    private bool _canExecute;

    #endregion

    #region Public Properties

    /// <summary>
    /// True if the command can execute, false if not
    /// </summary>
    public virtual bool CanExecuteCommand
    {
        get => _canExecute;
        set => SetCanExecute(value);
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Changes the state determining if the command can execute
    /// </summary>
    /// <param name="value">True if the command can execute</param>
    public virtual void SetCanExecute(bool value)
    {
        _canExecute = value;
        CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Get a boolean determining if the command can execute
    /// </summary>
    /// <param name="parameter">Optional parameter to pass in</param>
    /// <returns></returns>
    public virtual bool CanExecute(object? parameter = null) => _canExecute;

    /// <summary>
    /// Execute the command
    /// </summary>
    /// <param name="parameter">Optional parameters to pass in</param>
    public abstract void Execute(object? parameter = null);

    #endregion

    #region Events

    /// <summary>
    /// Fires when the state determining if the command can execute has changed
    /// </summary>
    public event EventHandler? CanExecuteChanged;

    #endregion
}