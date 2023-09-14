using System.IO;
using System.Net;
using RayCarrot.RCP.Metro.ModLoader.Extractors;

namespace RayCarrot.RCP.Metro;

public class ModFileUriLaunchHandler : UriLaunchHandler
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    public override bool DisableFullStartup => true;
    public override string UriProtocol => "rcpgp";
    public override string UriProtocolName => "Rayman Control Panel Mod Protocol";

    // TODO: Move to some general helper class
    private static string GetRedirectedUrl(string url)
    {
        HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(url);
        webRequest.Method = "HEAD";
        webRequest.AllowAutoRedirect = false;
        webRequest.Timeout = 5000; // 5 seconds

        using WebResponse webResponse = webRequest.GetResponse();
        return webResponse.Headers["Location"];
    }

    public override async void Invoke(string uri, State state)
    {
        // TODO-UPDATE: Completely change this. Have 1-click installer provider file id and mod id. Then use that to match game(s) and download as GB mod. Problem now is that downloading a mod like this won't have it support updates since the GB info is lost.

        // NOTE: Currently most errors here will cause a silent failure rather than a message to the user. Change this?

        string downloadUrlString = GetValueFromUri(uri);
        
        if (!Uri.TryCreate(downloadUrlString, UriKind.Absolute, out Uri downloadUri))
        {
            Logger.Warn("URI value '{0}' was not correctly formatted", downloadUrlString);
            return;
        }

        try
        {
            // For GameBanana the URL gets redirected to the actual download, so we want to get that URL instead
            downloadUrlString = GetRedirectedUrl(downloadUrlString);
        }
        catch (Exception ex)
        {
            // Ignore exceptions here for now and attempt to continue
            Logger.Warn(ex, "Get redirected URL");
        }

        try
        {
            FileExtension fileExtension = new(downloadUrlString);
            bool isValid = ModExtractor.GetModExtractors().Any(x => x.FileExtension == fileExtension);

            if (!isValid)
            {
                Logger.Warn("URI value '{0}' does not contain a supported file extension", downloadUrlString);
                return;
            }

            using TempDirectory tempDir = new(true);

            // Download the patch
            bool downloadResult = 
                // TODO-UPDATE: Don't use this api - do a load operation on the app view model
                await Services.App.DownloadAsync(new[] { downloadUri }, false, tempDir.TempPath);

            if (!downloadResult)
                return;

            // Show the mod loader with the downloaded files
            await Services.UI.ShowModLoaderAsync(new[] { tempDir.TempPath + Path.GetFileName(downloadUrlString) });
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Runing mod loader from URI launch");

            await Services.MessageUI.DisplayExceptionMessageAsync(ex, 
                // TODO-UPDATE: Error message
                "");
        }
    }
}