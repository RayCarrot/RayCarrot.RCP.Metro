using MahApps.Metro.Controls;
using MahApps.Metro.SimpleChildWindow;
using System.Threading.Tasks;

namespace RayCarrot.RCP.Metro
{
    public class RCPWindowDialogBaseManager : WindowDialogBaseManager
    {
        #region Protected Methods

        protected void ConfigureChildWindow(RCPChildWindow window, IWindowControl windowContent, bool isModal)
        {
            // Set window properties
            window.Content = windowContent.UIContent;
            window.IsModal = isModal;
            window.CanMaximize = windowContent.IsResizable;
        }

        protected override Task ShowAsync(IWindowControl windowContent, bool isModal, string title)
        {
            // Show as a child window
            if (Services.Data.UI_UseChildWindows && App.Current?.ChildWindowsParent is MetroWindow metroWindow)
            {
                // Create the child window
                var childWin = new RCPChildWindow();

                // Configure the window
                ConfigureChildWindow(childWin, windowContent, isModal);

                // Set the window instance
                windowContent.WindowInstance = new ChildWindowInstance(childWin);

                // Set the title
                if (title != null)
                    windowContent.WindowInstance.Title = title;

                // Show the window
                return metroWindow.ShowChildWindowAsync(childWin);
            }
            // or show as a normal window
            else
            {
                return base.ShowAsync(windowContent, isModal, title);
            }
        }

        #endregion
    }
}