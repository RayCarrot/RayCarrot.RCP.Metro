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
        /// <param name="window">The parent window</param>
        /// <param name="game">The game</param>
        public Ray_Origins_Legends_Config(Window window, Games game)
        {
            InitializeComponent();
            ParentWindow = window;

            DataContext = new Ray_Origins_Legends_ConfigViewModel(game);
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