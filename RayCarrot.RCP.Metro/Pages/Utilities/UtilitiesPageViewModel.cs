using RayCarrot.Extensions;
using RayCarrot.IO;
using RayCarrot.Rayman;
using RayCarrot.UI;
using System;
using System.Drawing.Imaging;
using System.Threading.Tasks;
using System.Windows.Input;

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
            OpenCNTArchiveExplorerCommand = new AsyncRelayCommand(OpenCNTArchiveExplorerAsync);
            ConvertFromGFCommand = new AsyncRelayCommand(ConvertFromGFAsync);

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
        /// The available game modes for .cnt Archive Explorer
        /// </summary>
        public OpenSpaceGameMode[] AvailableCNTGameModes => new OpenSpaceGameMode[]
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
        };

        /// <summary>
        /// The available game modes for .gf converter
        /// </summary>
        public OpenSpaceGameMode[] AvailableGFGameModes => OpenSpaceGameMode.Rayman2PC.GetValues();

        /// <summary>
        /// The selected game mode for .cnt Archive Explorer
        /// </summary>
        public OpenSpaceGameMode SelectedCNTGameMode { get; set; }

        /// <summary>
        /// The selected game mode for .gf converter
        /// </summary>
        public OpenSpaceGameMode SelectedGFGameMode { get; set; }

        #endregion

        #region Protected Methods

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
                new OpenSpaceCntArchiveDataManager(SelectedCNTGameMode.GetSettings()), 
                new FileFilterItem("*.cnt", "CNT files").ToString(), 
                GetGame(SelectedCNTGameMode));
        }

        /// <summary>
        /// Converts .gf files to image files
        /// </summary>
        /// <returns>The task</returns>
        public async Task ConvertFromGFAsync()
        {
            // Convert the files
            await ConvertFilesAsync(new OpenSpaceGfSerializer(SelectedGFGameMode.GetSettings()), (data, filePath) =>
            {
                // TODO: Support mipmaps - have checkbox to include them

                // Get a bitmap from the image data
                using var bmp = data.GetBitmap();

                // TODO: Allow multiple file formats - use same dialog as Archive Explorer

                // Save the image
                bmp.Save(filePath.ChangeFileExtension(".png"), ImageFormat.Png);
            }, new FileFilterItem("*.gf", "GF").ToString(), GetGame(SelectedGFGameMode));
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

        public ICommand ConvertFromGFCommand { get; }

        public ICommand ConvertToGFCommand { get; }

        #endregion

        #region Enums

        // TODO: Move to RayCarrot.Rayman - set byte order based on platform
        public enum UbiArtGameModes
        {

        }

        #endregion
    }
}