using RayCarrot.WPF;
using System.Windows;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Interaction logic for RaymanFiestaRunConfig.xaml
    /// </summary>
    public partial class RaymanFiestaRunConfig : VMUserControl<RaymanFiestaRunConfigViewModel>
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public RaymanFiestaRunConfig()
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