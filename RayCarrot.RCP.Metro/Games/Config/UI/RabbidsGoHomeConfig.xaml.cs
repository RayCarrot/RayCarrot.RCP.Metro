using System.Windows;
using System.Windows.Controls;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Interaction logic for RabbidsGoHomeConfig.xaml
    /// </summary>
    public partial class RabbidsGoHomeConfig : UserControl
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public RabbidsGoHomeConfig()
        {
            InitializeComponent();
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