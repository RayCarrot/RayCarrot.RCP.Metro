using Newtonsoft.Json;
using RayCarrot.CarrotFramework.Abstractions;
using RayCarrot.IO;
using RayCarrot.Rayman;
using RayCarrot.UI;
using System;
using System.IO;
using System.Linq;
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

        #region Public Properties

        /// <summary>
        /// Indicates if a conversion is being processed
        /// </summary>
        public bool IsLoading { get; set; }

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
        /// <typeparam name="Settings">The type of serializer settings</typeparam>
        /// <param name="serializer">The serializer to use</param>
        /// <param name="convertAction">The convert action, converting the data to the specified file path</param>
        /// <param name="fileFilter">The file filter when selecting files to convert</param>
        /// <param name="supportedFileExtensions">The supported file extensions to export as</param>
        /// <param name="defaultDir">The default directory</param>
        /// <param name="encoder">An optional data encoder to use</param>
        /// <returns>The task</returns>
        protected async Task ConvertFromAsync<T, Settings>(BinaryDataSerializer<T, Settings> serializer, Action<T, FileSystemPath> convertAction, string fileFilter, string[] supportedFileExtensions, FileSystemPath? defaultDir, IDataEncoder encoder = null)
            where T : IBinarySerializable<Settings>
            where Settings : BinarySerializerSettings
        {
            if (IsLoading)
                return;

            try
            {
                IsLoading = true;

                // Make sure the directory exists
                if (defaultDir?.DirectoryExists != true)
                    defaultDir = null;

                // Allow the user to select the files
                var fileResult = await RCFUI.BrowseUI.BrowseFileAsync(new FileBrowserViewModel()
                {
                    Title = Resources.Utilities_Converter_FileSelectionHeader,
                    DefaultDirectory = defaultDir?.FullPath,
                    ExtensionFilter = fileFilter,
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

                // Allow the user to select the file extension to export as
                var extResult = await RCFRCP.UI.SelectFileExtensionAsync(new FileExtensionSelectionDialogViewModel(supportedFileExtensions, Resources.Utilities_Converter_ExportExtensionHeader));

                if (extResult.CanceledByUser)
                    return;

                try
                {
                    await Task.Run(() =>
                    {
                        // Convert every file
                        foreach (var file in fileResult.SelectedFiles)
                        {
                            Stream stream = null;

                            try
                            {
                                if (encoder != null)
                                {
                                    // Open the file in a stream
                                    using var fileStream = File.Open(file, FileMode.Open, FileAccess.Read);

                                    // Create a memory stream
                                    stream = new MemoryStream();

                                    // Decode the data
                                    encoder.Decode(fileStream, stream);

                                    // Set the position
                                    stream.Position = 0;
                                }
                                else
                                {
                                    stream = File.Open(file, FileMode.Open, FileAccess.Read);
                                }

                                // Read the file data
                                var data = serializer.Deserialize(stream);

                                // Get the destination file
                                var destinationFile = destinationResult.SelectedDirectory + file.Name;

                                // Set the file extension
                                destinationFile = destinationFile.ChangeFileExtension(new FileExtension(extResult.SelectedFileFormat)).GetNonExistingFileName();

                                // Convert the file
                                convertAction(data, destinationFile);
                            }
                            finally
                            {
                                stream?.Dispose();
                            }
                        }
                    });

                    await RCFUI.MessageUI.DisplaySuccessfulActionMessageAsync(Resources.Utilities_Converter_Success);
                }
                catch (Exception ex)
                {
                    ex.HandleError("Converting files");

                    await RCFUI.MessageUI.DisplayExceptionMessageAsync(ex, Resources.Utilities_Converter_Error);
                }
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Converts files using the specified serializer and convert action
        /// </summary>
        /// <typeparam name="T">The type of data to convert</typeparam>
        /// <typeparam name="Settings">The type of serializer settings</typeparam>
        /// <param name="serializer">The serializer to use</param>
        /// <param name="convertAction">The convert action, converting the data from the specified file path with the selected output format</param>
        /// <param name="fileFilter">The file filter when selecting files to convert</param>
        /// <param name="fileExtension">The file extension to export as</param>
        /// <param name="outputFormats">The available output formats</param>
        /// <param name="encoder">An optional data encoder to use</param>
        /// <returns>The task</returns>
        protected async Task ConvertToAsync<T, Settings>(BinaryDataSerializer<T, Settings> serializer, Func<FileSystemPath, string, T> convertAction, string fileFilter, FileExtension fileExtension, string[] outputFormats = null, IDataEncoder encoder = null)
            where T : IBinarySerializable<Settings>
            where Settings : BinarySerializerSettings
        {
            if (IsLoading)
                return;

            try
            {
                IsLoading = true;

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
                    Title = Resources.Browse_DestinationHeader,
                });

                if (destinationResult.CanceledByUser)
                    return;

                try
                {
                    // The output format
                    string outputFormat = null;

                    // Get the output format if available
                    if (outputFormats?.Any() == true)
                    {
                        // Get the format
                        var formatResult = await RCFRCP.UI.SelectFileExtensionAsync(new FileExtensionSelectionDialogViewModel(outputFormats, Resources.Utilities_Converter_FormatSelectionHeader));
                        
                        if (formatResult.CanceledByUser)
                            return;

                        outputFormat = formatResult.SelectedFileFormat;
                    }

                    await Task.Run(() =>
                    {
                        // Convert every file
                        foreach (var file in fileResult.SelectedFiles)
                        {
                            // Get the destination file
                            var destinationFile = destinationResult.SelectedDirectory + file.Name;

                            // Set the file extension
                            destinationFile = destinationFile.ChangeFileExtension(fileExtension).GetNonExistingFileName();

                            // Convert the file
                            var data = convertAction(file, outputFormat);

                            if (encoder == null)
                            {
                                // Create the destination file
                                using var destinationFileStream = File.Open(destinationFile, FileMode.Create, FileAccess.Write);

                                // Save the converted data
                                serializer.Serialize(destinationFileStream, data);
                            }
                            else
                            {
                                // Create a memory stream
                                using var encodingStream = new MemoryStream();

                                // Serialize the converted data to the memory stream
                                serializer.Serialize(encodingStream, data);

                                // Create the destination file
                                using var destinationFileStream = File.Open(destinationFile, FileMode.Create, FileAccess.Write);

                                encodingStream.Position = 0;

                                // Encode the data to the file
                                encoder.Encode(encodingStream, destinationFileStream);
                            }
                        }
                    });

                    await RCFUI.MessageUI.DisplaySuccessfulActionMessageAsync(Resources.Utilities_Converter_Success);
                }
                catch (Exception ex)
                {
                    ex.HandleError("Converting files");

                    await RCFUI.MessageUI.DisplayExceptionMessageAsync(ex, Resources.Utilities_Converter_Error);
                }
            }
            finally
            {
                IsLoading = false;
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