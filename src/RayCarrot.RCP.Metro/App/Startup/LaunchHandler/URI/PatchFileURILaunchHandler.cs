using System;
using System.IO;
using System.Linq;
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
        // NOTE: Currently most errors here will cause a silent failure rather than a message to the user. Change this?

        string value = GetValue(uri);
        
        if (!Uri.TryCreate(value, UriKind.Absolute, out Uri patchUri))
        {
            Logger.Warn("URI value '{0}' was not correctly formatted", value);
            return;
        }

        try
        {
            string ext = Path.GetExtension(uri);
            FileType fileType = FileType.Unknown;

            if (ext.Equals(".zip", StringComparison.OrdinalIgnoreCase))
                fileType = FileType.Zip;
            else if (ext.Equals(PatchFile.FileExtension, StringComparison.OrdinalIgnoreCase))
                fileType = FileType.GamePatch;

            if (fileType == FileType.Unknown)
            {
                Logger.Warn("URI value '{0}' does not contain a supported file extension", value);
                return;
            }

            using TempDirectory tempDir = new(true);

            bool isCompressed = fileType == FileType.Zip;

            // Download the patch
            bool result = await Services.App.DownloadAsync(new[] { patchUri }, isCompressed, tempDir.TempPath);

            if (!result)
                return;

            string[] patchFiles = Directory.GetFiles(tempDir.TempPath, $"*{PatchFile.FileExtension}", SearchOption.AllDirectories);

            if (patchFiles.Length == 0)
            {
                Logger.Warn("No patch files were found in the download");
                return;
            }

            // Show the Patcher
            await Services.UI.ShowPatcherAsync(patchFiles.Select(x => new FileSystemPath(x)).ToArray());
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Runing Patcher from URI launch");

            await Services.MessageUI.DisplayExceptionMessageAsync(ex, Resources.Patcher_CriticalError);
        }
    }

    private enum FileType
    {
        Unknown,
        Zip,
        GamePatch,
    }
}