using System.Linq;
using MahApps.Metro.Controls;
using System.Windows.Controls;
using System.Windows.Input;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Interaction logic for Rayman2Config.xaml
    /// </summary>
    public partial class Rayman2Config : UserControl
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public Rayman2Config()
        {
            InitializeComponent();
            Loaded += (s, e) => ScrollViewer = this.GetAncestors().FirstOrDefault(x => x is ScrollViewer) as ScrollViewer;
        }

        #endregion

        #region Private Properties

        private ScrollViewer ScrollViewer { get; set; }

        #endregion

        #region Event Handlers

        private void UIElement_OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            // Temporary solution for scrolling over data grid
            ScrollViewer?.ScrollToVerticalOffset(ScrollViewer.ContentVerticalOffset - (e.Delta / 2d));
        }

        private void UIElement_OnKeyDown(object sender, KeyEventArgs e)
        {
            ((HotKeyBox)sender).HotKey = new HotKey(e.Key);
        }

        #endregion
    }
}