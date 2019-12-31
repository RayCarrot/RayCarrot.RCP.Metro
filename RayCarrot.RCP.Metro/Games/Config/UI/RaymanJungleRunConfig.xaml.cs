using RayCarrot.WPF;
using System.Windows;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Interaction logic for RaymanJungleRunConfig.xaml
    /// </summary>
    public partial class RaymanJungleRunConfig : VMUserControl<RaymanJungleRunConfigViewModel>
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public RaymanJungleRunConfig()
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