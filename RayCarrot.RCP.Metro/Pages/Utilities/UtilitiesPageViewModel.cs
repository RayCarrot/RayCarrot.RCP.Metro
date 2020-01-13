using RayCarrot.Extensions;
using RayCarrot.IO;
using RayCarrot.Rayman;
using RayCarrot.UI;
using RayCarrot.WPF;
using System;
using System.Drawing.Imaging;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// View model for the utilities page
    /// </summary>
    public class UtilitiesPageViewModel : BaseRCPViewModel
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public UtilitiesPageViewModel()
        {
            // Create commands
            OpenCNTArchiveExplorerCommand = new AsyncRelayCommand(OpenCNTArchiveExplorerAsync);
            OpenIPKArchiveExplorerCommand = new AsyncRelayCommand(OpenIPKArchiveExplorerAsync);
            ConvertFromGFCommand = new AsyncRelayCommand(ConvertFromGFAsync);
            LoadR1LevelCommand = new AsyncRelayCommand(LoadR1LevelAsync);

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
                UbiArtGameMode.RaymanLegendsPC,
                UbiArtGameMode.RaymanAdventuresAndroid,
                UbiArtGameMode.RaymanMiniMac,
                UbiArtGameMode.JustDance2017WiiU,
            });

            // TODO: Support more games + Fiesta Run
            LocGameModeSelection = new EnumSelectionViewModel<UbiArtGameMode>(UbiArtGameMode.RaymanOriginsPC, new []
            {
                UbiArtGameMode.RaymanOriginsPC,
                UbiArtGameMode.RaymanLegendsPC,
                UbiArtGameMode.RaymanAdventuresAndroid,
                UbiArtGameMode.RaymanMiniMac,
            });

            GfGameModeSelection = new EnumSelectionViewModel<OpenSpaceGameMode>(OpenSpaceGameMode.Rayman2PC, OpenSpaceGameMode.Rayman2PC.GetValues());

            LevGameModeSelection = new EnumSelectionViewModel<Rayman1GameMode>(Rayman1GameMode.Rayman1PC, new Rayman1GameMode[]
            {
                Rayman1GameMode.Rayman1PC,
            });

            // TODO: Allow batch export?
            //_ = Task.Run(() =>
            //{
            //    FileSystemPath baseOutDir = @"C:\Users\RayCarrot\Downloads\CNT Archives\Rayman 2 iOS Extracted";
            //    FileSystemPath baseInDir = @"C:\Users\RayCarrot\Downloads\CNT Archives\Rayman 2 iOS";

            //    foreach (var file in Directory.GetFiles(baseInDir, "*.gf", SearchOption.AllDirectories))
            //    {
            //        var data = new OpenSpaceGfSerializer(OpenSpaceGameMode.Rayman2IOS.GetSettings()).Deserialize(file);

            //        var destinationFile = baseOutDir + (file - baseInDir);

            //        using var bmp = data.GetBitmap();

            //        Directory.CreateDirectory(destinationFile.Parent);
            //        bmp.Save(destinationFile.ChangeFileExtension(".png"), ImageFormat.Png);
            //    }
            //});
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

        /// <summary>
        /// The game mode selection for .gf files
        /// </summary>
        public EnumSelectionViewModel<OpenSpaceGameMode> GfGameModeSelection { get; }

        /// <summary>
        /// Indicates if mipmaps should be included when converting .gf files
        /// </summary>
        public bool GfIncludeMipmaps { get; set; }

        /// <summary>
        /// The game mode selection for .loc/.loc8 files
        /// </summary>
        public EnumSelectionViewModel<UbiArtGameMode> LocGameModeSelection { get; }

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

        #region Protected Methods

        /// <summary>
        /// Gets the RCP supported game from an OpenSpace game mode
        /// </summary>
        /// <param name="gameMode">The game mode to get the game from</param>
        /// <returns>The game or null if not available</returns>
        protected Games? GetGame(OpenSpaceGameMode gameMode)
        {
            return gameMode switch
            {
                OpenSpaceGameMode.Rayman2PC => Games.Rayman2,
                OpenSpaceGameMode.RaymanMPC => Games.RaymanM,
                OpenSpaceGameMode.RaymanArenaPC => Games.RaymanArena,
                OpenSpaceGameMode.Rayman3PC => Games.Rayman3,
                _ => (null as Games?)
            };
        }

        /// <summary>
        /// Gets the RCP supported game from an UbiArt game mode
        /// </summary>
        /// <param name="gameMode">The game mode to get the game from</param>
        /// <returns>The game or null if not available</returns>
        protected Games? GetGame(UbiArtGameMode gameMode)
        {
            return gameMode switch
            {
                UbiArtGameMode.RaymanOriginsPC => Games.RaymanOrigins,
                UbiArtGameMode.RaymanLegendsPC => Games.RaymanLegends,
                _ => (null as Games?)
            };
        }

        /// <summary>
        /// Gets the RCP supported game from a Rayman 1 game mode
        /// </summary>
        /// <param name="gameMode">The game mode to get the game from</param>
        /// <returns>The game or null if not available</returns>
        protected Games? GetGame(Rayman1GameMode gameMode)
        {
            return gameMode switch
            {
                Rayman1GameMode.Rayman1PC => Games.Rayman1,
                Rayman1GameMode.RaymanDesignerPC => Games.RaymanDesigner,
                Rayman1GameMode.RaymanEduPC => Games.EducationalDos,
                _ => (null as Games?)
            };
        }

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
                GetGame(CntGameModeSelection.SelectedValue));
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
                GetGame(IpkGameModeSelection.SelectedValue));
        }

        /// <summary>
        /// Converts .gf files to image files
        /// </summary>
        /// <returns>The task</returns>
        public async Task ConvertFromGFAsync()
        {
            // Convert the files
            await ConvertFilesAsync(new OpenSpaceGfSerializer(GfGameModeSelection.SelectedValue.GetSettings()), (data, filePath) =>
            {
                if (GfIncludeMipmaps)
                {
                    // TODO: Support mipmaps

                }
                else
                {
                    // Get a bitmap from the image data
                    using var bmp = data.GetBitmap();

                    // TODO: Allow multiple file formats - use same dialog as Archive Explorer

                    // Save the image
                    bmp.Save(filePath.ChangeFileExtension(".png"), ImageFormat.Png);
                }
            }, new FileFilterItem("*.gf", "GF").ToString(), GetGame(GfGameModeSelection.SelectedValue));
        }

        public async Task LoadR1LevelAsync()
        {
            // Allow the user to select the level file
            var fileResult = await RCFUI.BrowseUI.BrowseFileAsync(new FileBrowserViewModel()
            {
                // TODO: Localize
                Title = "Select level file",
                DefaultDirectory = GetGame(LevGameModeSelection.SelectedValue)?.GetInstallDir(false).FullPath,
                // TODO: Localize
                ExtensionFilter = new FileFilterItem("*.lev", "Level").ToString(),
                MultiSelection = true
            });

            if (fileResult.CanceledByUser)
                return;

            LevData = new Rayman1LevSerializer(new Rayman1Settings(LevGameModeSelection.SelectedValue)).Deserialize(fileResult.SelectedFile);

            LevGraphicsImageSource = LevData.GetBitmap().ToImageSource();
            LevTypesImageSource = LevData.GetTypeBitmap().ToImageSource();
        }

        public async Task OpenArchiveExplorerAsync(IArchiveDataManager manager, string fileFilter, Games? game)
        {
            // Allow the user to select the files
            var fileResult = await RCFUI.BrowseUI.BrowseFileAsync(new FileBrowserViewModel()
            {
                // TODO: Localize
                Title = "Select archive files",
                DefaultDirectory = game?.GetInstallDir(false).FullPath,
                ExtensionFilter = fileFilter,
                MultiSelection = true
            });

            if (fileResult.CanceledByUser)
                return;

            await RCFRCP.UI.ShowArchiveExplorerAsync(manager, fileResult.SelectedFiles);
        }

        public async Task ConvertFilesAsync<T>(BinaryDataSerializer<T> serializer, Action<T, FileSystemPath> convertAction, string fileFilter, Games? game)
        {
            // Allow the user to select the files
            var fileResult = await RCFUI.BrowseUI.BrowseFileAsync(new FileBrowserViewModel()
            {
                // TODO: Localize
                Title = "Select files to convert",
                DefaultDirectory = game?.GetInstallDir(false).FullPath,
                ExtensionFilter = fileFilter,
                MultiSelection = true
            });

            if (fileResult.CanceledByUser)
                return;

            // Allow the user to select the destination directory
            var destinationResult = await RCFUI.BrowseUI.BrowseDirectoryAsync(new DirectoryBrowserViewModel()
            {
                // TODO: Localize
                Title = "Select destination",
            });

            if (destinationResult.CanceledByUser)
                return;

            // TODO: Try/catch

            foreach (var file in fileResult.SelectedFiles)
            {
                var data = serializer.Deserialize(file);

                var destinationFile = destinationResult.SelectedDirectory + file.Name;

                destinationFile = destinationFile.GetNonExistingFileName();

                convertAction(data, destinationFile);
            }
        }

        #endregion

        #region Commands

        public ICommand OpenCNTArchiveExplorerCommand { get; }

        public ICommand OpenIPKArchiveExplorerCommand { get; }

        public ICommand ConvertFromGFCommand { get; }

        public ICommand ConvertToGFCommand { get; }

        public ICommand LoadR1LevelCommand { get; }

        #endregion
    }
}