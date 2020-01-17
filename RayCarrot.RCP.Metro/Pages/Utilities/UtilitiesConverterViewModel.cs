using Newtonsoft.Json;
using RayCarrot.CarrotFramework.Abstractions;
using RayCarrot.Extensions;
using RayCarrot.IO;
using RayCarrot.Rayman;
using RayCarrot.UI;
using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

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
            ConvertToGFCommand = new AsyncRelayCommand(ConvertToGFAsync);
            ConvertFromR3SaveCommand = new AsyncRelayCommand(ConvertFromR3SaveAsync);
            ConvertFromLOCCommand = new AsyncRelayCommand(ConvertFromLOCAsync);
            ConvertToLOCCommand = new AsyncRelayCommand(ConvertToLOCAsync);
            ConvertFromRJRSaveCommand = new AsyncRelayCommand(ConvertFromRJRSaveAsync);
            ConvertToRJRSaveCommand = new AsyncRelayCommand(ConvertToRJRSaveAsync);

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
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// The game mode selection for .gf files
        /// </summary>
        public EnumSelectionViewModel<OpenSpaceGameMode> GfGameModeSelection { get; }

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
            await ConvertFromAsync(new OpenSpaceGfSerializer(GfGameModeSelection.SelectedValue.GetSettings()), (data, filePath, configPath) =>
            {
                // Get a bitmap from the image data
                using var bmp = data.GetBitmap();

                // Save the image
                bmp.Save(filePath, ImageHelpers.GetImageFormat(filePath.FileExtension));

                // Create the config file
                var config = new GFConfigData(data.Channels, data.Format, data.MipmapCount, data.RepeatByte, true);

                // Save the config file
                SerializeJSON(config, configPath);
            }, new FileFilterItem("*.gf", "GF").ToString(), ImageHelpers.GetSupportedBitmapExtensions(), GfGameModeSelection.SelectedValue.GetGame());
        }

        /// <summary>
        /// Converts .gf files from image files
        /// </summary>
        /// <returns>The task</returns>
        public async Task ConvertToGFAsync()
        {
            var settings = GfGameModeSelection.SelectedValue.GetSettings();

            await ConvertToAsync(new OpenSpaceGfSerializer(settings), (filePath, configPath) =>
            {
                // Read the config file
                var config = DeserializeJSON<GFConfigData>(configPath);

                // Create the GF data
                var data = new OpenSpaceGFFile(settings)
                {
                    Channels = config.Channels,
                    Format = config.Format,
                    MipmapCount = config.MipmapCount,
                };

                // Read the image
                using var bmp = new Bitmap(filePath);

                // Load it into the GF data
                data.ImportFromBitmap(bmp);

                // Set repeat byte if not auto generated
                if (!config.AutoGenerateRepeatByte)
                    data.RepeatByte = config.RepeatByte;

                // Return the data
                return data;
            }, new FileFilterItemCollection(ImageHelpers.GetSupportedBitmapExtensions().Select(x => new FileFilterItem($"*{x}", 
                x.Substring(1).ToUpper()))).ToString(), ".gf", true);
        }

        /// <summary>
        /// Converts Rayman 3 save files to JSON files
        /// </summary>
        /// <returns>The task</returns>
        public async Task ConvertFromR3SaveAsync()
        {
            await ConvertFromAsync(new Rayman3SaveDataSerializer(), (data, filePath, configPath) =>
            {
                // Save the data
                SerializeJSON(data, filePath);
            }, new FileFilterItem("*.sav", "SAV").ToString(), new []
            {
                ".json"
            }, Games.Rayman3);
        }

        /// <summary>
        /// Converts UbiArt localization files to JSON files
        /// </summary>
        /// <returns>The task</returns>
        public async Task ConvertFromLOCAsync()
        {
            if (LocGameModeSelection.SelectedValue == UbiArtGameMode.RaymanFiestaRunPC)
            {
                await ConvertFromAsync(new FiestaRunLocalizationSerializer(), (data, filePath, configPath) =>
                {
                    // Save the data
                    SerializeJSON(data, filePath);
                }, new FileFilterItem("*.loc", "LOC").ToString(), new[]
                {
                    ".json"
                }, Games.RaymanFiestaRun);
            }
            else
            {
                var settings = LocGameModeSelection.SelectedValue.GetSettings();

                var fileExtension = settings.TextEncoding == TextEncoding.UTF8 ? new FileFilterItem("*.loc8", "LOC8") : new FileFilterItem("*.loc", "LOC");

                await ConvertFromAsync(new UbiArtLocalizationSerializer(settings), (data, filePath, configPath) =>
                {
                    // Save the data
                    SerializeJSON(data, filePath);
                }, fileExtension.ToString(), new[]
                {
                    ".json"
                }, LocGameModeSelection.SelectedValue.GetGame());
            }
        }

        /// <summary>
        /// Converts UbiArt localization files from JSON files
        /// </summary>
        /// <returns>The task</returns>
        public async Task ConvertToLOCAsync()
        {
            if (LocGameModeSelection.SelectedValue == UbiArtGameMode.RaymanFiestaRunPC)
            {
                await ConvertToAsync(new FiestaRunLocalizationSerializer(), (filePath, configPath) =>
                {
                    // Read the data
                    return DeserializeJSON<FiestaRunLocalizationData>(filePath);
                }, new FileFilterItem("*.json", "JSON").ToString(), ".loc", false);
            }
            else
            {
                var settings = LocGameModeSelection.SelectedValue.GetSettings();

                var fileExtension = settings.TextEncoding == TextEncoding.UTF8 ? ".loc8" : ".loc";

                await ConvertToAsync(new UbiArtLocalizationSerializer(settings), (filePath, configPath) =>
                {
                    // Read the data
                    return DeserializeJSON<UbiArtLocalizationData>(filePath);
                }, new FileFilterItem("*.json", "JSON").ToString(), fileExtension, false);
            }
        }

        /// <summary>
        /// Converts Rayman Jungle Run save files to JSON files
        /// </summary>
        /// <returns>The task</returns>
        public async Task ConvertFromRJRSaveAsync()
        {
            await ConvertFromAsync(new JungleRunSaveDataSerializer(), (data, filePath, configPath) =>
            {
                // Save the data
                SerializeJSON(data, filePath);
            }, new FileFilterItem("*.dat", "DAT").ToString(), new[]
            {
                ".json"
            }, Games.RaymanJungleRun);
        }

        /// <summary>
        /// Converts Rayman Jungle Run save files from JSON files
        /// </summary>
        /// <returns>The task</returns>
        public async Task ConvertToRJRSaveAsync()
        {
            await ConvertToAsync(new JungleRunSaveDataSerializer(), (filePath, configPath) =>
            {
                // Read the data
                return DeserializeJSON<JungleRunSaveData>(filePath);
            }, new FileFilterItem("*.json", "JSON").ToString(), ".dat", false);
        }

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
        public async Task ConvertFromAsync<T>(BinaryDataSerializer<T> serializer, Action<T, FileSystemPath, FileSystemPath> convertAction, string fileFilter, string[] supportedFileExtensions, Games? game)
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
        public async Task ConvertToAsync<T>(BinaryDataSerializer<T> serializer, Func<FileSystemPath, FileSystemPath, T> convertAction, string fileFilter, string fileExtension, bool requiresConfig)
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
        public void SerializeJSON<T>(T data, FileSystemPath filePath)
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
        public T DeserializeJSON<T>(FileSystemPath filePath)
        {
            // Deserialize the data
            return JsonConvert.DeserializeObject<T>(File.ReadAllText(filePath), new ByteArrayHexConverter());
        }

        #endregion

        #region Commands

        public ICommand ConvertFromGFCommand { get; }

        public ICommand ConvertToGFCommand { get; }

        public ICommand ConvertFromR3SaveCommand { get; }

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

        #region Classes

        /// <summary>
        /// Configuration data for a GF file
        /// </summary>
        public class GFConfigData
        {
            public GFConfigData(byte channels, uint format, byte mipmapCount, byte repeatByte, bool autoGenerateRepeatByte)
            {
                Channels = channels;
                Format = format;
                MipmapCount = mipmapCount;
                RepeatByte = repeatByte;
                AutoGenerateRepeatByte = autoGenerateRepeatByte;
            }

            public byte Channels { get; }

            public uint Format { get; }

            public byte MipmapCount { get; }

            public byte RepeatByte { get; }

            public bool AutoGenerateRepeatByte { get; }
        }

        #endregion
    }
}