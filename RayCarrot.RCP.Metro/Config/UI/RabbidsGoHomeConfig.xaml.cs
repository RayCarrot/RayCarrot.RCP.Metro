using RayCarrot.WPF;
using System.Windows;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Interaction logic for RabbidsGoHomeConfig.xaml
    /// </summary>
    public partial class RabbidsGoHomeConfig : VMUserControl<RabbidsGoHomeConfigViewModel>
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