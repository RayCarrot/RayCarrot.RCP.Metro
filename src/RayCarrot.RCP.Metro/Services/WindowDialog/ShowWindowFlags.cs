#nullable disable
namespace RayCarrot.RCP.Metro;

/// <summary>
/// The available flags to use on <see cref="IDialogBaseManager.ShowWindowAsync"/>
/// </summary>
[Flags]
public enum ShowWindowFlags
{
    /// <summary>
    /// No flags
    /// </summary>
    None = 0,

    /// <summary>
    /// Indicates that several windows using the same content type are not allowed
    /// </summary>
    DuplicateTypesNotAllowed = 1 << 0,

    /// <summary>
    /// Indicates that the blocking window preventing the current one from being shown should not be focused
    /// </summary>
    DoNotFocusBlockingWindow = 1 << 1,

    /// <summary>
    /// Indicates if the window should be shown as a modal window and thus block other windows
    /// </summary>
    Modal = 1 << 2,
}