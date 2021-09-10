using System.Windows;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// The dialog base manager for the Rayman Control Panel
    /// </summary>
    public class RCPDialogBaseManager : WindowDialogBaseManager
    {
        /// <summary>
        /// Gets a new instance of a window
        /// </summary>
        /// <returns>The window instance</returns>
        public override Window GetWindow()
        {
            // Return a base window for this application
            return new BaseWindow();
        }
    }
}