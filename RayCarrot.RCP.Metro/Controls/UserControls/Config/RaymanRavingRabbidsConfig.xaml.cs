using RayCarrot.WPF;
using System.Windows;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Interaction logic for RaymanRavingRabbidsConfig.xaml
    /// </summary>
    public partial class RaymanRavingRabbidsConfig : VMUserControl<RaymanRavingRabbidsConfigViewModel>
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="window">The parent window</param>
        public RaymanRavingRabbidsConfig(Window window)
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

        #endregion
    }
}