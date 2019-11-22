using System;
using Newtonsoft.Json;
using RayCarrot.IO;
using RayCarrot.Rayman;
using RayCarrot.UI;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
using RayCarrot.CarrotFramework.Abstractions;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// View model for the Rayman Legends Loc8 converter utility
    /// </summary>
    public class RLLoc8ConverterUtilityViewModel : BaseRCPViewModel
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public RLLoc8ConverterUtilityViewModel()
        {
            ExportToJSONCommand = new AsyncRelayCommand(ExportToJSONAsync);
            ImportToLoc8Command = new AsyncRelayCommand(ImportToLoc8Async);
        }

        #endregion

        #region Commands

        public ICommand ExportToJSONCommand { get; }
        
        public ICommand ImportToLoc8Command { get; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Exports a .loc8 file to a .json file
        /// </summary>
        /// <returns>The task</returns>
        public async Task ExportToJSONAsync()
        {
            // Get the input file
            var inputResult = await RCFUI.BrowseUI.BrowseFileAsync(new FileBrowserViewModel()
            {
                Title = Resources.RLU_Loc8ConverterExportSelectionHeader,
                DefaultDirectory = Games.RaymanLegends.GetData().InstallDirectory + "EngineData" + "Localisation",
                ExtensionFilter = new FileFilterItem("*.loc8", Resources.RLU_Loc8ConverterLoc8FilterDescription).StringRepresentation,
                DefaultName = "localisation.loc8"
            });

            if (inputResult.CanceledByUser)
                return;

            // Get the output file
            var outputResult = await RCFUI.BrowseUI.SaveFileAsync(new SaveFileViewModel()
            {
                Title = Resources.RLU_Loc8ConverterExportDestinationSelectionHeader,
                DefaultName = inputResult.SelectedFile.ChangeFileExtension(".json").Name,
                Extensions = new FileFilterItem("*.json", Resources.RLU_Loc8ConverterJSONFilterDescription).StringRepresentation
            });

            if (outputResult.CanceledByUser)
                return;

            try
            {
                // Serialize the data into the new file
                File.WriteAllText(outputResult.SelectedFileLocation, JsonConvert.SerializeObject(new Loc8Handler(inputResult.SelectedFile).StringData, Formatting.Indented));
            }
            catch (Exception ex)
            {
                ex.HandleError("Exporting loc8 to JSON");
                await RCFUI.MessageUI.DisplayExceptionMessageAsync(ex, Resources.RLU_Loc8ConverterExportError);
            }
        }

        /// <summary>
        /// Imports a .json file to a .loc8 file
        /// </summary>
        /// <returns>The task</returns>
        public async Task ImportToLoc8Async()
        {
            // Get the input file
            var inputResult = await RCFUI.BrowseUI.BrowseFileAsync(new FileBrowserViewModel()
            {
                Title = Resources.RLU_Loc8ConverterImportSelectionHeader,
                ExtensionFilter = new FileFilterItem("*.json", Resources.RLU_Loc8ConverterJSONFilterDescription).StringRepresentation,
                DefaultName = "localisation.json"
            });

            if (inputResult.CanceledByUser)
                return;

            // Get the localization file
            var outputResult = await RCFUI.BrowseUI.BrowseFileAsync(new FileBrowserViewModel()
            {
                Title = Resources.RLU_Loc8ConverterImportDestinationSelectionHeader,
                DefaultName = inputResult.SelectedFile.ChangeFileExtension(".loc8").Name,
                ExtensionFilter = new FileFilterItem("*.loc8", Resources.RLU_Loc8ConverterLoc8FilterDescription).StringRepresentation,
            });

            if (outputResult.CanceledByUser)
                return;

            try
            {
                // Deserialize the file
                var data = JsonConvert.DeserializeObject<Loc8StringData>(File.ReadAllText(inputResult.SelectedFile));

                // Load the localization handler
                var handler = new Loc8Handler(outputResult.SelectedFile)
                {
                    // Write the data to the handler
                    StringData = data
                };

                // Save the handler data
                handler.Save();
            }
            catch (Exception ex)
            {
                ex.HandleError("Importing JSON to loc8");
                await RCFUI.MessageUI.DisplayExceptionMessageAsync(ex, Resources.RLU_Loc8ConverterImportError);
            }
        }

        #endregion
    }
}