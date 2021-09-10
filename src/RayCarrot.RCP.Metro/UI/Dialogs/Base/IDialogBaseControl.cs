using System;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Used for dialog controls for <see cref="IDialogBaseControl{V, R}"/>
    /// </summary>
    /// <typeparam name="VM">The view model type</typeparam>
    /// <typeparam name="R">The result type</typeparam>
    public interface IDialogBaseControl<out VM, out R> : IDisposable, IWindowBaseControl<VM>
        where VM : UserInputViewModel
    {
        #region Methods

        /// <summary>
        /// Gets the current result
        /// </summary>
        /// <returns>The result</returns>
        R GetResult();

        #endregion
    }
}