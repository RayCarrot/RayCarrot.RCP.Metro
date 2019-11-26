using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Markup;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Interaction logic for OverflowMenu.xaml
    /// </summary>
    public partial class OverflowMenu : Button
    {
        #region Constructor

        public OverflowMenu()
        {
            InitializeComponent();

            InheritDataContext = true;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Indicates if the data context of the menu should inherit from this control
        /// </summary>
        public bool InheritDataContext { get; set; }

        #endregion

        #region Event Handlers

        private void OverflowMenu_OnClick(object sender, RoutedEventArgs e)
        {
            // Make sure we have a context menu
            if (ContextMenu == null)
                return;

            // Set the placement
            ContextMenu.Placement = PlacementMode.Absolute;

            if (InheritDataContext)
                // Set the data context
                ContextMenu.DataContext = DataContext;

            // Subscribe to when loaded
            ContextMenu.Loaded += ContextMenu_Loaded;

            // Open the overflow menu
            ContextMenu.IsOpen = true;
        }

        private void ContextMenu_Loaded(object sender, RoutedEventArgs e)
        {
            // Get the point on screen
            var point = PointToScreen(new Point(0 - ContextMenu.ActualWidth + ActualWidth, ActualHeight));

            // Set the offsets
            ContextMenu.HorizontalOffset = point.X - Margin.Left;
            ContextMenu.VerticalOffset = point.Y + Margin.Bottom;

            // Unsubscribe
            ContextMenu.Loaded -= ContextMenu_Loaded;
        }

        private void OverflowMenu_OnContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            // Cancel opening
            e.Handled = true;
        }

        #endregion
    }
}