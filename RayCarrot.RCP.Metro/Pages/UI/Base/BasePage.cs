using System.Windows.Controls;
using RayCarrot.WPF;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// The base for a page control
    /// </summary>
    /// <typeparam name="VM">The view model type</typeparam>
    public class BasePage<VM> : VMUserControl<VM>, IBasePage
        where VM : BaseRCPViewModel, new()
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public BasePage()
        { }

        /// <summary>
        /// Constructor for passing in a view model instance
        /// </summary>
        /// <param name="instance">The instance of the view model to use</param>
        public BasePage(VM instance) : base(instance)
        { }

        /// <summary>
        /// The overflow menu
        /// </summary>
        public ContextMenu OverflowMenu { get; set; }
    }
}