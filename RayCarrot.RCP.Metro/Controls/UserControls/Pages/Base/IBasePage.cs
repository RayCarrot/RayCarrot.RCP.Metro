using System.Windows.Controls;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Used to define the properties of a base page
    /// </summary>
    public interface IBasePage
    {
        /// <summary>
        /// The overflow menu
        /// </summary>
        ContextMenu OverflowMenu { get; set; }
    }
}