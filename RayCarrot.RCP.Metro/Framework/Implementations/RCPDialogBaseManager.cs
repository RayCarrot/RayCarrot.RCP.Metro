using RayCarrot.CarrotFramework.Abstractions;
using RayCarrot.WPF;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using RayCarrot.Extensions;
using RayCarrot.UI;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// The dialog base manager for the Game Launcher
    /// </summary>
    public class RCPDialogBaseManager : IDialogBaseManager
    {
        /// <summary>
        /// Shows the dialog and returns when the dialog closes with a result
        /// </summary>
        /// <typeparam name="V">The view model type</typeparam>
        /// <typeparam name="R">The return type</typeparam>
        /// <param name="dialog">The dialog to show</param>
        /// <param name="owner">The owner window</param>
        /// <returns>The result</returns>
        public Task<R> ShowDialogAsync<V, R>(IDialogBaseControl<V, R> dialog, object owner)
            where V : UserInputViewModel
        {
            try
            {
                RCFCore.Logger?.LogInformationSource($"A dialog is being created...");

                using (dialog)
                {
                    return Application.Current.Dispatcher.Invoke(() =>
                    {
                        // Create the window
                        var window = new BaseWindow()
                        {
                            Content = dialog.DialogContent,
                            ResizeMode = dialog.Resizable ? ResizeMode.CanResize : ResizeMode.NoResize,
                            Title = dialog.ViewModel.Title ?? String.Empty,
                            SizeToContent = dialog.Resizable ? SizeToContent.Manual : SizeToContent.WidthAndHeight
                        };

                        // Set size properties
                        switch (dialog.BaseSize)
                        {
                            case DialogBaseSize.Smallest:
                                if (dialog.Resizable)
                                {
                                    window.Height = 100;
                                    window.Width = 150;
                                }

                                window.MinHeight = 100;
                                window.MinWidth = 150;

                                break;

                            case DialogBaseSize.Small:
                                if (dialog.Resizable)
                                {
                                    window.Height = 200;
                                    window.Width = 250;
                                }

                                window.MinHeight = 200;
                                window.MinWidth = 250;

                                break;

                            case DialogBaseSize.Medium:
                                if (dialog.Resizable)
                                {
                                    window.Height = 350;
                                    window.Width = 500;
                                }

                                window.MinHeight = 300;
                                window.MinWidth = 400;

                                break;

                            case DialogBaseSize.Large:
                                if (dialog.Resizable)
                                {
                                    window.Height = 475;
                                    window.Width = 750;
                                }

                                window.MinHeight = 350;
                                window.MinWidth = 500;

                                break;

                            case DialogBaseSize.Largest:
                                if (dialog.Resizable)
                                {
                                    window.Height = 600;
                                    window.Width = 900;
                                }

                                window.MinHeight = 500;
                                window.MinWidth = 650;

                                break;
                        }

                        // Set owner
                        if (owner is Window ow)
                            window.Owner = ow;
                        else if (owner is IntPtr oi)
                            new WindowInteropHelper(window).Owner = oi;
                        else
                            window.Owner = Application.Current?.Windows.Cast<Window>().FindItem(x => x.IsActive);

                        RCFCore.Logger?.LogInformationSource($"The owner window has been set to {window.Owner?.ToString() ?? "null"}");

                        // Set startup location
                        window.WindowStartupLocation = window.Owner == null ? WindowStartupLocation.CenterScreen : WindowStartupLocation.CenterOwner;

                        // Attempt to get default Window style from Framework
                        window.Style = RCF.GetService<IWPFStyle>(false)?.WindowStyle ?? window.Style;

                        void Dialog_CloseDialog(object sender, EventArgs e)
                        {
                            window.Close();
                        }

                        // Close window on request
                        dialog.CloseDialog += Dialog_CloseDialog;

                        RCFCore.Logger?.LogInformationSource($"A dialog has been created and is showing...");

                        // Show window as dialog
                        window.ShowDialog();

                        // Unsubscribe
                        dialog.CloseDialog -= Dialog_CloseDialog;

                        RCFCore.Logger?.LogInformationSource($"The dialog has closed");

                        // Return the result
                        return Task.FromResult(dialog.GetResult());
                    });
                }
            }
            catch (Exception ex)
            {
                ex.HandleError($"Showing dialog of type {dialog.GetType().Name}");
                throw;
            }
        }
    }
}