using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Web;
using RayCarrot.RCP.Metro.Games.Components;
using RayCarrot.RCP.Metro.ModLoader.Extractors;
using RayCarrot.RCP.Metro.ModLoader.Sources.GameBanana;

namespace RayCarrot.RCP.Metro;

public class GameBananaModFileUriLaunchHandler : UriLaunchHandler
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    public override LocalizedString DisplayName => new ResourceLocString(nameof(Resources.UriLaunchHandler_GameBananaMod));
    public override bool DisableFullStartup => true;
    
    public override string UriProtocol => "rcpgp";
    public override string UriProtocolName => "Rayman Control Panel Mod Protocol";

    // TODO: Move to some general helper class, use httpclient and have be async
    private static string GetRedirectedUrl(string url)
    {
        HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(url);
        webRequest.Method = "HEAD";
        webRequest.AllowAutoRedirect = false;
        webRequest.Timeout = 5000; // 5 seconds

        using WebResponse webResponse = webRequest.GetResponse();
        return webResponse.Headers["Location"];
    }

    // Attempts to find the file size for a URL by also dealing with redirects
    private static long? GetFileSize(string url)
    {
        while (true)
        {
            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(url);
            webRequest.Method = "HEAD";
            webRequest.AllowAutoRedirect = false;
            webRequest.Timeout = 5000; // 5 seconds

            using WebResponse webResponse = webRequest.GetResponse();
            string? location = webResponse.Headers.Get("Location");

            if (location == null || location == url)
            {
                string? contentLength = webResponse.Headers.Get("Content-Length");

                if (contentLength == null)
                    return null;

                return Int64.TryParse(contentLength, out long length) ? length : null;
            }

            url = location;
        }
    }

    private static async Task<GameInstallation?> FindGameInstallationAsync(long? gameId)
    {
        List<GameInstallation> gameInstallations = Services.Games.GetInstalledGames().ToList();

        // Filter by the GameBanana game id
        if (gameId != null)
        {
            gameInstallations = gameInstallations.
                Where(x => x.GetComponents<GameBananaGameComponent>().Any(g => g.GameId == gameId)).
                ToList();
        }

        // Make sure there is at least one available game
        if (!gameInstallations.Any())
            return null;

        // If there is more than 1 matching game we ask the user which one to patch
        if (gameInstallations.Count > 1)
        {
            GamesSelectionResult result = await Services.UI.SelectGamesAsync(new GamesSelectionViewModel(gameInstallations)
            {
                Title = Resources.ModLoader_SelectInstallTargetTitle
            });

            if (result.CanceledByUser)
                return null;

            return result.SelectedGame;
        }
        else
        {
            return gameInstallations.First();
        }
    }

    private static async Task DownloadModAsync(string modUrl, long? modId, long? fileId, long? gameId)
    {
        try
        {
            // For GameBanana the URL gets redirected to the actual download, so
            // we want to get that URL instead so we can get the proper file name
            modUrl = GetRedirectedUrl(modUrl);
        }
        catch (Exception ex)
        {
            Logger.Warn(ex, "Get redirected URL");
            return;
        }

        // Make sure the file is valid
        FileExtension fileExtension = new(modUrl);
        bool isValid = ModExtractor.GetModExtractors().Any(x => x.FileExtension == fileExtension);

        if (!isValid)
        {
            Logger.Warn("URI value '{0}' does not contain a supported file extension", modUrl);
            return;
        }

        // Find the matching game installation for the mod
        GameInstallation? gameInstallation = await FindGameInstallationAsync(gameId);

        if (gameInstallation == null)
        {
            Logger.Info("No valid game installation found");
            return;
        }

        string fileName = Path.GetFileName(modUrl);
        long? fileSize = GetFileSize(modUrl);

        await Services.UI.ShowModLoaderAsync(gameInstallation, async x =>
        {
            bool validGameBananaMod = modId != null && fileId != null;
            GameBananaModsSource? source = validGameBananaMod ? new GameBananaModsSource() : null;
            GameBananaInstallData? installData = validGameBananaMod ? new GameBananaInstallData(modId!.Value, fileId!.Value) : null;

            await x.InstallModFromDownloadableFileAsync(
                source: source,
                existingMod: null,
                fileName: fileName,
                downloadUrl: modUrl,
                fileSize: fileSize,
                installData: installData,
                modName: fileName); // NOTE: We don't have the mod name, so just show the file name
        });
    }

    public override async void Invoke(string uri, State state)
    {
        // NOTE: Currently most errors here will cause a silent failure rather than a message to the user. Change this?

        // The value will be formatted as: url?itemType={itemType}&itemId={itemid}&fileId={fileId}. This is to preserve
        // backwards compatibility with older versions of RCP.
        string value = GetValueFromUri(uri);
        
        // Parse the uri
        if (!Uri.TryCreate(value, UriKind.Absolute, out Uri downloadUri))
        {
            Logger.Warn("URI value '{0}' was not correctly formatted", value);
            return;
        }

        // Parse the query
        NameValueCollection query = HttpUtility.ParseQueryString(downloadUri.Query);
        long? modId = null;
        long? fileId = null;
        long? gameId = null;

        if (query["itemType"] == "Mod")
        {
            string? modIdString = query["itemId"];
            if (modIdString != null)
                modId = Int64.TryParse(modIdString, out long m) ? m : null;

            string? fileIdString = query["fileId"];
            if (fileIdString != null)
                fileId = Int64.TryParse(fileIdString, out long m) ? m : null;

            string? gameIdString = query["gameId"];
            if (gameIdString != null)
                gameId = Int64.TryParse(gameIdString, out long m) ? m : null;

            if (modId == null || fileId == null || gameId == null)
                Logger.Warn("Invalid query {0}", query);
        }
        else
        {
            Logger.Warn("Invalid query {0}. The item type is not mod.", query);
        }

        await DownloadModAsync(downloadUri.AbsoluteUri, modId, fileId, gameId);
    }
}