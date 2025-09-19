using System.Windows.Input;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// View model for a dialog message action
/// </summary>
public class DialogMessageActionViewModel : BaseViewModel
{
    #region Properties

    /// <summary>
    /// The display text
    /// </summary>
    public string DisplayText { get; set; }

    /// <summary>
    /// The display description
    /// </summary>
    public string DisplayDescription { get; set; }

    /// <summary>
    /// The action result
    /// </summary>
    public UserInputResult ActionResult { get; set; }

    /// <summary>
    /// True if this is the default action
    /// </summary>
    public bool IsDefault { get; set; }

    /// <summary>
    /// True if this is the default cancel action
    /// </summary>
    public bool IsCancel { get; set; }

    /// <summary>
    /// True if the dialog should close when this action is handled
    /// </summary>
    public bool ShouldCloseDialog { get; set; } = true;

    /// <summary>
    /// Optional action for when this action is handled
    /// </summary>
    public Action OnHandled { get; set; }

    #endregion

    #region Commands

    private ICommand _ActionCommand;

    /// <summary>
    /// Command for when the user selects this action
    /// </summary>
    public ICommand ActionCommand => _ActionCommand ??= new RelayCommand(HandleAction);

    #endregion

    #region Public Methods

    /// <summary>
    /// Handles that the action was chosen by the user
    /// </summary>
    public virtual void HandleAction()
    {
        // Invoke optional action
        OnHandled?.Invoke();

        // Invoke event
        ActionHandled?.Invoke(this, new DialogMessageActionHandledEventArgs(ActionResult, ShouldCloseDialog));
    }

    #endregion

    #region Events

    /// <summary>
    /// Occurs when the action is chosen by the user
    /// </summary>
    public event EventHandler<DialogMessageActionHandledEventArgs> ActionHandled;

    #endregion
}