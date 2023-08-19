using System.Windows.Input;
using RayCarrot.RCP.Metro.Patcher.Library;

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
        try
        {
            PatchLibrary library = new(GameInstallation);
            PatchManifest patchManifest = await Task.Run(library.ReadPatchManifest);
            int count = patchManifest.Patches.Values.Count(x => x.IsEnabled);

            if (count == 1)
                InfoText = new ResourceLocString(nameof(Resources.GameHub_PatcherPanel_InfoSingle), count);
            else
                InfoText = new ResourceLocString(nameof(Resources.GameHub_PatcherPanel_InfoMultiple), count);
        }
        catch (Exception ex)
        {
            Logger.Warn(ex, "Reading patch manifest");
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