using System.Windows;
using System.Windows.Controls;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Interaction logic for Ray_Origins_Legends_Config.xaml
    /// </summary>
    public partial class Ray_Origins_Legends_Config : UserControl
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="game">The game</param>
        public Ray_Origins_Legends_Config(Games game)
        {
            InitializeComponent();

            DataContext = new Ray_Origins_Legends_ConfigViewModel(game);
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