using System;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using RayCarrot.CarrotFramework.Abstractions;
using RayCarrot.IO;
using RayCarrot.Rayman;
using RayCarrot.UI;
using RayCarrot.WPF;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// View model for the work in process utilities
    /// </summary>
    public class UtilitiesWIPViewModel : BaseRCPViewModel
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public UtilitiesWIPViewModel()
        {
            // Create commands
            LoadR1LevelCommand = new AsyncRelayCommand(LoadR1LevelAsync);

            // Set up selections
            LevGameModeSelection = new EnumSelectionViewModel<Rayman1GameMode>(Rayman1GameMode.Rayman1PC, new Rayman1GameMode[]
            {
                Rayman1GameMode.Rayman1PC,
            });
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// The game mode selection for .lev files
        /// </summary>
        public EnumSelectionViewModel<Rayman1GameMode> LevGameModeSelection { get; }

        /// <summary>
        /// The currently loaded image source for a .lev graphic
        /// </summary>
        public ImageSource LevGraphicsImageSource { get; set; }

        /// <summary>
        /// The currently loaded image source for a .lev type graphic
        /// </summary>
        public ImageSource LevTypesImageSource { get; set; }

        /// <summary>
        /// The currently loaded .lev data
        /// </summary>
        public Rayman1LevData LevData { get; set; }

        #endregion

        #region Public Methods

        public async Task LoadR1LevelAsync()
        {
            // Allow the user to select the level file
            var fileResult = await RCFUI.BrowseUI.BrowseFileAsync(new FileBrowserViewModel()
            {
                // TODO: Localize
                Title = "Select level file",
                DefaultDirectory = LevGameModeSelection.SelectedValue.GetGame()?.GetInstallDir(false).FullPath,
                // TODO: Localize
                ExtensionFilter = new FileFilterItem("*.lev", "Level").ToString(),
                MultiSelection = true
            });

            if (fileResult.CanceledByUser)
                return;

            try
            {
                // Get the level data
                LevData = new Rayman1LevSerializer(new Rayman1Settings(LevGameModeSelection.SelectedValue)).Deserialize(fileResult.SelectedFile);

                LevGraphicsImageSource = LevData.GetBitmap().ToImageSource();
                LevTypesImageSource = LevData.GetTypeBitmap().ToImageSource();
            }
            catch (Exception ex)
            {
                ex.HandleError("Loading R1 level map");

                // TODO: Error message
            }
        }

        #endregion

        #region Commands

        public ICommand LoadR1LevelCommand { get; }

        #endregion
    }
}