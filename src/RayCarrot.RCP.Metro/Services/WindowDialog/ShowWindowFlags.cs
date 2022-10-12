#nullable disable
using System;

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
}