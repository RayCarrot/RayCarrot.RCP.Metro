using RayCarrot.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
using RayCarrot.IO;
using RayCarrot.Rayman;
using RayCarrot.Logging;
using RayCarrot.WPF;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Base view model for general decoder utility view models
    /// </summary>
    /// <typeparam name="GameMode">The type of game modes to support</typeparam>
    public abstract class Utility_BaseDecoder_ViewModel<GameMode> : BaseRCPViewModel
        where GameMode : Enum
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        protected Utility_BaseDecoder_ViewModel()
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

        /// <summary>
        /// Gets the game for the current selection
        /// </summary>
        protected abstract Games? GetGame { get; }

        #endregion

        #region Public Properties

        /// <summary>
        /// Indicates if an operation is loading
        /// </summary>
        public bool IsLoading { get; set; }

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
            if (IsLoading)
                return;

            try
            {
                IsLoading = true;

                // Get the encoder
                var encoder = GetEncoder();

                // Decode every file
                foreach (var file in inputFiles)
                {
                    // Open the input file
                    using var inputStream = File.OpenRead(file);

                    // Open and create the destination file
                    using var outputStream = File.OpenWrite((outputDir + file.Name).GetNonExistingFileName());

                    // Process the file data
                    if (shouldDecode)
                        encoder.Decode(inputStream, outputStream);
                    else
                        encoder.Encode(inputStream, outputStream);
                }
            }
            finally
            {
                IsLoading = false;
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
            // Allow the user to select the files
            var fileResult = await Services.BrowseUI.BrowseFileAsync(new FileBrowserViewModel()
            {
                Title = Resources.Utilities_Decoder_DecodeFileSelectionHeader,
                DefaultDirectory = GetGame?.GetInstallDir(false).FullPath,
                ExtensionFilter = GetFileFilter,
                MultiSelection = true
            });

            if (fileResult.CanceledByUser)
                return;

            // Allow the user to select the destination directory
            var destinationResult = await Services.BrowseUI.BrowseDirectoryAsync(new DirectoryBrowserViewModel()
            {
                Title = Resources.Browse_DestinationHeader,
            });

            if (destinationResult.CanceledByUser)
                return;

            try
            {
                // Process the files
                await Task.Run(() => ProcessFile(fileResult.SelectedFiles, destinationResult.SelectedDirectory, true));

                await Services.MessageUI.DisplaySuccessfulActionMessageAsync(Resources.Utilities_Decoder_DecodeSuccess);
            }
            catch (NotImplementedException ex)
            {
                ex.HandleExpected("Decoding files");

                await Services.MessageUI.DisplayMessageAsync(Resources.NotImplemented, MessageType.Error);
            }
            catch (Exception ex)
            {
                ex.HandleError("Decoding files");
                
                await Services.MessageUI.DisplayExceptionMessageAsync(ex, Resources.Utilities_Decoder_DecodeError);
            }
        }

        /// <summary>
        /// Encodes a file
        /// </summary>
        /// <returns>The task</returns>
        public async Task EncodeFileAsync()
        {
            // Allow the user to select the files
            var fileResult = await Services.BrowseUI.BrowseFileAsync(new FileBrowserViewModel()
            {
                Title = Resources.Utilities_Decoder_EncodeFileSelectionHeader,
                DefaultDirectory = GetGame?.GetInstallDir(false).FullPath,
                ExtensionFilter = GetFileFilter,
                MultiSelection = true
            });

            if (fileResult.CanceledByUser)
                return;

            // Allow the user to select the destination directory
            var destinationResult = await Services.BrowseUI.BrowseDirectoryAsync(new DirectoryBrowserViewModel()
            {
                Title = Resources.Browse_DestinationHeader,
            });

            if (destinationResult.CanceledByUser)
                return;

            try
            {
                // Process the files
                await Task.Run(() => ProcessFile(fileResult.SelectedFiles, destinationResult.SelectedDirectory, false));

                await Services.MessageUI.DisplaySuccessfulActionMessageAsync(Resources.Utilities_Decoder_EncodeSuccess);
            }
            catch (NotImplementedException ex)
            {
                ex.HandleExpected("Encoding files");

                await Services.MessageUI.DisplayMessageAsync(Resources.NotImplemented, MessageType.Error);
            }
            catch (Exception ex)
            {
                ex.HandleError("Encoding files");

                await Services.MessageUI.DisplayExceptionMessageAsync(ex, Resources.Utilities_Decoder_EncodeError);
            }
        }

        #endregion

        #region Commands

        public ICommand DecodeCommand { get; }

        public ICommand EncodeCommand { get; }

        #endregion
    }
}