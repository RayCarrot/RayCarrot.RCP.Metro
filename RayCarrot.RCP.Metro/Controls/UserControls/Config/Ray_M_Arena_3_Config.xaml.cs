using System;
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
        /// <param name="window">The parent window</param>
        /// <param name="game">The game</param>
        public Ray_M_Arena_3_Config(Window window, Games game)
        {
            InitializeComponent();
            ParentWindow = window;

            if (game == Games.RaymanM)
                DataContext = new RaymanMConfigViewModel();

            else if (game == Games.RaymanArena)
                DataContext = new RaymanArenaConfigViewModel();

            else if (game == Games.Rayman3)
                DataContext = new Rayman3ConfigViewModel();

            else
                throw new ArgumentOutOfRangeException(nameof(game), game, $"{nameof(Ray_M_Arena_3_Config)} only supports Rayman M, Arena and 3");
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