#nullable disable
using System;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// Event arguments for when a dialog message action is handled
/// </summary>
public class DialogMessageActionHandledEventArgs : EventArgs
{
    #region Constructor

    /// <summary>
    /// Default constructor
    /// </summary>
    /// <param name="actionResult">The action result</param>
    /// <param name="shouldCloseDialog">True if the dialog should close when this action is handled</param>
    public DialogMessageActionHandledEventArgs(UserInputResult actionResult, bool shouldCloseDialog)
    {
        ActionResult = actionResult;
        ShouldCloseDialog = shouldCloseDialog;
    }

    #endregion

    #region Properties

    /// <summary>
    /// The action result
    /// </summary>
    public UserInputResult ActionResult { get; }

    /// <summary>
    /// True if the dialog should close when this action is handled
    /// </summary>
    public bool ShouldCloseDialog { get; }

    #endregion
}

/// <summary>
/// Event arguments for when a dialog message action is handled
/// </summary>
public class DialogMessageActionHandledEventArgs<Result> : DialogMessageActionHandledEventArgs
    where Result : UserInputResult
{
    #region Constructor

    /// <summary>
    /// Default constructor
    /// </summary>
    /// <param name="actionResult">The action result</param>
    /// <param name="shouldCloseDialog">True if the dialog should close when this action is handled</param>
    public DialogMessageActionHandledEventArgs(Result actionResult, bool shouldCloseDialog) : base(actionResult, shouldCloseDialog)
    {
    }

    #endregion

    #region Properties

    /// <summary>
    /// The action result
    /// </summary>
    public new Result ActionResult => base.ActionResult.CastTo<Result>();

    #endregion
}