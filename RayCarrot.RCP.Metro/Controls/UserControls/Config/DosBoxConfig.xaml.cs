using System.Windows;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Interaction logic for DosBoxConfig.xaml
    /// </summary>
    public partial class DosBoxConfig : BaseUserControl<DosBoxConfigViewModel>
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="window">The parent window</param>
        /// <param name="game">The DosBox game</param>
        public DosBoxConfig(Window window, Games game) : base(new DosBoxConfigViewModel(game))
        {
            InitializeComponent();
            ParentWindow = window;
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