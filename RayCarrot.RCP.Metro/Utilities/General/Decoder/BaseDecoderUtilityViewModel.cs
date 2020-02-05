using RayCarrot.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
using RayCarrot.IO;
using RayCarrot.Rayman;
using RayCarrot.CarrotFramework.Abstractions;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Base view model for general decoder utility view models
    /// </summary>
    /// <typeparam name="GameMode">The type of game modes to support</typeparam>
    public abstract class BaseDecoderUtilityViewModel<GameMode> : BaseRCPViewModel
        where GameMode : Enum
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        protected BaseDecoderUtilityViewModel()
        {
            // Create commands
            DecodeCommand = new AsyncRelayCommand(DecodeFileAsync);
            EncodeCommand = new AsyncRelayCommand(EncodeFileAsync);
        }

        #endregion

        #region Protected Abstract Properties

        /// <summary>
        /// Gets the file filter to use
        /// </summary>
        protected abstract string GetFileFilter { get; }

        #endregion

        #region Public Abstract Properties

        /// <summary>
        /// The game mode selection
        /// </summary>
        public abstract EnumSelectionViewModel<GameMode> GameModeSelection { get; }

        #endregion

        #region Protected Abstract Methods

        /// <summary>
        /// Gets a new data encoder
        /// </summary>
        protected abstract IDataEncoder GetEncoder();

        #endregion

        #region Protected Methods

        /// <summary>
        /// Processes a file
        /// </summary>
        /// <param name="inputFiles">The input files to process</param>
        /// <param name="outputDir">The output directory</param>
        /// <param name="shouldDecode">True if the files should be decoded, or false to encode them</param>
        protected void ProcessFile(IEnumerable<FileSystemPath> inputFiles, FileSystemPath outputDir, bool shouldDecode)
        {
            // Get the encoder
            var encoder = GetEncoder();

            // Decode every file
            foreach (var file in inputFiles)
            {
                // Open the file
                using var fileStream = File.OpenRead(file);

                // Process the file data
                var data = shouldDecode ? encoder.Decode(fileStream) : encoder.Encode(fileStream);

                // Get the destination file
                var destinationFile = (outputDir + file.Name).GetNonExistingFileName();

                // Open and create the file
                using var outputFileStream = File.OpenWrite(destinationFile);

                // Write to the file
                foreach (var b in data)
                    outputFileStream.WriteByte(b);
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Decodes a file
        /// </summary>
        /// <returns>The task</returns>
        public async Task DecodeFileAsync()
        {
            // Get the game
            var game = GameModeSelection.SelectedValue.GetGame();

            // Allow the user to select the files
            var fileResult = await RCFUI.BrowseUI.BrowseFileAsync(new FileBrowserViewModel()
            {
                Title = Resources.Utilities_Decoder_DecodeFileSelectionHeader,
                DefaultDirectory = game?.GetInstallDir(false).FullPath,
                ExtensionFilter = GetFileFilter,
                MultiSelection = true
            });

            if (fileResult.CanceledByUser)
                return;

            // Allow the user to select the destination directory
            var destinationResult = await RCFUI.BrowseUI.BrowseDirectoryAsync(new DirectoryBrowserViewModel()
            {
                Title = Resources.Browse_DestinationHeader,
            });

            if (destinationResult.CanceledByUser)
                return;

            try
            {
                // Process the files
                ProcessFile(fileResult.SelectedFiles, destinationResult.SelectedDirectory, true);

                await RCFUI.MessageUI.DisplaySuccessfulActionMessageAsync(Resources.Utilities_Decoder_DecodeSuccess);
            }
            catch (Exception ex)
            {
                ex.HandleError("Decoding files");
                
                await RCFUI.MessageUI.DisplayExceptionMessageAsync(ex, Resources.Utilities_Decoder_DecodeError);
            }
        }

        /// <summary>
        /// Encodes a file
        /// </summary>
        /// <returns>The task</returns>
        public async Task EncodeFileAsync()
        {
            // Get the game
            var game = GameModeSelection.SelectedValue.GetGame();

            // Allow the user to select the files
            var fileResult = await RCFUI.BrowseUI.BrowseFileAsync(new FileBrowserViewModel()
            {
                Title = Resources.Utilities_Decoder_EncodeFileSelectionHeader,
                DefaultDirectory = game?.GetInstallDir(false).FullPath,
                ExtensionFilter = GetFileFilter,
                MultiSelection = true
            });

            if (fileResult.CanceledByUser)
                return;

            // Allow the user to select the destination directory
            var destinationResult = await RCFUI.BrowseUI.BrowseDirectoryAsync(new DirectoryBrowserViewModel()
            {
                Title = Resources.Browse_DestinationHeader,
            });

            if (destinationResult.CanceledByUser)
                return;

            try
            {
                // Process the files
                ProcessFile(fileResult.SelectedFiles, destinationResult.SelectedDirectory, false);

                await RCFUI.MessageUI.DisplaySuccessfulActionMessageAsync(Resources.Utilities_Decoder_EncodeSuccess);
            }
            catch (Exception ex)
            {
                ex.HandleError("Encoding files");

                await RCFUI.MessageUI.DisplayExceptionMessageAsync(ex, Resources.Utilities_Decoder_EncodeError);
            }
        }

        #endregion

        #region Commands

        public ICommand DecodeCommand { get; }

        public ICommand EncodeCommand { get; }

        #endregion
    }
}