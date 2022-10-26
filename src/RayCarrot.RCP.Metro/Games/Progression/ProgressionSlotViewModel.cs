using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using NLog;

namespace RayCarrot.RCP.Metro;

public class ProgressionSlotViewModel : BaseRCPViewModel
{
    #region Constructors

    public ProgressionSlotViewModel(ProgressionGameViewModel game, LocalizedString? name, int index, int collectiblesCount, int totalCollectiblesCount, IEnumerable<ProgressionDataViewModel> dataItems)
    {
        Game = game;
        Name = name ?? new ResourceLocString(nameof(Resources.Progression_GenericSlot), index + 1);
        Index = index;
        CollectiblesCount = collectiblesCount;
        TotalCollectiblesCount = totalCollectiblesCount;
        Percentage = collectiblesCount / (double)totalCollectiblesCount * 100;
        DataItems = new ObservableCollection<ProgressionDataViewModel>(dataItems);
        PrimaryDataItems = new ObservableCollection<ProgressionDataViewModel>(DataItems.Where(x => x.IsPrimaryItem));
        Is100Percent = CollectiblesCount == TotalCollectiblesCount;

        ExportCommand = new AsyncRelayCommand(ExportAsync);
        ImportCommand = new AsyncRelayCommand(ImportAsync);
        EditCommand = new AsyncRelayCommand(EditAsync);
        OpenLocationCommand = new AsyncRelayCommand(OpenLocationAsync);
    }

