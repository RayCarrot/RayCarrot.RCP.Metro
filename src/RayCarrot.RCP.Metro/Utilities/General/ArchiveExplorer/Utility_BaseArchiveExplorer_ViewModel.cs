using RayCarrot.IO;
using RayCarrot.UI;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Base view model for general Archive Explorer utility view models
    /// </summary>
    /// <typeparam name="GameMode">The type of game modes to support</typeparam>
    public abstract class Utility_BaseArchiveExplorer_ViewModel<GameMode> : BaseRCPViewModel
        where GameMode : Enum
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        protected Utility_BaseArchiveExplorer_ViewModel()
        {
            // Create commands
            OpenArchiveExplorerCommand = new AsyncRelayCommand(OpenArchiveExplorerAsync);
            CreateArchiveCommand = new AsyncRelayCommand(CreateArchiveAsync);
        }

        #endregion

        #region Protected Abstract Properties

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

        #region Protected Abstract Methods

        /// <summary>
        /// Gets a new archive data manager
        /// </summary>
        /// <param name="mode">The archive mode</param>
        /// <returns>The archive data manager</returns>
        protected abstract IArchiveDataManager GetArchiveDataManager(ArchiveMode mode);

        #endregion

        #region Public Methods

        /// <summary>
        /// Opens a supported file in the Archive Explorer
        /// </summary>
        /// <returns>The task</returns>
        public async Task OpenArchiveExplorerAsync()
        {
            // Open the Archive Explorer
            await OpenArchiveExplorerAsync(GetArchiveDataManager(ArchiveMode.Explorer), new FileFilterItem($"*{ArchiveFileExtension}", ArchiveFileExtension.Substring(1).ToUpper()).ToString(), GameModeSelection.SelectedValue.GetGame());
        }

        /// <summary>
        /// Opens the Archive Explorer with the specified manager
        /// </summary>
        /// <param name="manager">The manager to use</param>
        /// <param name="fileFilter">The file filter when selecting the files to open</param>
        /// <param name="game">The game to open, if available</param>
        /// <returns>The task</returns>
        public async Task OpenArchiveExplorerAsync(IArchiveDataManager manager, string fileFilter, Games? game)
        {
            // Allow the user to select the files
            var fileResult = await Services.BrowseUI.BrowseFileAsync(new FileBrowserViewModel()
            {
                Title = Resources.Utilities_ArchiveExplorer_FileSelectionHeader,
                DefaultDirectory = game?.GetInstallDir(false).FullPath,
                ExtensionFilter = fileFilter,
                MultiSelection = true
            });

            if (fileResult.CanceledByUser)
                return;

            // Show the Archive Explorer
            await Services.UI.ShowArchiveExplorerAsync(manager, fileResult.SelectedFiles.ToArray());
        }

        /// <summary>
        /// Opens the Archive Creator
        /// </summary>
        /// <returns>The task</returns>
        public async Task CreateArchiveAsync()
        {
            // Show the Archive Creator
            await Services.UI.ShowArchiveCreatorAsync(GetArchiveDataManager(ArchiveMode.Creator));
        }

        #endregion

        #region Commands

        public ICommand OpenArchiveExplorerCommand { get; }
        
        public ICommand CreateArchiveCommand { get; }

        #endregion

        #region Enums

        protected enum ArchiveMode
        {
            Explorer,
            Creator
        }

        #endregion
    }
}