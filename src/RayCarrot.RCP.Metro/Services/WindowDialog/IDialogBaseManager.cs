namespace RayCarrot.RCP.Metro;

/// <summary>
/// A dialog base manager for managing dialogs
/// </summary>
public interface IDialogBaseManager
{
    Task<ShowWindowResult> ShowWindowAsync(
        IWindowControl windowContent,
        ShowWindowFlags flags = ShowWindowFlags.None,
        string[]? typeGroupNames = null,
        string[]? globalGroupNames = null,
        string? title = null);
}