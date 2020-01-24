using System.Windows;
using System.Windows.Controls;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Interaction logic for Ray_M_Arena_3_Config.xaml
    /// </summary>
    public partial class Ray_M_Arena_3_Config : UserControl
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="viewModel">The game config view model</param>
        public Ray_M_Arena_3_Config(GameConfigViewModel viewModel)
        {
            InitializeComponent();

            DataContext = viewModel;
        }

        #endregion

        #region Event Handlers

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            App.Current.CurrentActiveWindow.Close();
        }

        #endregion
    }
}