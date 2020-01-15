using System;
using System.Drawing.Imaging;
using System.Threading.Tasks;
using System.Windows.Input;
using RayCarrot.CarrotFramework.Abstractions;
using RayCarrot.Extensions;
using RayCarrot.IO;
using RayCarrot.Rayman;
using RayCarrot.UI;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// View model for the converter utilities
    /// </summary>
    public class UtilitiesConverterViewModel : BaseRCPViewModel
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public UtilitiesConverterViewModel()
        {
            // Create commands
            ConvertFromGFCommand = new AsyncRelayCommand(ConvertFromGFAsync);

            // Set up selections
            GfGameModeSelection = new EnumSelectionViewModel<OpenSpaceGameMode>(OpenSpaceGameMode.Rayman2PC, OpenSpaceGameMode.Rayman2PC.GetValues());

            R3SaveGameModeSelection = new EnumSelectionViewModel<Platforms>(Platforms.PC, new Platforms[]
            {
                Platforms.PC
            });

            LocGameModeSelection = new EnumSelectionViewModel<UbiArtGameMode>(UbiArtGameMode.RaymanOriginsPC, new[]
            {
                UbiArtGameMode.RaymanOriginsPC,
                UbiArtGameMode.RaymanFiestaRunAndroid,
                UbiArtGameMode.RaymanLegendsPC,
                UbiArtGameMode.RaymanAdventuresAndroid,
                UbiArtGameMode.RaymanMiniMac,
                UbiArtGameMode.ValiantHeartsAndroid,
            });

            RJRSaveGameModeSelection = new EnumSelectionViewModel<Platforms>(Platforms.PC, new Platforms[]
            {
                Platforms.PC
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
        /// The game mode selection for .gf files
        /// </summary>
        public EnumSelectionViewModel<OpenSpaceGameMode> GfGameModeSelection { get; }

        /// <summary>
        /// Indicates if mipmaps should be included when converting .gf files
        /// </summary>
        public bool GfIncludeMipmaps { get; set; }

        /// <summary>
        /// The game mode selection for Rayman 3 .sav files
        /// </summary>
        public EnumSelectionViewModel<Platforms> R3SaveGameModeSelection { get; }

        /// <summary>
        /// The game mode selection for .loc/.loc8 files
        /// </summary>
        public EnumSelectionViewModel<UbiArtGameMode> LocGameModeSelection { get; }

        /// <summary>
        /// The game mode selection for Rayman 3 .sav files
        /// </summary>
        public EnumSelectionViewModel<Platforms> RJRSaveGameModeSelection { get; }

        #endregion

        #region Public Methods

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
            }, new FileFilterItem("*.gf", "GF").ToString(), GfGameModeSelection.SelectedValue.GetGame());
        }

        /// <summary>
        /// Converts files using the specified serializer and convert action
        /// </summary>
        /// <typeparam name="T">The type of data to convert</typeparam>
        /// <param name="serializer">The serializer to use</param>
        /// <param name="convertAction">The convert action, converting the data to the specified file path</param>
        /// <param name="fileFilter">The file filter when selecting files to convert</param>
        /// <param name="game">The game, if available</param>
        /// <returns>The task</returns>
        public async Task ConvertFilesAsync<T>(BinaryDataSerializer<T> serializer, Action<T, FileSystemPath> convertAction, string fileFilter, Games? game)
        {
            // Allow the user to select the files
            var fileResult = await RCFUI.BrowseUI.BrowseFileAsync(new FileBrowserViewModel()
            {
                Title = Resources.Utilities_Converter_FileSelectionHeader,
                DefaultDirectory = game?.GetInstallDir(false).FullPath,
                ExtensionFilter = fileFilter,
                MultiSelection = true
            });

            if (fileResult.CanceledByUser)
                return;

            // Allow the user to select the destination directory
            var destinationResult = await RCFUI.BrowseUI.BrowseDirectoryAsync(new DirectoryBrowserViewModel()
            {
                Title = Resources.Utilities_Converter_DestinationHeader,
            });

            if (destinationResult.CanceledByUser)
                return;

            try
            {
                // Convert every file
                foreach (var file in fileResult.SelectedFiles)
                {
                    // Read the file data
                    var data = serializer.Deserialize(file);

                    // Get the destination file
                    var destinationFile = destinationResult.SelectedDirectory + file.Name;

                    // Get a non-existing file name
                    destinationFile = destinationFile.GetNonExistingFileName();

                    // Convert the file
                    convertAction(data, destinationFile);
                }
            }
            catch (Exception ex)
            {
                ex.HandleError("Converting files");

                await RCFUI.MessageUI.DisplayExceptionMessageAsync(ex, Resources.Utilities_Converter_Error);
            }
        }

        #endregion

        #region Commands

        public ICommand ConvertFromGFCommand { get; }

        public ICommand ConvertToGFCommand { get; }

        public ICommand ConvertFromR3SaveCommand { get; }

        public ICommand ConvertToR3SaveCommand { get; }

        public ICommand ConvertFromLOCCommand { get; }

        public ICommand ConvertToLOCCommand { get; }

        public ICommand ConvertFromRJRSaveCommand { get; }

        public ICommand ConvertToRJRSaveCommand { get; }

        #endregion

        #region Enums

        /// <summary>
        /// The available platforms for utilities
        /// </summary>
        public enum Platforms
        {
            /// <summary>
            /// Windows PC
            /// </summary>
            PC
        }

        #endregion
    }
}