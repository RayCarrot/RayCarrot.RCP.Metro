using RayCarrot.UI;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// A <see cref="UserControl"/> with view model support
    /// </summary>
    /// <typeparam name="VM">The view model type</typeparam>
    public class VMUserControl<VM> : UserControl
        where VM : BaseViewModel, new()
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public VMUserControl()
        {
            if (DesignerProperties.GetIsInDesignMode(this))
                return;

            ViewModel = new VM();

            Loaded += VMUserControl_Loaded;
        }

        /// <summary>
        /// Constructor for passing in a view model instance
        /// </summary>
        /// <param name="instance">The instance of the view model to use</param>
        public VMUserControl(VM instance)
        {
            ViewModel = instance;

            Loaded += VMUserControl_Loaded;
        }

        /// <summary>
        /// The page view model
        /// </summary>
        public VM ViewModel
        {
            get => DataContext as VM;
            set => DataContext = value;
        }

        private void VMUserControl_Loaded(object sender, RoutedEventArgs e)
        {
            // Attempt to get parent window
            var window = Window.GetWindow(this);

            if (window == null)
                return;

            window.Closed += (ss, ee) => (ViewModel as IDisposable)?.Dispose();
        }
    }
}