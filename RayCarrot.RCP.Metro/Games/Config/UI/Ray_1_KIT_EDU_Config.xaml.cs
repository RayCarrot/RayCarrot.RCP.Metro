using System.Windows;
using System.Windows.Controls;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Interaction logic for Ray_1_KIT_EDU_Config.xaml
    /// </summary>
    public partial class Ray_1_KIT_EDU_Config : UserControl
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="viewModel">The view model</param>
        public Ray_1_KIT_EDU_Config(Ray_1_KIT_EDU_BaseConfigViewModel viewModel)
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