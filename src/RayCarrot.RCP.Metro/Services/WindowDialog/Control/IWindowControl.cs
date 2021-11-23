using System;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// An abstract window control
/// </summary>
public interface IWindowControl : IDisposable
{
    #region Properties

    /// <summary>
    /// The dialog content
    /// </summary>
    object UIContent { get; }

    /// <summary>
    /// Indicates if the dialog should be resizable
    /// </summary>
    bool IsResizable { get; }

    /// <summary>
    /// The current window instance. This gets set by the manager when the window gets created.
    /// </summary>
    public WindowInstance WindowInstance { get; set; }

    #endregion
}