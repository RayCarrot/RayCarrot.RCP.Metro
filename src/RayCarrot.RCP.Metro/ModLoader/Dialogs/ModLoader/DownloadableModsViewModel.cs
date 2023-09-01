﻿using System.Net.Http;
using System.Windows.Input;
using Newtonsoft.Json;
using RayCarrot.RCP.Metro.GameBanana;
using RayCarrot.RCP.Metro.Games.Components;

namespace RayCarrot.RCP.Metro.ModLoader.Dialogs.ModLoader;

// TODO-UPDATE: Add logging here and other places
public class DownloadableModsViewModel : BaseViewModel
{
    #region Constructor

    public DownloadableModsViewModel(GameInstallation gameInstallation, HttpClient httpClient)
    {
        GameInstallation = gameInstallation;
        _httpClient = httpClient;
        _gameBananaApi = new GameBananaApi();

        Mods = new ObservableCollection<DownloadableGameBananaModViewModel>();

        RefreshCommand = new AsyncRelayCommand(LoadModsAsync);
    }

    #endregion

    #region Private Fields

    private readonly HttpClient _httpClient;
    private readonly GameBananaApi _gameBananaApi;

    #endregion

    #region Commands

    public ICommand RefreshCommand { get; }

    #endregion

    #region Public Properties

    public GameInstallation GameInstallation { get; }

    public DownloadableGameBananaModViewModel? SelectedMod { get; set; }
    public ObservableCollection<DownloadableGameBananaModViewModel> Mods { get; }
    public bool IsEmpty { get; set; }
    public bool IsLoading { get; set; }
    public string? ErrorMessage { get; set; }

    #endregion

    #region Private Methods

    private async Task<T> ReadAsync<T>(string url)
    {
        string jsonString = await _httpClient.GetStringAsync(url);
        return JsonConvert.DeserializeObject<T>(jsonString) ?? throw new Exception("The retrieved JSON response was null");
    }

    #endregion

    #region Public Methods

    public async Task LoadModsAsync()
    {
        if (IsLoading)
            return;

        IsLoading = true;
        IsEmpty = false;
        ErrorMessage = null;
        Mods.Clear();

        try
        {
            // Enumerate every supported GameBanana game
            foreach (GameBananaGameComponent gameBananaGameComponent in GameInstallation.GetComponents<GameBananaGameComponent>())
            {
                int gameId = gameBananaGameComponent.GameId;

                GameBananaSubfeed subfeed;
                int page = 1;

                // Add every mod from every available page. This might become inefficient if there
                // are a lot of mods to load, but currently it's not a problem.
                do
                {
                    // Read the subfeed page
                    subfeed = await ReadAsync<GameBananaSubfeed>(_gameBananaApi.GetGameSubfeedUrl(gameId, page));

                    // Process every mod in the page
                    foreach (GameBananaRecord modRecord in subfeed.Records.Where(x => x.HasFiles))
                    {
                        // Read the mod profile
                        GameBananaMod modProfile = await ReadAsync<GameBananaMod>(_gameBananaApi.GetModUrl(modRecord.Id));
                        
                        // Make sure the mod has files
                        if (modProfile.Files == null)
                            continue;

                        // Get the files which contain valid RCP mods
                        List<GameBananaFile> validFiles = modProfile.Files.
                            Where(x => x.ModManagerIntegrations != null && 
                                       x.ModManagerIntegrations.Any(m => m.ToolId == _gameBananaApi.RaymanControlPanelToolId)).
                            ToList();

                        // Make sure at least one file has mod integration with RCP
                        if (validFiles.Count > 0)
                            Mods.Add(new DownloadableGameBananaModViewModel(modProfile, validFiles));
                    }

                    page++;
                } while (!subfeed.Metadata.IsComplete);
            }

            IsEmpty = !Mods.Any();
        }
        catch (Exception ex)
        {
            // TODO-UPDATE: Log
            ErrorMessage = ex.Message;
            IsEmpty = false;
        }
        finally
        {
            IsLoading = false;
        }
    }

    #endregion
}