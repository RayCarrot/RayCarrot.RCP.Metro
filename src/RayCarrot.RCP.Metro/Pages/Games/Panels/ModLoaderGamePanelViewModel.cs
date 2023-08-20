using System.Windows.Input;
using RayCarrot.RCP.Metro.ModLoader.Library;

namespace RayCarrot.RCP.Metro.Pages.Games;

public class ModLoaderGamePanelViewModel : GamePanelViewModel, IRecipient<ModifiedGameModsMessage>
{
    #region Constructor

    public ModLoaderGamePanelViewModel(GameInstallation gameInstallation) : base(gameInstallation)
    {
        OpenModLoaderCommand = new AsyncRelayCommand(OpenModLoaderAsync);
        OpenModCreatorCommand = new AsyncRelayCommand(OpenModCreatorAsync);

        Services.Messenger.RegisterAll(this);
    }

    #endregion

    #region Logger

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    #endregion

    #region Commands

    public ICommand OpenModLoaderCommand { get; }
    public ICommand OpenModCreatorCommand { get; }

    #endregion

    #region Public Properties

    public override GenericIconKind Icon => GenericIconKind.GamePanel_ModLoader;
    public override LocalizedString Header => "Mod Loader"; // TODO-UPDATE: Localize

    public LocalizedString? InfoText { get; set; }

    #endregion

    #region Protected Methods

    protected override async Task LoadAsyncImpl()
    {
        try
        {
            ModLibrary library = new(GameInstallation);
            ModManifest modManifest = await Task.Run(library.ReadModManifest);
            int count = modManifest.Mods.Values.Count(x => x.IsEnabled);

            // TODO-UPDATE: Localize
            if (count == 1)
                InfoText = String.Format("{0} mod applied", count);
            else
                InfoText = String.Format("{0} mods applied", count);
        }
        catch (Exception ex)
        {
            Logger.Warn(ex, "Reading mod manifest");
            InfoText = String.Empty;
        }
    }

    #endregion

    #region Public Methods

    public Task OpenModLoaderAsync() => Services.UI.ShowModLoaderAsync(GameInstallation);
    public Task OpenModCreatorAsync() => Services.UI.ShowModCreatorAsync(GameInstallation);

    public async void Receive(ModifiedGameModsMessage message)
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