using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
using Newtonsoft.Json;
using RayCarrot.CarrotFramework.Abstractions;
using RayCarrot.IO;
using RayCarrot.RCP.Core;
using RayCarrot.RCP.UI;
using RayCarrot.UI;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Base view model for a UbiArt localization converter utility
    /// </summary>
    public abstract class BaseUbiArtLocalizationConverterUtilityViewModel<DataType> : BaseRCPViewModel
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        protected BaseUbiArtLocalizationConverterUtilityViewModel()
        {
            ExportToJSONCommand = new AsyncRelayCommand(ExportToJSONAsync);
            ImportToLocCommand = new AsyncRelayCommand(ImportToLocAsync);
        }

        #endregion

        #region Commands

        public ICommand ExportToJSONCommand { get; }
        
        public ICommand ImportToLocCommand { get; }

        #endregion

        #region Protected Abstract Properties

        /// <summary>
        /// The default localization directory for the game, if available
        /// </summary>
        protected abstract FileSystemPath? DefaultLocalizationDirectory { get; }

        /// <summary>
        /// The localization file extension
        /// </summary>
        protected abstract string LocalizationFileExtension { get; }

        #endregion

        #region Protected Abstract Methods

        /// <summary>
        /// Deserializes the localization file
        /// </summary>
        /// <param name="file">The localization file</param>
        /// <returns>The data</returns>
        protected abstract DataType Deserialize(FileSystemPath file);

        /// <summary>
        /// Serializes the data to the localization file
        /// </summary>
        /// <param name="file">The localization file</param>
        /// <param name="data">The data</param>
        protected abstract void Serialize(FileSystemPath file, DataType data);

        #endregion

        #region Public Methods

        /// <summary>
        /// Exports a localization file to a JSON file
        /// </summary>
        /// <returns>The task</returns>
        public async Task ExportToJSONAsync()
        {
            // Get the input file
            var inputResult = await RCFUI.BrowseUI.BrowseFileAsync(new FileBrowserViewModel()
            {
                Title = Resources.UbiArtU_LocalizationConverterExportSelectionHeader,
                DefaultDirectory = DefaultLocalizationDirectory ?? FileSystemPath.EmptyPath,
                ExtensionFilter = new FileFilterItem($"*{LocalizationFileExtension}", Resources.UbiArtU_LocalizationConverterLocFilterDescription).StringRepresentation,
                DefaultName = $"localisation{LocalizationFileExtension}"
            });

            if (inputResult.CanceledByUser)
                return;

            // Get the output file
            var outputResult = await RCFUI.BrowseUI.SaveFileAsync(new SaveFileViewModel()
            {
                Title = Resources.ExportDestinationSelectionHeader,
                DefaultName = inputResult.SelectedFile.ChangeFileExtension(".json").Name,
                Extensions = new FileFilterItem("*.json", Resources.FileFilterDescription_JSON).StringRepresentation
            });

            if (outputResult.CanceledByUser)
                return;

            try
            {
                // Serialize the data into the new file
                File.WriteAllText(outputResult.SelectedFileLocation, JsonConvert.SerializeObject(Deserialize(inputResult.SelectedFile), Formatting.Indented));

                await RCFUI.MessageUI.DisplaySuccessfulActionMessageAsync(Resources.UbiArtU_LocalizationConverterExportSuccess);
            }
            catch (Exception ex)
            {
                ex.HandleError("Exporting localization file to JSON");
                await RCFUI.MessageUI.DisplayExceptionMessageAsync(ex, Resources.UbiArtU_LocalizationConverterExportError);
            }
        }

        /// <summary>
        /// Imports a JSON file to a localization file
        /// </summary>
        /// <returns>The task</returns>
        public async Task ImportToLocAsync()
        {
            // Get the input file
            var inputResult = await RCFUI.BrowseUI.BrowseFileAsync(new FileBrowserViewModel()
            {
                Title = Resources.ImportSelectionHeader,
                ExtensionFilter = new FileFilterItem("*.json", Resources.FileFilterDescription_JSON).StringRepresentation,
                DefaultName = "localisation.json"
            });

            if (inputResult.CanceledByUser)
                return;

            // Get the localization file
            var outputResult = await RCFUI.BrowseUI.BrowseFileAsync(new FileBrowserViewModel()
            {
                Title = Resources.UbiArtU_LocalizationConverterImportDestinationSelectionHeader,
                DefaultName = inputResult.SelectedFile.ChangeFileExtension(LocalizationFileExtension).Name,
                ExtensionFilter = new FileFilterItem($"*{LocalizationFileExtension}", Resources.UbiArtU_LocalizationConverterLocFilterDescription).StringRepresentation,
            });

            if (outputResult.CanceledByUser)
                return;

            try
            {
                // Deserialize the JSON input file
                var data = JsonConvert.DeserializeObject<DataType>(File.ReadAllText(inputResult.SelectedFile));

                // Serialize the data to the output localization file
                Serialize(outputResult.SelectedFile, data);

                await RCFUI.MessageUI.DisplaySuccessfulActionMessageAsync(Resources.UbiArtU_LocalizationConverterImportSuccess);
            }
            catch (Exception ex)
            {
                ex.HandleError("Importing JSON to localization file");
                await RCFUI.MessageUI.DisplayExceptionMessageAsync(ex, Resources.UbiArtU_LocalizationConverterImportError);
            }
        }

        #endregion
    }
}