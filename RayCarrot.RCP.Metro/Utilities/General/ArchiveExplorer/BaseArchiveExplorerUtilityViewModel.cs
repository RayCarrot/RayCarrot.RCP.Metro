using System;
using System.Threading.Tasks;
using System.Windows.Input;
using RayCarrot.IO;
using RayCarrot.UI;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Base view model for general Archive Explorer utility view models
    /// </summary>
    /// <typeparam name="GameMode">The type of game modes to support</typeparam>
    public abstract class BaseArchiveExplorerUtilityViewModel<GameMode> : BaseRCPViewModel
        where GameMode : Enum
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        protected BaseArchiveExplorerUtilityViewModel()
        {
            // Create commands
            OpenArchiveExplorerCommand = new AsyncRelayCommand(OpenArchiveExplorerAsync);
            CreateArchiveCommand = new AsyncRelayCommand(CreateArchiveAsync);
        }

        #endregion

        #region Protected Abstract Properties

        /// <summary>
        /// Gets a new archive explorer data manager
        /// </summary>
        protected abstract IArchiveExplorerDataManager GetArchiveExplorerDataManager { get; }

        /// <summary>
        /// Gets a new archive creator data manager
        /// </summary>
        protected abstract IArchiveCreatorDataManager GetArchiveCreatorDataManager { get; }

        /// <summary>
        /// The file extension for the archive
        /// </summary>
        public abstract string ArchiveFileExtension { get; }

        #endregion

        #region Public Abstract Properties

        /// <summary>
        /// The game mode selection
        /// </summary>
        public abstract EnumSelectionViewModel<GameMode> GameModeSelection { get; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Opens a supported file in the Archive Explorer
        /// </summary>
        /// <returns>The task</returns>
        public async Task OpenArchiveExplorerAsync()
        {
            // Open the Archive Explorer
            await OpenArchiveExplorerAsync(GetArchiveExplorerDataManager, new FileFilterItem($"*{ArchiveFileExtension}", ArchiveFileExtension.Substring(1).ToUpper()).ToString(), GameModeSelection.SelectedValue.GetGame());
        }

        /// <summary>
        /// Opens the Archive Explorer with the specified manager
        /// </summary>
        /// <param name="manager">The manager to use</param>
        /// <param name="fileFilter">The file filter when selecting the files to open</param>
        /// <param name="game">The game to open, if available</param>
        /// <returns>The task</returns>
        public async Task OpenArchiveExplorerAsync(IArchiveExplorerDataManager manager, string fileFilter, Games? game)
        {
            // Allow the user to select the files
            var fileResult = await RCFUI.BrowseUI.BrowseFileAsync(new FileBrowserViewModel()
            {
                Title = Resources.Utilities_ArchiveExplorer_FileSelectionHeader,
                DefaultDirectory = game?.GetInstallDir(false).FullPath,
                ExtensionFilter = fileFilter,
                MultiSelection = true
            });

            if (fileResult.CanceledByUser)
                return;

            // Show the Archive Explorer
            await RCFRCP.UI.ShowArchiveExplorerAsync(manager, fileResult.SelectedFiles);
        }

        /// <summary>
        /// Opens the Archive Creator
        /// </summary>
        /// <returns>The task</returns>
        public async Task CreateArchiveAsync()
        {
            // Show the Archive Creator
            await RCFRCP.UI.ShowArchiveCreatorAsync(GetArchiveCreatorDataManager);
        }

        #endregion

        #region Commands

        public ICommand OpenArchiveExplorerCommand { get; }
        
        public ICommand CreateArchiveCommand { get; }

        #endregion
    }
}