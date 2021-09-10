using System;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Used for window controls
    /// </summary>
    /// <typeparam name="VM">The view model type</typeparam>
    public interface IWindowBaseControl<out VM> : IDisposable
        where VM : UserInputViewModel
    {
        #region Properties

        /// <summary>
        /// The view model
        /// </summary>
        VM ViewModel { get; }

        /// <summary>
        /// The dialog content
        /// </summary>
        object UIContent { get; }

        /// <summary>
        /// Indicates if the dialog should be resizable
        /// </summary>
        bool Resizable { get; }

        /// <summary>
        /// The base size for the dialog
        /// </summary>
        DialogBaseSize BaseSize { get; }

        #endregion

        #region Events

        /// <summary>
        /// Invoke to request the dialog to close
        /// </summary>
        event EventHandler CloseDialog;

        #endregion
    }
}