using System.Windows.Input;
using RayCarrot.RCP.Metro.Archive;

namespace RayCarrot.RCP.Metro;

public class ArchiveGamePanelViewModel : GamePanelViewModel
{
    #region Constructor

    public ArchiveGamePanelViewModel(GameInstallation gameInstallation) : base(gameInstallation)
    {
        OpenArchiveExplorerCommand = new AsyncRelayCommand(OpenArchiveExplorerAsync);
    }

    #endregion

    #region Logger

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    #endregion

    #region Commands

    public ICommand OpenArchiveExplorerCommand { get; }

    #endregion

    #region Public Properties

    public override GenericIconKind Icon => GenericIconKind.GamePanel_Archive;
    public override LocalizedString Header => new ResourceLocString(nameof(Resources.Utilities_ArchiveExplorer_Header));

    public ObservableCollection<string>? TrimmedArchiveFilePaths { get; set; }
    public ObservableCollection<string>? ArchiveFilePaths { get; set; }
    public bool IsTrimmed { get; set; }

    #endregion

    #region Protected Methods

    protected override Task LoadAsyncImpl()
    {
        // Ideally all of this trimming and formatting shouldn't be handled here like this, but it's the easiest for now
        ArchiveFilePaths = new ObservableCollection<string>(
            GameDescriptor.GetArchiveFilePaths(GameInstallation).
                Where(x => (GameInstallation.InstallLocation + x).FileExists).
                Select(x => $"• {x}"));

        if (ArchiveFilePaths.Count > 3)
        {
            TrimmedArchiveFilePaths = new ObservableCollection<string>(ArchiveFilePaths.Take(2).Append("..."));
            IsTrimmed = true;
        }
        else
        {
            TrimmedArchiveFilePaths = ArchiveFilePaths;
            IsTrimmed = false;
        }

        return Task.CompletedTask;
    }

    #endregion

    #region Public Methods

    public async Task OpenArchiveExplorerAsync()
    {
        using IArchiveDataManager? archiveDataManager = GameDescriptor.GetArchiveDataManager(GameInstallation);

        if (archiveDataManager == null)
        {
            Logger.Error("Archive data manager is null for {0}", GameDescriptor.GameId);
            return;
        }

        try
        {
            // Show the Archive Explorer
            await Services.UI.ShowArchiveExplorerAsync(
                manager: archiveDataManager,
                filePaths: GameDescriptor.GetArchiveFilePaths(GameInstallation).
                    Select(x => GameInstallation.InstallLocation + x).
                    Where(x => x.FileExists).
                    ToArray());
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Running Archive Explorer");

            await Services.MessageUI.DisplayExceptionMessageAsync(ex, Resources.Archive_CriticalError);
        }
    }

    #endregion
}