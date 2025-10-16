using System.Windows.Media;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// View model for a dialog message
/// </summary>
public class DialogMessageViewModel : UserInputViewModel
{
    public DialogMessageViewModel(
        string messageText,
        MessageType messageType,
        IList<DialogMessageActionViewModel> dialogActions,
        UserInputResult defaultActionResult,
        ImageSource dialogImageSource)
    {
        MessageText = messageText;
        MessageType = messageType;
        DialogActions = dialogActions;
        DefaultActionResult = defaultActionResult;
        DialogImageSource = dialogImageSource;
    }

    /// <summary>
    /// The message text
    /// </summary>
    public string MessageText { get; }

    /// <summary>
    /// The message type
    /// </summary>
    public MessageType MessageType { get; }

    /// <summary>
    /// The dialog actions
    /// </summary>
    public IList<DialogMessageActionViewModel> DialogActions { get; }

    /// <summary>
    /// The default action result
    /// </summary>
    public UserInputResult DefaultActionResult { get; }

    /// <summary>
    /// The dialog image source
    /// </summary>
    public ImageSource DialogImageSource { get; }
}