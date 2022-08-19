using System;
using System.IO;
using NLog;
using RayCarrot.RCP.Metro.Patcher;

namespace RayCarrot.RCP.Metro;

public class PatchFileURILaunchHandler : URILaunchHandler
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    public override bool DisableFullStartup => true;

    public override string BaseURI => PatchFile.URIProtocol;

    public override async void Invoke(string uri, State state)
    {
        string value = GetValue(uri);
        
        if (!Uri.TryCreate(value, UriKind.Absolute, out Uri patchUri))
        {
            Logger.Warn("URI value '{0}' was not correctly formatted", value);
            return;
        }

        try
        {
            using TempDirectory tempDir = new(true);

            // Download the patch
            bool result = await Services.App.DownloadAsync(new[] { patchUri }, false, tempDir.TempPath);

            if (!result)
                return;

            // Due to how the downloading system currently works we need to get the path like this
            FileSystemPath patchFilePath = tempDir.TempPath + Path.GetFileName(patchUri.AbsoluteUri);

            // Show the Patcher
            await Services.UI.ShowPatcherAsync(patchFilePath);
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Runing Patcher from URI launch");

            await Services.MessageUI.DisplayExceptionMessageAsync(ex, Resources.Patcher_CriticalError);
        }
    }
}