using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;

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
        }

        #endregion

        #region Event Handlers

        private void OverflowMenu_OnClick(object sender, RoutedEventArgs e)
        {
            if (Menu != null)
                // Open the overflow menu
                Menu.IsOpen = true;
        }

        private void OldMenu_Loaded(object sender, RoutedEventArgs e)
        {
            // Get the point on screen
            var point = PointToScreen(new Point(0 - Menu.ActualWidth + ActualWidth, ActualHeight));

            // Set the offsets
            Menu.HorizontalOffset = point.X - Margin.Left;
            Menu.VerticalOffset = point.Y + Margin.Bottom;
        }

        #endregion

        #region Dependency Properties

        /// <summary>
        /// The menu
        /// </summary>
        public ContextMenu Menu
        {
            get => (ContextMenu)GetValue(MenuProperty);
            set => SetValue(MenuProperty, value);
        }

        public static readonly DependencyProperty MenuProperty = DependencyProperty.Register(nameof(Menu), typeof(ContextMenu), typeof(OverflowMenu), new PropertyMetadata(null, PropertyChangedCallback));

        #endregion

        #region Static Event Handlers

        private static void PropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is OverflowMenu instance))
                return;

            // Unsubscribe old value
            if (e.OldValue is ContextMenu oldMenu)
                oldMenu.Loaded -= instance.OldMenu_Loaded;

            if (e.NewValue is ContextMenu newMenu)
            {
                // Set the placement
                newMenu.Placement = PlacementMode.Absolute;

                // Subscribe to when loaded
                newMenu.Loaded += instance.OldMenu_Loaded;
            }
        }

        #endregion
    }
}