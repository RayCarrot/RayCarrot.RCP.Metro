using System.Windows.Input;

namespace RayCarrot.RCP.Metro.Pages.Progression;

public class GameSlotViewModel : BaseRCPViewModel
{
    #region Constructors

    public GameSlotViewModel(GameViewModel game, GameProgressionSlot slot, bool canOpenLocation = true)
    {
        Game = game;
        Slot = slot;
        DataItems = new ObservableCollection<GameProgressionDataItem>(slot.DataItems);
        PrimaryDataItems = new ObservableCollection<GameProgressionDataItem>(slot.DataItems.Where(x => x.IsPrimaryItem));
        FilePath = slot.FilePath;
        CanOpenLocation = canOpenLocation && FilePath.FileExists;
        CanExport = slot.CanExport;
        CanImport = slot.CanImport;

        ExportCommand = new AsyncRelayCommand(ExportAsync);
        ImportCommand = new AsyncRelayCommand(ImportAsync);
        EditCommand = new AsyncRelayCommand(EditAsync);
        OpenLocationCommand = new AsyncRelayCommand(OpenLocationAsync);
    }

    #endregion

    #region Logger

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    #endregion

    #region Commands

    public ICommand ExportCommand { get; }
    public ICommand ImportCommand { get; }
    public ICommand EditCommand { get; }
    public ICommand OpenLocationCommand { get; }

    #endregion

    #region Private Properties

    private string DefaultExportName => $"{FilePath.RemoveFileExtension().Name}_{Name.Value}.json";

    #endregion

    #region Public Properties

    public GameProgressionSlot Slot { get; }
    public GameViewModel Game { get; }
    public LocalizedString Name => Slot.Name;
    public double Percentage => Slot.Percentage;
    public GameProgressionSlot.ProgressionState State => Slot.State;

    public FileSystemPath FilePath { get; }
    public bool CanOpenLocation { get; }

    public ObservableCollection<DuoGridItemViewModel> InfoItems { get; } = new();
    public bool HasInfoItems { get; set; }

    public ObservableCollection<GameProgressionDataItem> PrimaryDataItems { get; }
    public ObservableCollection<GameProgressionDataItem> DataItems { get; }

    public bool CanExport { get; set; }
    public bool CanImport { get; set; }

    #endregion

    #region Private Methods

    private async Task<bool> ConfirmSaveEditingAsync()
    {
        // Always return true if the warning has already been shown
        if (Data.Progression_ShownEditSaveWarning) 
            return true;
        
        bool confirmResult = await Services.MessageUI.DisplayMessageAsync(Resources.Progression_SaveEditWarning, Resources.Progression_SaveEditWarningHeader, MessageType.Question, true);

        if (!confirmResult)
            return false;

        Data.Progression_ShownEditSaveWarning = true;

        return true;
    }

    #endregion

    #region Public Methods

    public virtual async Task RefreshInfoItemsAsync(GameInstallation gameInstallation)
    {
        try
        {
            InfoItems.Clear();

            if (FilePath.FileExists)
            {
                InfoItems.Add(new DuoGridItemViewModel(
                    header: new ResourceLocString(nameof(Resources.Progression_SlotInfo_File)), 
                    text: FilePath.FullPath));
                InfoItems.Add(new DuoGridItemViewModel(
                    header: new ResourceLocString(nameof(Resources.Progression_SlotInfo_Size)), 
                    text: await Task.Run(() => BinaryHelpers.BytesToString(FilePath.GetSize()))));

                DateTime lastWriteTime = FilePath.GetFileInfo().LastWriteTime;

                InfoItems.Add(new DuoGridItemViewModel(
                    header: new ResourceLocString(nameof(Resources.Progression_SlotInfo_LastModified)), 
                    text: new GeneratedLocString(() => lastWriteTime.ToShortDateString())));
                HasInfoItems = true;
            }
            else
            {
                HasInfoItems = false;
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Error refreshing info items for {0}", gameInstallation.FullId);
        }
    }

    public async Task ExportAsync()
    {
        // Get the output file
        SaveFileResult outputResult = await Services.BrowseUI.SaveFileAsync(new SaveFileViewModel()
        {
            Title = Resources.ExportDestinationSelectionHeader,
            DefaultName = DefaultExportName,
            Extensions = new FileFilterItem("*.json", Resources.FileFilterDescription_JSON).StringRepresentation
        });

        if (outputResult.CanceledByUser)
            return;

        Logger.Info("Progression data for slot {0} is being exported...", Name.Value);

        try
        {
            // Export
            Slot.ExportSlot(outputResult.SelectedFileLocation);

            Logger.Info("Progression data has been exported");

            await Services.MessageUI.DisplaySuccessfulActionMessageAsync(Resources.Progression_ExportSuccess);
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Exporting progression slot to JSON");
            await Services.MessageUI.DisplayExceptionMessageAsync(ex, Resources.Progression_ExportError);
        }
    }

    public async Task ImportAsync()
    {
        if (!await ConfirmSaveEditingAsync())
            return;

        // Get the input file
        FileBrowserResult inputResult = await Services.BrowseUI.BrowseFileAsync(new FileBrowserViewModel()
        {
            Title = Resources.ImportSelectionHeader,
            ExtensionFilter = new FileFilterItem("*.json", Resources.FileFilterDescription_JSON).StringRepresentation,
            DefaultName = DefaultExportName,
        });

        if (inputResult.CanceledByUser)
            return;

        Logger.Info("Progression data for slot {0} is being imported...", Name.Value);

        try
        {
            // Import
            Slot.ImportSlot(inputResult.SelectedFile);

            Logger.Info("Progression data has been imported");
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Importing JSON to progression slot file");
            await Services.MessageUI.DisplayExceptionMessageAsync(ex, Resources.Progression_ImportError);

            return;
        }

        // Reload data
        await Game.LoadProgressAsync();
        await Game.LoadSlotInfoItemsAsync();
        await Game.LoadBackupAsync();

        await Services.MessageUI.DisplaySuccessfulActionMessageAsync(Resources.Progression_ImportSuccess);
    }

    public async Task EditAsync()
    {
        if (!await ConfirmSaveEditingAsync())
            return;

        Logger.Trace("Progression slot for {0} is being opened for editing...", Game.GameInstallation.FullId);

        // Wait for the process to close...
        using (LoadState state = await App.LoaderViewModel.RunAsync())
        {
            try
            {
                using FileEditing fileEditing = new(DefaultExportName);

                bool modified = await fileEditing.ExecuteAsync(
                    fileExtension: ".json",
                    readOnly: false,
                    state: state,
                    createFileAction: Slot.ExportSlot);

                if (modified)
                {
                    // Import the modified data
                    Slot.ImportSlot(fileEditing.FilePath);

                    // Reload data
                    await Game.LoadProgressAsync();
                    await Game.LoadSlotInfoItemsAsync();
                    await Game.LoadBackupAsync();

                    await Services.MessageUI.DisplaySuccessfulActionMessageAsync(Resources.Progression_SaveEditSuccess);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Opening progression slot file for editing");

                await Services.MessageUI.DisplayExceptionMessageAsync(ex, Resources.Archive_ViewEditFileError);
            }
        }
    }

    public async Task OpenLocationAsync()
    {
        await Services.File.OpenExplorerLocationAsync(FilePath);
    }

    #endregion
}