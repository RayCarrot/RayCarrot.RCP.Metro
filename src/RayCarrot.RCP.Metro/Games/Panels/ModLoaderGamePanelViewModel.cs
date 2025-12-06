using System.Net.Http;
using System.Windows.Input;
using RayCarrot.RCP.Metro.ModLoader.Library;
using RayCarrot.RCP.Metro.ModLoader.Sources;

namespace RayCarrot.RCP.Metro.Games.Panels;

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
    public override LocalizedString Header => new ResourceLocString(nameof(Resources.ModLoader_Title));

    public LocalizedString? AppliedModsText { get; set; }
    public LocalizedString? DownloadableModsText { get; set; }
    public LocalizedString? UpdatesText { get; set; }

    #endregion

    #region Private Methods

    private static async Task<bool> CheckForModUpdateAsync(HttpClient httpClient, ModInstallInfo modInstallInfo)
    {
        DownloadableModsSource? source = DownloadableModsSource.GetSource(modInstallInfo);

        if (source == null)
            return false;

        try
        {
            ModUpdateCheckResult result = await source.CheckForUpdateAsync(httpClient, modInstallInfo);
            return result.State == ModUpdateState.UpdateAvailable;
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Checking for mod update");
            return false;
        }
    }

    #endregion

    #region Protected Methods

    protected override async Task LoadAsyncImpl()
    {
        AppliedModsText = null;
        DownloadableModsText = null;
        UpdatesText = null;

        ModManifest modManifest;
        try
        {
            // Read the mod manifest from the library
            ModLibrary library = new(GameInstallation);
            modManifest = await Task.Run(library.ReadModManifest);

            // Get the amount of applied mods
            int appliedModsCount = modManifest.Mods.Values.Count(x => x.IsEnabled);

            // TODO-LOC
            AppliedModsText = $"{appliedModsCount}/{modManifest.Mods.Count} mods applied";
        }
        catch (Exception ex)
        {
            Logger.Warn(ex, "Reading mod manifest");
            return;
        }

        // Optionally get the amount of downloadable mods
        if (Services.Data.ModLoader_ShowDownloadableModsCount)
        {
            try
            {
                using HttpClient httpClient = Services.HttpClientFactory.CreateClient();

                // Calculate the count
                int count = 0;
                foreach (DownloadableModsSource source in DownloadableModsSource.GetSources())
                    count += await source.GetDownloadableModsCountAsync(httpClient, GameInstallation);

                // TODO-LOC
                DownloadableModsText = $"{count} downloadable mods available";
            }
            catch (Exception ex)
            {
                Logger.Warn(ex, "Getting downloadable mods count");
            }
        }

        // Optionally check for updates
        if (Services.Data.ModLoader_AutomaticallyCheckForUpdates && modManifest.Mods.Count > 0)
        {
            using HttpClient httpClient = Services.HttpClientFactory.CreateClient();
            bool[] results = await Task.WhenAll(modManifest.Mods.Values.Select(x => CheckForModUpdateAsync(httpClient, x.InstallInfo)));
            int updates = results.Count(x => x);

            if (updates == 0)
            {
                UpdatesText = null;
            }
            else
            {
                // Can't show both at once, so remove this
                DownloadableModsText = null;

                if (updates == 1)
                    UpdatesText = new ResourceLocString(nameof(Resources.GameHub_ModLoaderPanel_UpdatesAvailableSingle), updates);
                else
                    UpdatesText = new ResourceLocString(nameof(Resources.GameHub_ModLoaderPanel_UpdatesAvailableMultiple), updates);
            }
        }
    }

    #endregion

    #region Public Methods

    public Task OpenModLoaderAsync() => Services.UI.ShowModLoaderAsync(GameInstallation);
    public Task OpenModCreatorAsync() => Services.UI.ShowModCreatorAsync(GameInstallation);

    public override Task UnloadAsync()
    {
        Services.Messenger.UnregisterAll(this);
        return Task.CompletedTask;
    }

    #endregion

    #region Message Receivers

    async void IRecipient<ModifiedGameModsMessage>.Receive(ModifiedGameModsMessage message)
    {
        if (message.GameInstallation == GameInstallation)
            await RefreshAsync();
    }

    #endregion
}