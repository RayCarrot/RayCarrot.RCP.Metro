#nullable disable
using RayCarrot.IO;
using RayCarrot.UI;
using System;
using System.Threading.Tasks;
using System.Windows.Input;
using NLog;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// View model for a progression slot item
/// </summary>
public abstract class GameProgression_BaseSlotViewModel : BaseRCPViewModel, IDisposable
{
    #region Constructor

    /// <summary>
    /// Default constructor
    /// </summary>
    /// <param name="slotName">The slot name</param>
    /// <param name="items">The progression info items</param>
    /// <param name="saveSlotFilePath">The file path for the save slot</param>
    /// <param name="progressionViewModel">The progression view model containing this slot</param>
    protected GameProgression_BaseSlotViewModel(LocalizedString slotName, GameProgression_InfoItemViewModel[] items, FileSystemPath saveSlotFilePath, GameProgression_BaseViewModel progressionViewModel)
    {
        SlotName = slotName;
        Items = items;
        SaveSlotFilePath = saveSlotFilePath;
        ProgressionViewModel = progressionViewModel;

        ExportCommand = new AsyncRelayCommand(ExportAsync);
        ImportCommand = new AsyncRelayCommand(ImportAsync);
    }

    #endregion

    #region Logger

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    #endregion

    #region Commands

    public ICommand ExportCommand { get; }

    public ICommand ImportCommand { get; }

    #endregion

    #region Public Properties

    /// <summary>
    /// The slot name
    /// </summary>
    public LocalizedString SlotName { get; }

    /// <summary>
    /// The file path for the save slot
    /// </summary>
    public FileSystemPath SaveSlotFilePath { get; }

    /// <summary>
    /// The progression info items
    /// </summary>
    public GameProgression_InfoItemViewModel[] Items { get; }

    /// <summary>
    /// The progression view model containing this slot
    /// </summary>
    public GameProgression_BaseViewModel ProgressionViewModel { get; }

    #endregion

    #region Public Abstract Properties

    /// <summary>
    /// Indicates if the slot can be exported/imported
    /// </summary>
    public abstract bool CanModify { get; }

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
        var outputResult = await Services.BrowseUI.SaveFileAsync(new SaveFileViewModel()
        {
            Title = Resources.ExportDestinationSelectionHeader,
            DefaultName = SaveSlotFilePath.ChangeFileExtension(new FileExtension(".json")).Name,
            Extensions = new FileFilterItem("*.json", Resources.FileFilterDescription_JSON).StringRepresentation
        });

        if (outputResult.CanceledByUser)
            return;

        Logger.Info("Progression data for slot {0} is being exported...", SlotName);

        try
        {
            // Export
            await ExportSaveDataAsync(outputResult.SelectedFileLocation);

            Logger.Info("Progression data has been exported");

            await Services.MessageUI.DisplaySuccessfulActionMessageAsync(Resources.Progression_ExportSuccess);
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Exporting progression slot to JSON");
            await Services.MessageUI.DisplayExceptionMessageAsync(ex, Resources.Progression_ExportError);
        }
    }

    /// <summary>
    /// Imports an exported save slot to the save slot
    /// </summary>
    /// <returns>The task</returns>
    public async Task ImportAsync()
    {
        // Get the input file
        var inputResult = await Services.BrowseUI.BrowseFileAsync(new FileBrowserViewModel()
        {
            Title = Resources.ImportSelectionHeader,
            ExtensionFilter = new FileFilterItem("*.json", Resources.FileFilterDescription_JSON).StringRepresentation,
            DefaultName = SaveSlotFilePath.ChangeFileExtension(new FileExtension(".json")).Name
        });

        if (inputResult.CanceledByUser)
            return;

        Logger.Info("Progression data for slot {0} is being imported...", SlotName);

        try
        {
            // Import
            await ImportSaveDataAsync(inputResult.SelectedFile);

            Logger.Info("Progression data has been imported");
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Importing JSON to progression slot file");
            await Services.MessageUI.DisplayExceptionMessageAsync(ex, Resources.Progression_ImportError);

            return;
        }

        try
        {
            // Reload data
            await ProgressionViewModel.LoadDataAsync();

            Logger.Info("Progression data has been reloaded");
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Reload game progression view model");
        }

        await Services.MessageUI.DisplaySuccessfulActionMessageAsync(Resources.Progression_ImportSuccess);
    }

    public void Dispose()
    {
        SlotName?.Dispose();
        Items?.DisposeAll();
    }

    #endregion
}