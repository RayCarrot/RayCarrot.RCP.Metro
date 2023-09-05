using System.Net.Http;
using Newtonsoft.Json.Linq;
using RayCarrot.RCP.Metro.Games.Components;
using RayCarrot.RCP.Metro.ModLoader.Dialogs.ModLoader;

namespace RayCarrot.RCP.Metro.ModLoader.Sources.GameBanana;

public class GameBananaModsSource : DownloadableModsSource
{
    #region Constant Fields

    private const int RaymanControlPanelToolId = 10372;

    #endregion

    #region Public Properties

    public override string Id => "GameBanana";
    public override ModSourceIconAsset Icon => ModSourceIconAsset.GameBanana;

    #endregion

    #region Public Methods

    public override async Task LoadDownloadableModsAsync(
        ModLoaderViewModel modLoaderViewModel,
        HttpClient httpClient, 
        GameInstallation gameInstallation, 
        ObservableCollection<DownloadableModViewModel> modsCollection)
    {
        List<GameBananaRecord> modRecords = new();

        // TODO-UPDATE: Maybe we should load in batches if there are too many mods? Pages?

        // Enumerate every supported GameBanana game
        foreach (GameBananaGameComponent gameBananaGameComponent in gameInstallation.GetComponents<GameBananaGameComponent>())
        {
            int gameId = gameBananaGameComponent.GameId;

            GameBananaSubfeed subfeed;
            int page = 1;

            // Add every mod from every available page. This might become inefficient if there
            // are a lot of mods to load, but currently it's not a problem.
            do
            {
                // Read the subfeed page
                subfeed = await httpClient.GetDeserializedAsync<GameBananaSubfeed>(
                    $"https://gamebanana.com/apiv11/Game/{gameId}/Subfeed?" +
                    $"_nPage={page}&" +
                    $"_sSort=new&" +
                    $"_csvModelInclusions=Mod");

                // Add the mods
                modRecords.AddRange(subfeed.Records.Where(x => x.HasFiles));

                page++;
            } while (!subfeed.Metadata.IsComplete);
        }

        // Get data for every mod
        GameBananaMod[] mods = await httpClient.GetDeserializedAsync<GameBananaMod[]>(
            $"https://gamebanana.com/apiv11/Mod/Multi?" +
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
                modsCollection.Add(new GameBananaModViewModel(
                    downloadableModsSource: this,
                    modLoaderViewModel: modLoaderViewModel,
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
    }

    #endregion
}