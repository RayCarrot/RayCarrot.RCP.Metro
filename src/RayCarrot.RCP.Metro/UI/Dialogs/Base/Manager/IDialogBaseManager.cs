using System.Threading.Tasks;
using System.Windows;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// A dialog base manager for managing dialogs
    /// </summary>
    public interface IDialogBaseManager
    {
        /// <summary>
        /// Shows the dialog and returns when the dialog closes with a result
        /// </summary>
        /// <typeparam name="VM">The view model type</typeparam>
        /// <typeparam name="R">The return type</typeparam>
        /// <param name="dialog">The dialog to show</param>
        /// <param name="owner">The owner window</param>
        /// <returns>The result</returns>
        Task<R> ShowDialogAsync<VM, R>(IDialogBaseControl<VM, R> dialog, object owner)
            where VM : UserInputViewModel;

        /// <summary>
        /// Shows the Window without waiting for it to close
        /// </summary>
        /// <typeparam name="VM">The view model</typeparam>
        /// <param name="windowContent">The window content to show</param>
        /// <param name="owner">The owner window</param>
        /// <returns>The window</returns>
        Task<Window> ShowWindowAsync<VM>(IWindowBaseControl<VM> windowContent, object owner)
            where VM : UserInputViewModel;
    }
}