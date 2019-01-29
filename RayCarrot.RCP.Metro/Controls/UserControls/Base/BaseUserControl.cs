using System.ComponentModel;
using System.Windows.Controls;
using RayCarrot.CarrotFramework;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// A base user control to inherit from
    /// </summary>
    /// <typeparam name="VM">The view model type</typeparam>
    public class BaseUserControl<VM> : UserControl
        where VM : BaseViewModel, new()
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public BaseUserControl()
        {
            if (DesignerProperties.GetIsInDesignMode(this))
                return;

            ViewModel = new VM();
        }

        /// <summary>
        /// Constructor for passing in a view model instance
        /// </summary>
        /// <param name="instance">The instance of the view model to use</param>
        public BaseUserControl(VM instance)
        {
            ViewModel = instance;
        }

        /// <summary>
        /// The page view model
        /// </summary>
        public VM ViewModel
        {
            get => DataContext as VM;
            set => DataContext = value;
        }
    }
}