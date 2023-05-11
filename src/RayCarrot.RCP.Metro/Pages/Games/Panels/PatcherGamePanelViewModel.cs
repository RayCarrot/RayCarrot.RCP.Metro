using System.Windows.Input;
using BinarySerializer;
using RayCarrot.RCP.Metro.Patcher;

namespace RayCarrot.RCP.Metro.Pages.Games;

public class PatcherGamePanelViewModel : GamePanelViewModel, IRecipient<ModifiedGamePatchesMessage>
{
    #region Constructor

    public PatcherGamePanelViewModel(GameInstallation gameInstallation) : base(gameInstallation)
    {
        OpenPatcherCommand = new AsyncRelayCommand(OpenPatcherAsync);
        OpenPatchCreatorCommand = new AsyncRelayCommand(OpenPatchCreatorAsync);

        Services.Messenger.RegisterAll(this);
    }

    #endregion

    #region Logger

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    #endregion

    #region Commands

    public ICommand OpenPatcherCommand { get; }
    public ICommand OpenPatchCreatorCommand { get; }

    #endregion

    #region Public Properties

    public override GenericIconKind Icon => GenericIconKind.GamePanel_Patcher;
    public override LocalizedString Header => new ResourceLocString(nameof(Resources.Patcher_Title));

    public LocalizedString? InfoText { get; set; }

    #endregion

    #region Protected Methods

    protected override async Task LoadAsyncImpl()
    {
        // Get applied patches
        using Context context = new RCPContext(String.Empty);
        PatchLibrary library = new(GameInstallation.InstallLocation.Directory, Services.File);

        try
        {
            PatchLibraryFile? libraryFile = await Task.Run(() => context.ReadFileData<PatchLibraryFile>(library.LibraryFilePath));
            InfoText = new ResourceLocString(nameof(Resources.GameHub_PatcherPanel_Info), libraryFile?.Patches.Count(x => x.IsEnabled) ?? 0);
        }
        catch (Exception ex)
        {
            Logger.Warn(ex, "Reading patch library");
            InfoText = String.Empty;
        }
    }

    #endregion

    #region Public Methods

    public Task OpenPatcherAsync() => Services.UI.ShowPatcherAsync(GameInstallation);
    public Task OpenPatchCreatorAsync() => Services.UI.ShowPatchCreatorAsync(GameInstallation);

    public async void Receive(ModifiedGamePatchesMessage message)
    {
        if (message.GameInstallation == GameInstallation)
            await RefreshAsync();
    }

    public override Task UnloadAsync()
    {
        Services.Messenger.UnregisterAll(this);
        return Task.CompletedTask;
    }

    #endregion
}