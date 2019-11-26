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
        public RaymanRavingRabbidsConfig()
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