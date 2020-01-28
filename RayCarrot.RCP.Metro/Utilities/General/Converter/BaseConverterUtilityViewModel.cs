using Newtonsoft.Json;
using RayCarrot.CarrotFramework.Abstractions;
using RayCarrot.IO;
using RayCarrot.Rayman;
using RayCarrot.UI;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Base view model for general converter utility view models
    /// </summary>
    /// <typeparam name="GameMode">The type of game modes to support</typeparam>
    public abstract class BaseConverterUtilityViewModel<GameMode> : BaseRCPViewModel
        where GameMode : Enum
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        protected BaseConverterUtilityViewModel()
        {
            // Create commands
            ConvertFromCommand = new AsyncRelayCommand(ConvertFromAsync);
            ConvertToCommand = new AsyncRelayCommand(ConvertToAsync);
        }

        #endregion

        #region Public Abstract Properties

        /// <summary>
        /// The game mode selection
        /// </summary>
        public abstract EnumSelectionViewModel<GameMode> GameModeSelection { get; }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Converts files using the specified serializer and convert action
        /// </summary>
        /// <typeparam name="T">The type of data to convert</typeparam>
        /// <param name="serializer">The serializer to use</param>
        /// <param name="convertAction">The convert action, converting the data to the specified file path with an optional configuration path</param>
        /// <param name="fileFilter">The file filter when selecting files to convert</param>
        /// <param name="supportedFileExtensions">The supported file extensions to export as</param>
        /// <param name="game">The game, if available</param>
        /// <returns>The task</returns>
        protected async Task ConvertFromAsync<T>(BinaryDataSerializer<T> serializer, Action<T, FileSystemPath, FileSystemPath> convertAction, string fileFilter, string[] supportedFileExtensions, Games? game)
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

            // Allow the user to select the file extension to export as
            var extResult = await RCFRCP.UI.SelectFileExtensionAsync(new FileExtensionSelectionDialogViewModel(supportedFileExtensions, Resources.Utilities_Converter_ExportExtensionHeader));

            if (extResult.CanceledByUser)
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

                    // Set the file extension
                    destinationFile = destinationFile.ChangeFileExtension(extResult.SelectedFileFormat).GetNonExistingFileName();

                    // Get the optional configuration path
                    var configFile = destinationFile.ChangeFileExtension(".json").GetNonExistingFileName();

                    // Convert the file
                    convertAction(data, destinationFile, configFile);
                }

                await RCFUI.MessageUI.DisplaySuccessfulActionMessageAsync(Resources.Utilities_Converter_Success);
            }
            catch (Exception ex)
            {
                ex.HandleError("Converting files");

                await RCFUI.MessageUI.DisplayExceptionMessageAsync(ex, Resources.Utilities_Converter_Error);
            }
        }

        /// <summary>
        /// Converts files using the specified serializer and convert action
        /// </summary>
        /// <typeparam name="T">The type of data to convert</typeparam>
        /// <param name="serializer">The serializer to use</param>
        /// <param name="convertAction">The convert action, converting the data from the specified file path with an optional configuration path</param>
        /// <param name="fileFilter">The file filter when selecting files to convert</param>
        /// <param name="fileExtension">The file extension to export as</param>
        /// <param name="requiresConfig">Indicates if the config file is required</param>
        /// <returns>The task</returns>
        protected async Task ConvertToAsync<T>(BinaryDataSerializer<T> serializer, Func<FileSystemPath, FileSystemPath, T> convertAction, string fileFilter, string fileExtension, bool requiresConfig)
        {
            // Allow the user to select the files
            var fileResult = await RCFUI.BrowseUI.BrowseFileAsync(new FileBrowserViewModel()
            {
                Title = Resources.Utilities_Converter_FileSelectionHeader,
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
                    // Get the destination file
                    var destinationFile = destinationResult.SelectedDirectory + file.Name;

                    // Set the file extension
                    destinationFile = destinationFile.ChangeFileExtension(fileExtension).GetNonExistingFileName();

                    // Get the optional configuration path
                    var configFile = file.ChangeFileExtension(".json");

                    if (requiresConfig && !configFile.FileExists)
                    {
                        await RCFUI.MessageUI.DisplayMessageAsync(String.Format(Resources.Utilities_Converter_MissingConfig, file), MessageType.Error);

                        continue;
                    }

                    // Convert the file
                    var data = convertAction(file, configFile);

                    // Create the destination file
                    using var destinationFileStream = File.Open(destinationFile, FileMode.Create, FileAccess.Write);

                    // Save the converted data
                    serializer.Serialize(destinationFileStream, data);
                }

                await RCFUI.MessageUI.DisplaySuccessfulActionMessageAsync(Resources.Utilities_Converter_Success);
            }
            catch (Exception ex)
            {
                ex.HandleError("Converting files");

                await RCFUI.MessageUI.DisplayExceptionMessageAsync(ex, Resources.Utilities_Converter_Error);
            }
        }

        /// <summary>
        /// Serializes data as JSON to a file
        /// </summary>
        /// <typeparam name="T">The type of data</typeparam>
        /// <param name="data">The data to serialize</param>
        /// <param name="filePath">The file path to save to</param>
        protected void SerializeJSON<T>(T data, FileSystemPath filePath)
        {
            // Serialize the data
            var configData = JsonConvert.SerializeObject(data, Formatting.Indented, new ByteArrayHexConverter());

            // Save the data
            File.WriteAllText(filePath, configData);
        }

        /// <summary>
        /// Deserializes data as JSON from a file
        /// </summary>
        /// <typeparam name="T">The type of data</typeparam>
        /// <param name="filePath">The file path to save to</param>
        protected T DeserializeJSON<T>(FileSystemPath filePath)
        {
            // Deserialize the data
            return JsonConvert.DeserializeObject<T>(File.ReadAllText(filePath), new ByteArrayHexConverter());
        }

        #endregion

        #region Public Abstract Methods

        /// <summary>
        /// Converts from the format
        /// </summary>
        /// <returns>The task</returns>
        public abstract Task ConvertFromAsync();

        /// <summary>
        /// Converts to the format
        /// </summary>
        /// <returns>The task</returns>
        public abstract Task ConvertToAsync();

        #endregion

        #region Commands

        public ICommand ConvertFromCommand { get; }

        public ICommand ConvertToCommand { get; }

        #endregion
    }
}