    public ProgressionSlotViewModel(ProgressionGameViewModel game, LocalizedString? name, int index, double percentage, IEnumerable<ProgressionDataViewModel> dataItems)
    {
        Game = game;
        Name = name ?? new ResourceLocString(Resources.Progression_GenericSlot, index + 1);
        Index = index;
        CollectiblesCount = null;
        TotalCollectiblesCount = null;
        Percentage = percentage;
        DataItems = new ObservableCollection<ProgressionDataViewModel>(dataItems);
        PrimaryDataItems = new ObservableCollection<ProgressionDataViewModel>(DataItems.Where(x => x.IsPrimaryItem));
        Is100Percent = percentage >= 100;

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

    #region Private Fields

    private readonly FileSystemPath _filePath;

    #endregion

    #region Public Properties

    public ProgressionGameViewModel Game { get; }
    public LocalizedString Name { get; }
    public int Index { get; }
    public int? CollectiblesCount { get; }
    public int? TotalCollectiblesCount { get; }
    public double Percentage { get; }
    public bool Is100Percent { get; }

    public int SlotGroup { get; init; }

    public FileSystemPath FilePath
    {
        get => _filePath;
        init
        {
            _filePath = value;
            CanOpenLocation = value.FileExists;
        }
    }

    public bool CanOpenLocation { get; set; }

    public Brush ProgressBrush
    {
        get
        {
            if (Is100Percent)
                return new SolidColorBrush(Color.FromRgb(76, 175, 80));
            else if (Percentage >= 50)
                return new SolidColorBrush(Color.FromRgb(255, 238, 88));
            else
                return new SolidColorBrush(Color.FromRgb(239, 83, 80));
        }
    }

    public ObservableCollection<DuoGridItemViewModel> InfoItems { get; } = new();
    public bool HasInfoItems { get; set; }

    public ObservableCollection<ProgressionDataViewModel> PrimaryDataItems { get; }
    public ObservableCollection<ProgressionDataViewModel> DataItems { get; }

    public bool CanExport { get; set; }
    public bool CanImport { get; set; }

    #endregion

    #region Private Methods

    protected async Task<bool> ConfirmSaveEditingAsync()
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

    #region Protected Methods

    protected Task LoadInfoItemsAsync() => Task.CompletedTask;

    protected virtual void ExportSlot(FileSystemPath filePath) => throw new NotSupportedException("This slot does not support exporting slots");
    protected virtual void ImportSlot(FileSystemPath filePath) => throw new NotSupportedException("This slot does not support importing slots");

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
                    text: await Task.Run(() => FilePath.GetSize().ToString())));

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

            await LoadInfoItemsAsync();
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Error refreshing info items for {0}", gameInstallation.Id);
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
            ExportSlot(outputResult.SelectedFileLocation);

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
            ImportSlot(inputResult.SelectedFile);

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

        Logger.Trace("Progression slot for {0} is being opened for editing...", Game.GameInstallation.Id);

        try
        {
            // Create a temporary file
            using TempFile tempFile = new(false, new FileExtension(".json"));

            using HashAlgorithm sha1 = HashAlgorithm.Create();

            // Export the slot to the temp file
            ExportSlot(tempFile.TempPath);

            IEnumerable<byte> originalHash;

            // Get the original file hash
            using (var tmpFile = File.OpenRead(tempFile.TempPath))
                originalHash = sha1.ComputeHash(tmpFile);

            // Get the program to open the file with
            FileSystemPath programPath = Data.Progression_SaveEditorExe;

            // Have the user select a program if it doesn't exist
            if (!programPath.FileExists)
            {
                ProgramSelectionResult programResult = await Services.UI.GetProgramAsync(new ProgramSelectionViewModel()
                {
                    Title = Resources.Progression_SelectEditProgram,
                    FileExtensions = new FileExtension[]
                    {
                        new FileExtension(".json"),
                        new FileExtension(".txt"),
                    }
                });

                if (programResult.CanceledByUser)
                    return;

                programPath = programResult.ProgramFilePath;
                Data.Progression_SaveEditorExe = programPath;
            }

            string args = String.Empty;

            // Add specific arguments for common editor programs so that they open a new instance. If not then the new
            // process will close immediately as it will re-use the already existing one which means the WaitForExitAsync
            // won't wait for the program to close.
            if (programPath.Name == "Code.exe") // VS Code
                args += $"--new-window ";
            else if (programPath.Name == "notepad++.exe") // Notepad++
                args += $"-multiInst ";

            args += $"\"{tempFile.TempPath}\"";

            // Open the file
            using (Process? p = await Services.File.LaunchFileAsync(programPath, arguments: args))
            {
                // Ignore if the file wasn't opened
                if (p == null)
                {
                    Logger.Trace("The file was not opened");
                    return;
                }

                // Wait for the file to close...
                using (LoadState state = await App.LoaderViewModel.RunAsync(String.Format(Resources.WaitForEditorToClose, programPath.RemoveFileExtension().Name), canCancel: true))
                {
                    try
                    {
                        await p.WaitForExitAsync(state.CancellationToken);
                    }
                    catch (OperationCanceledException ex)
                    {
                        Logger.Trace(ex, "Cancelled editing progression slot");

                        try
                        {
                            // Attempt to close the process
                            p.CloseMainWindow();
                        }
                        catch (Exception ex2)
                        {
                            Logger.Warn(ex2, "Closing progression slot editor process after cancellation");
                        }
                    }
                }
            }

            // Open the temp file
            using FileStream tempFileStream = new(tempFile.TempPath, FileMode.Open, FileAccess.Read);

            // Get the new hash
            byte[] newHash = sha1.ComputeHash(tempFileStream);

            tempFileStream.Position = 0;

            // Check if the file has been modified
            if (!originalHash.SequenceEqual(newHash))
            {
                Logger.Trace("The file was modified");

                // Import the modified data
                ImportSlot(tempFile.TempPath);

                // Reload data
                await Game.LoadProgressAsync();
                await Game.LoadSlotInfoItemsAsync();
                await Game.LoadBackupAsync();

                await Services.MessageUI.DisplaySuccessfulActionMessageAsync(Resources.Progression_SaveEditSuccess);
            }
            else
            {
                Logger.Trace("The file was not modified");
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Opening progression slot file for editing");

            await Services.MessageUI.DisplayExceptionMessageAsync(ex, Resources.Archive_ViewEditFileError);
        }
    }

    public async Task OpenLocationAsync()
    {
        await Services.File.OpenExplorerLocationAsync(FilePath);
    }

    #endregion
}