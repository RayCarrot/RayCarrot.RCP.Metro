using System;
using RayCarrot.Common;

namespace RayCarrot.RCP.Metro
{
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
        public DialogMessageActionHandledEventArgs(object actionResult, bool shouldCloseDialog)
        {
            ActionResult = actionResult;
            ShouldCloseDialog = shouldCloseDialog;
        }

        #endregion

        #region Properties

        /// <summary>
        /// The action result
        /// </summary>
        public object ActionResult { get; }

        /// <summary>
        /// True if the dialog should close when this action is handled
        /// </summary>
        public bool ShouldCloseDialog { get; }

        #endregion
    }

    /// <summary>
    /// Event arguments for when a dialog message action is handled
    /// </summary>
    public class DialogMessageActionHandledEventArgs<T> : DialogMessageActionHandledEventArgs
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="actionResult">The action result</param>
        /// <param name="shouldCloseDialog">True if the dialog should close when this action is handled</param>
        public DialogMessageActionHandledEventArgs(T actionResult, bool shouldCloseDialog) : base(actionResult, shouldCloseDialog)
        {
        }

        #endregion

        #region Properties

        /// <summary>
        /// The action result
        /// </summary>
        public new T ActionResult => base.ActionResult.CastTo<T>();

        #endregion
    }
}