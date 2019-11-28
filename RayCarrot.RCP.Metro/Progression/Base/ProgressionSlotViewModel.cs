using System;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows.Input;
using RayCarrot.CarrotFramework.Abstractions;
using RayCarrot.IO;
using RayCarrot.UI;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// View model for a progression slot item
    /// </summary>
    public abstract class ProgressionSlotViewModel : BaseRCPViewModel, IDisposable
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="slotNameGenerator">The function to get the slot name</param>
        /// <param name="items">The progression info items</param>
        /// <param name="saveSlotFilePath">The file path for the save slot</param>
        /// <param name="progressionViewModel">The progression view model containing this slot</param>
        protected ProgressionSlotViewModel(Func<string> slotNameGenerator, ProgressionInfoItemViewModel[] items, FileSystemPath saveSlotFilePath, BaseProgressionViewModel progressionViewModel)
        {
            SlotNameGenerator = slotNameGenerator;
            Items = items;
            SaveSlotFilePath = saveSlotFilePath;
            ProgressionViewModel = progressionViewModel;
            SlotName = SlotNameGenerator();

            ExportCommand = new AsyncRelayCommand(ExportAsync);
            ImportCommand = new AsyncRelayCommand(ImportAsync);

            RCFCore.Data.CultureChanged += Data_CultureChanged;
        }

        #endregion

        #region Commands

        public ICommand ExportCommand { get; }

        public ICommand ImportCommand { get; }

        #endregion

        #region Protected Properties

        /// <summary>
        /// The function to get the slot name
        /// </summary>
        protected Func<string> SlotNameGenerator { get; }

        #endregion

        #region Public Properties

        /// <summary>
        /// The slot name
        /// </summary>
        public string SlotName { get; set; }

        /// <summary>
        /// The file path for the save slot
        /// </summary>
        public FileSystemPath SaveSlotFilePath { get; }

        /// <summary>
        /// The progression info items
        /// </summary>
        public ProgressionInfoItemViewModel[] Items { get; }

        /// <summary>
        /// The progression view model containing this slot
        /// </summary>
        public BaseProgressionViewModel ProgressionViewModel { get; }

        #endregion

        #region Event Handlers

        private void Data_CultureChanged(object sender, PropertyChangedEventArgs<CultureInfo> e)
        {
            SlotName = SlotNameGenerator();
        }

        #endregion

        #region Protected Abstract Methods

        /// <summary>
        /// Exports the save slot from the specified path
        /// </summary>
        /// <param name="outputFilePath">The output file path</param>
        /// <returns>The task</returns>
        protected abstract Task ExportSaveDataAsync(FileSystemPath outputFilePath);

        /// <summary>
        /// Imports an exported save slot to the save slot from the specified path
        /// </summary>
        /// <param name="inputFilePath">The input file path</param>
        /// <returns>The task</returns>
        protected abstract Task ImportSaveDataAsync(FileSystemPath inputFilePath);

        #endregion

        #region Public Methods

        /// <summary>
        /// Exports the save slot from the specified path
        /// </summary>
        /// <returns>The task</returns>
        public async Task ExportAsync()
        {
            // Get the output file
            var outputResult = await RCFUI.BrowseUI.SaveFileAsync(new SaveFileViewModel()
            {
                Title = Resources.ExportDestinationSelectionHeader,
                DefaultName = SaveSlotFilePath.ChangeFileExtension(".json").Name,
                Extensions = new FileFilterItem("*.json", Resources.FileFilterDescription_JSON).StringRepresentation
            });

            if (outputResult.CanceledByUser)
                return;

            RCFCore.Logger?.LogInformationSource($"Progression data for slot {SlotName} is being exported...");

            try
            {
                // Export
                await ExportSaveDataAsync(outputResult.SelectedFileLocation);

                RCFCore.Logger?.LogInformationSource($"Progression data has been exported");

                await RCFUI.MessageUI.DisplaySuccessfulActionMessageAsync(Resources.Progression_ExportSuccess);
            }
            catch (Exception ex)
            {
                ex.HandleError("Exporting progression slot to JSON");
                await RCFUI.MessageUI.DisplayExceptionMessageAsync(ex, Resources.Progression_ExportError);
            }
        }

        /// <summary>
        /// Imports an exported save slot to the save slot
        /// </summary>
        /// <returns>The task</returns>
        public async Task ImportAsync()
        {
            // Get the input file
            var inputResult = await RCFUI.BrowseUI.BrowseFileAsync(new FileBrowserViewModel()
            {
                Title = Resources.ImportSelectionHeader,
                ExtensionFilter = new FileFilterItem("*.json", Resources.FileFilterDescription_JSON).StringRepresentation,
                DefaultName = SaveSlotFilePath.ChangeFileExtension(".json").Name
            });

            if (inputResult.CanceledByUser)
                return;

            RCFCore.Logger?.LogInformationSource($"Progression data for slot {SlotName} is being imported...");

            try
            {
                // Import
                await ImportSaveDataAsync(inputResult.SelectedFile);

                RCFCore.Logger?.LogInformationSource($"Progression data has been imported");
            }
            catch (Exception ex)
            {
                ex.HandleError("Importing JSON to progression slot file");
                await RCFUI.MessageUI.DisplayExceptionMessageAsync(ex, Resources.Progression_ImportError);

                return;
            }

            try
            {
                // Reload data
                await ProgressionViewModel.LoadDataAsync();

                RCFCore.Logger?.LogInformationSource($"Progression data has been reloaded");
            }
            catch (Exception ex)
            {
                ex.HandleError("Reload game progression view model");
            }

            await RCFUI.MessageUI.DisplaySuccessfulActionMessageAsync(Resources.Progression_ImportSuccess);
        }

        public void Dispose()
        {
            RCFCore.Data.CultureChanged -= Data_CultureChanged;
        }

        #endregion
    }
}