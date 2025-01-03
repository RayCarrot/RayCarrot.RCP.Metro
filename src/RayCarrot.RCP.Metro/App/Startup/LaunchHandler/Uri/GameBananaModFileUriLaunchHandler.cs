﻿using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Web;
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

    // TODO: Move to some general helper class
    private static string GetRedirectedUrl(Uri url)
    {
        HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(url);
        webRequest.Method = "HEAD";
        webRequest.AllowAutoRedirect = false;
        webRequest.Timeout = 5000; // 5 seconds

        using WebResponse webResponse = webRequest.GetResponse();
        return webResponse.Headers["Location"];
    }

    private static async Task DownloadModAsync(string modUrl, long? modId, long? fileId)
    {
        // Make sure the file is valid
        FileExtension fileExtension = new(modUrl);
        bool isValid = ModExtractor.GetModExtractors().Any(x => x.FileExtension == fileExtension);

        if (!isValid)
        {
            Logger.Warn("URI value '{0}' does not contain a supported file extension", modUrl);
            return;
        }

        string fileName = Path.GetFileName(modUrl);

        await Services.UI.ShowModLoaderAsync(
            gameInstallation: null,
            modUrl: modUrl, 
            fileName: fileName,
            sourceId: modId != null && fileId != null 
                ? new GameBananaModsSource().Id 
                : null, 
            installData: modId != null && fileId != null 
                ? new GameBananaInstallData(modId.Value, fileId.Value) 
                : null);
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

        if (query["itemType"] == "Mod")
        {
            string? modIdString = query["itemId"];
            if (modIdString != null)
                modId = Int64.TryParse(modIdString, out long m) ? m : null;

            string? fileIdString = query["fileId"];
            if (fileIdString != null)
                fileId = Int64.TryParse(fileIdString, out long m) ? m : null;

            if (modId == null || fileId == null)
                Logger.Warn("Invalid query {0}", query);
        }
        else
        {
            Logger.Warn("Invalid query {0}. The item type is not mod.", query);
        }

        string redirectedDownloadUrl;
        
        try
        {
            // For GameBanana the URL gets redirected to the actual download, so we want to get that URL instead
            redirectedDownloadUrl = GetRedirectedUrl(downloadUri);
        }
        catch (Exception ex)
        {
            Logger.Warn(ex, "Get redirected URL");
            return;
        }

        await DownloadModAsync(redirectedDownloadUrl, modId, fileId);
    }
}