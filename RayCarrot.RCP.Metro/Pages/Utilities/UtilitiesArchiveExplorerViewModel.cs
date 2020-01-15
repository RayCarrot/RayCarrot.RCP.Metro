using System.Threading.Tasks;
using System.Windows.Input;
using RayCarrot.IO;
using RayCarrot.Rayman;
using RayCarrot.UI;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// View model for the Archive Explorer utilities
    /// </summary>
    public class UtilitiesArchiveExplorerViewModel : BaseRCPViewModel
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public UtilitiesArchiveExplorerViewModel()
        {
            // Create commands
            OpenCNTArchiveExplorerCommand = new AsyncRelayCommand(OpenCNTArchiveExplorerAsync);
            OpenIPKArchiveExplorerCommand = new AsyncRelayCommand(OpenIPKArchiveExplorerAsync);

            // Set up selections
            CntGameModeSelection = new EnumSelectionViewModel<OpenSpaceGameMode>(OpenSpaceGameMode.Rayman2PC, new OpenSpaceGameMode[]
            {
                OpenSpaceGameMode.Rayman2PC,
                OpenSpaceGameMode.Rayman2PCDemo1,
                OpenSpaceGameMode.Rayman2PCDemo2,
                OpenSpaceGameMode.RaymanMPC,
                OpenSpaceGameMode.RaymanArenaPC,
                OpenSpaceGameMode.Rayman3PC,
                OpenSpaceGameMode.TonicTroublePC,
                OpenSpaceGameMode.TonicTroubleSEPC,
                OpenSpaceGameMode.DonaldDuckPC,
                OpenSpaceGameMode.PlaymobilHypePC,
            });

            IpkGameModeSelection = new EnumSelectionViewModel<UbiArtGameMode>(UbiArtGameMode.RaymanOriginsPC, new UbiArtGameMode[]
            {
                UbiArtGameMode.RaymanOriginsPC,
                UbiArtGameMode.RaymanOriginsPS3,
                UbiArtGameMode.RaymanOriginsWii,
                UbiArtGameMode.RaymanOriginsPSVita,
                UbiArtGameMode.RaymanLegendsPC,
                UbiArtGameMode.RaymanLegendsWiiU,
                UbiArtGameMode.RaymanLegendsPSVita,
                UbiArtGameMode.RaymanLegendsPS4,
                UbiArtGameMode.RaymanLegendsSwitch,
                UbiArtGameMode.RaymanAdventuresAndroid,
                UbiArtGameMode.RaymanAdventuresiOS,
                UbiArtGameMode.RaymanMiniMac,
                UbiArtGameMode.JustDance2017WiiU,
                UbiArtGameMode.ChildOfLightPC,
                UbiArtGameMode.ChildOfLightPSVita,
                UbiArtGameMode.ValiantHeartsAndroid,
                UbiArtGameMode.GravityFalls3DS,
            });
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// The game mode selection for .cnt files
        /// </summary>
        public EnumSelectionViewModel<OpenSpaceGameMode> CntGameModeSelection { get; }

        /// <summary>
        /// The game mode selection for .ipk files
        /// </summary>
        public EnumSelectionViewModel<UbiArtGameMode> IpkGameModeSelection { get; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Opens a OpenSpace .cnt file in the Archive Explorer
        /// </summary>
        /// <returns>The task</returns>
        public async Task OpenCNTArchiveExplorerAsync()
        {
            // Open the Archive Explorer with the .cnt manager
            await OpenArchiveExplorerAsync(
                new OpenSpaceCntArchiveDataManager(CntGameModeSelection.SelectedValue.GetSettings()),
                new FileFilterItem("*.cnt", "CNT").ToString(),
                CntGameModeSelection.SelectedValue.GetGame());
        }

        /// <summary>
        /// Opens a UbiArt .ipk file in the Archive Explorer
        /// </summary>
        /// <returns>The task</returns>
        public async Task OpenIPKArchiveExplorerAsync()
        {
            // Open the Archive Explorer with the .ipk manager
            await OpenArchiveExplorerAsync(
                new UbiArtIPKArchiveDataManager(IpkGameModeSelection.SelectedValue.GetSettings()),
                new FileFilterItem("*.ipk", "IPK").ToString(),
                IpkGameModeSelection.SelectedValue.GetGame());
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

        #endregion

        #region Commands

        public ICommand OpenCNTArchiveExplorerCommand { get; }

        public ICommand OpenIPKArchiveExplorerCommand { get; }

        #endregion
    }
}