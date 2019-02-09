using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using RayCarrot.CarrotFramework;

namespace RayCarrot.RCP.Metro
{
    // TODO: Create a file logger
    // TODO: Create a symbolic link for ubi.ini with source being in C:\Windows\Ubisoft and other being in AppData

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

    /// <summary>
    /// View model for DosBox configuration
    /// </summary>
    public class DosBoxConfigViewModel : GameConfigViewModel
    {
        #region Constructor

        /// <summary>
        /// Default constructor without default game <see cref="Games.Rayman1"/>
        /// </summary>
        public DosBoxConfigViewModel()
        {
            Game = Games.Rayman1;
        }

        /// <summary>
        /// Default constructor for a specific game
        /// </summary>
        /// <param name="game">The DosBox game</param>
        public DosBoxConfigViewModel(Games game)
        {
            Game = game;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// The DosBox game
        /// </summary>
        public Games Game { get; }

        /// <summary>
        /// The file or directory to mount
        /// </summary>
        public FileSystemPath MountPath { get; set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Loads and sets up the current configuration properties
        /// </summary>
        /// <returns>The task</returns>
        public override async Task SetupAsync()
        {
            // Get the current DosBox options for the specified game
            var options = RCFRCP.Data.DosBoxGames[Game];

            MountPath = options.MountPath;
        }

        /// <summary>
        /// Saves the changes
        /// </summary>
        /// <returns>The task</returns>
        public async Task SaveAsync()
        {
            // Get the current DosBox options for the specified game
            var options = RCFRCP.Data.DosBoxGames[Game];

            options.MountPath = MountPath;
        }

        #endregion
    }
}