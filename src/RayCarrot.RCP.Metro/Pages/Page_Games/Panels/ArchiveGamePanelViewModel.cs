using System.Windows.Input;
using RayCarrot.RCP.Metro.Archive;
using RayCarrot.RCP.Metro.Games.Components;

namespace RayCarrot.RCP.Metro;

public class ArchiveGamePanelViewModel : GamePanelViewModel
{
    #region Constructor

    public ArchiveGamePanelViewModel(GameInstallation gameInstallation, ArchiveComponent archiveComponent) : base(gameInstallation)
    {
        ArchiveComponent = archiveComponent;
        AdditionalAction = archiveComponent.GetAdditionalAction();

        OpenArchiveExplorerCommand = new AsyncRelayCommand(OpenArchiveExplorerAsync);
        AdditionalActionCommand = new AsyncRelayCommand(AdditionalActionAsync);
    }

    #endregion

    #region Logger

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    #endregion

    #region Commands

    public ICommand OpenArchiveExplorerCommand { get; }
    public ICommand AdditionalActionCommand { get; }

    #endregion

    #region Public Properties

    public override GenericIconKind Icon => GenericIconKind.GamePanel_Archive;
    public override LocalizedString Header => new ResourceLocString(nameof(Resources.Utilities_ArchiveExplorer_Header));

    public ArchiveComponent ArchiveComponent { get; }

    public ObservableCollection<string>? TrimmedArchiveFilePaths { get; set; }
    public ObservableCollection<string>? ArchiveFilePaths { get; set; }
    public bool IsTrimmed { get; set; }

    public ArchiveComponent.AdditionalArchiveAction? AdditionalAction { get; }
    public GenericIconKind AdditionalActionIcon => AdditionalAction?.Icon ?? GenericIconKind.None;
    public LocalizedString? AdditionalActionDescription => AdditionalAction?.Description;

    #endregion

    #region Protected Methods

    protected override Task LoadAsyncImpl()
    {
        // Ideally all of this trimming and formatting shouldn't be handled here like this, but it's the easiest for now
        ArchiveFilePaths = new ObservableCollection<string>(
            ArchiveComponent.GetArchiveFilePaths().
                Where(x => (GameInstallation.InstallLocation.Directory + x).FileExists).
                Select(x => $"• {x}"));

        if (ArchiveFilePaths.Count > 3)
        {
            TrimmedArchiveFilePaths = new ObservableCollection<string>(ArchiveFilePaths.Take(2).Append("..."));
            IsTrimmed = true;
        }
        else if (ArchiveFilePaths.Count != 0)
        {
            TrimmedArchiveFilePaths = ArchiveFilePaths;
            IsTrimmed = false;
        }
        else
        {
            IsEmpty = true;
        }

        return Task.CompletedTask;
    }

    #endregion

    #region Public Methods

    public async Task OpenArchiveExplorerAsync()
    {
        using IArchiveDataManager archiveDataManager = ArchiveComponent.CreateObject();

        try
        {
            // Show the Archive Explorer
            await Services.UI.ShowArchiveExplorerAsync(
                manager: archiveDataManager,
                filePaths: ArchiveComponent.GetArchiveFilePaths().
                    Select(x => GameInstallation.InstallLocation.Directory + x).
                    Where(x => x.FileExists).
                    ToArray());
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Running Archive Explorer");

            await Services.MessageUI.DisplayExceptionMessageAsync(ex, Resources.Archive_CriticalError);
        }
    }

    public Task AdditionalActionAsync() => AdditionalAction?.Action(GameInstallation) ?? Task.CompletedTask;

    #endregion
}