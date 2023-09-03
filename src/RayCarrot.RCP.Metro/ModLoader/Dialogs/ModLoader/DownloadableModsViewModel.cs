using System.Net.Http;
using System.Windows.Input;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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

        Mods = new ObservableCollection<DownloadableGameBananaModViewModel>();

        RefreshCommand = new AsyncRelayCommand(LoadModsAsync);
    }

    #endregion

    #region Constant Fields

    private const int RaymanControlPanelToolId = 10372;

    #endregion

    #region Private Fields

    private readonly HttpClient _httpClient;

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
            List<GameBananaRecord> modRecords = new();

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
                    subfeed = await ReadAsync<GameBananaSubfeed>($"https://gamebanana.com/apiv11/Game/{gameId}/Subfeed?" +
                                                                 $"_nPage={page}&" +
                                                                 $"_sSort=new&" +
                                                                 $"_csvModelInclusions=Mod");

                    // Add the mods
                    modRecords.AddRange(subfeed.Records.Where(x => x.HasFiles));

                    page++;
                } while (!subfeed.Metadata.IsComplete);
            }

            // Get data for every mod
            GameBananaMod[] mods = await ReadAsync<GameBananaMod[]>($"https://gamebanana.com/apiv11/Mod/Multi?" + 
                                                                    $"_csvRowIds={modRecords.Select(x => x.Id).JoinItems(",")}&" + 
                                                                    $"_csvProperties=_aFiles,_sDescription,_sText,_nDownloadCount,_aModManagerIntegrations");
            
            // Process every mod
            for (int i = 0; i < modRecords.Count; i++)
            {
                GameBananaRecord modRecord = modRecords[i];
                GameBananaMod mod = mods[i];

                // Make sure the mod has files
                if (mod.Files == null)
                    continue;

                // Get the files which contain valid RCP mods
                List<GameBananaFile> validFiles = mod.Files.
                    Where(x => mod.ModManagerIntegrations is JObject obj &&
                               obj.ToObject<Dictionary<string, GameBananaModManager[]>>()?.TryGetValue(x.Id.ToString(), out GameBananaModManager[] m) == true &&
                               m.Any(mm => mm.ToolId == RaymanControlPanelToolId)).
                    ToList();

                // Make sure at least one file has mod integration with RCP
                if (validFiles.Count > 0)
                    Mods.Add(new DownloadableGameBananaModViewModel(
                        gameBananaId: modRecord.Id,
                        name: modRecord.Name,
                        uploaderUserName: modRecord.Submitter?.Name ?? String.Empty,
                        uploadDate: modRecord.DateAdded,
                        description: mod.Description ?? String.Empty,
                        text: mod.Text ?? String.Empty,
                        previewMedia: modRecord.PreviewMedia,
                        likesCount: modRecord.LikeCount,
                        downloadsCount: mod.DownloadCount,
                        viewsCount: modRecord.ViewCount,
                        files: validFiles));
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