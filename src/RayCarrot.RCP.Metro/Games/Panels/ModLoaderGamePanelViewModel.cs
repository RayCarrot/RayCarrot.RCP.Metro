﻿using System.Net.Http;
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

    public LocalizedString? InfoText { get; set; }
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
        try
        {
            InfoText = null;
            UpdatesText = null;

            // Read the mod manifest from the library
            ModLibrary library = new(GameInstallation);
            ModManifest modManifest = await Task.Run(library.ReadModManifest);

            // Get the amount of applied mods
            int count = modManifest.Mods.Values.Count(x => x.IsEnabled);

            if (count == 1)
                InfoText = new ResourceLocString(nameof(Resources.GameHub_ModLoaderPanel_InfoSingle), count);
            else
                InfoText = new ResourceLocString(nameof(Resources.GameHub_ModLoaderPanel_InfoMultiple), count);

            // Check for updates if set to do so
            if (Services.Data.ModLoader_AutomaticallyCheckForUpdates)
            {
                using HttpClient httpClient = new();
                bool[] results = await Task.WhenAll(modManifest.Mods.Values.Select(x => CheckForModUpdateAsync(httpClient, x.InstallInfo)));
                int updates = results.Count(x => x);

                if (updates == 0)
                    UpdatesText = null;
                else if (updates == 1)
                    UpdatesText = new ResourceLocString(nameof(Resources.GameHub_ModLoaderPanel_UpdatesAvailableSingle), updates);
                else
                    UpdatesText = new ResourceLocString(nameof(Resources.GameHub_ModLoaderPanel_UpdatesAvailableMultiple), updates);
            }
        }
        catch (Exception ex)
        {
            Logger.Warn(ex, "Reading mod manifest");
            InfoText = null;
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