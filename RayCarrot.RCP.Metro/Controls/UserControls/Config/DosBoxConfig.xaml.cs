using System;
using System.Windows;
using System.Windows.Controls;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Interaction logic for DosBoxConfig.xaml
    /// </summary>
    public partial class DosBoxConfig : UserControl
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="game">The DosBox game</param>
        public DosBoxConfig(Games game)
        {
            InitializeComponent();

            if (game == Games.Rayman1)
                DataContext = new Rayman1ConfigViewModel();

            else if (game == Games.RaymanDesigner | game == Games.RaymanByHisFans || game == Games.Rayman60Levels)
                DataContext = new RaymanDesignerConfigViewModel(game);

            else if (game == Games.EducationalDos)
                DataContext = new EducationalDosBoxGameConfigViewModel();

            else
                throw new ArgumentOutOfRangeException(nameof(game), game, $"{nameof(DosBoxConfig)} only supports Rayman 1, Rayman Designer, Rayman by his Fans, Rayman 60 Levels and Educational DOS Games");
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