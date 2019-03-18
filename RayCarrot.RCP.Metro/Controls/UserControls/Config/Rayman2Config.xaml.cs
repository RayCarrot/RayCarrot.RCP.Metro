using System.Windows;
using System.Windows.Input;
using MahApps.Metro.Controls;
using RayCarrot.WPF;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Interaction logic for Rayman2Config.xaml
    /// </summary>
    public partial class Rayman2Config : VMUserControl<Rayman2ConfigViewModel>
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="window">The parent window</param>
        public Rayman2Config(Window window)
        {
            InitializeComponent();
            ParentWindow = window;
        }

        #endregion

        #region Private Properties

        /// <summary>
        /// The parent window
        /// </summary>
        private Window ParentWindow { get; }

        #endregion

        #region Event Handlers

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            ParentWindow.Close();
        }

        private void UIElement_OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            // Temporary solution for scrolling over data grid
            scrollViewer.ScrollToVerticalOffset(scrollViewer.ContentVerticalOffset - (e.Delta / 2d));
        }

        private void UIElement_OnKeyDown(object sender, KeyEventArgs e)
        {
            ((HotKeyBox)sender).HotKey = new HotKey(e.Key);
        }

        #endregion
    }
}