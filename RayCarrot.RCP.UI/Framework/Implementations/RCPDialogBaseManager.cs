using RayCarrot.WPF;
using System.Windows;

namespace RayCarrot.RCP.UI
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
            return new BaseWindow();
        }
    }
}