using Newtonsoft.Json.Linq;
using RayCarrot.RCP.Metro.Games.Clients;

namespace RayCarrot.RCP.Metro.Games.Components;

public class BizHawkEmulatedSaveFilesComponent : EmulatedSaveFilesComponent
{
    public BizHawkEmulatedSaveFilesComponent() : base(GetEmulatedSaveFiles) { }

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    public static IEnumerable<EmulatedSaveFile> GetEmulatedSaveFiles(GameInstallation gameInstallation)
    {
        GameClientInstallation gameClientInstallation = Services.GameClients.GetRequiredAttachedGameClient(gameInstallation);
        
        // Check if the installation is portable
        FileSystemPath configFilePath = gameClientInstallation.InstallLocation.Directory + "config.ini";

        if (!configFilePath.FileExists)
        {
            Logger.Warn("BizHawk settings file not found");
            yield break;
        }

        FileSystemPath saveFile;

        try
        {
            JObject configData = JsonHelpers.DeserializeFromFile<JObject>(configFilePath);
            PathEntry[]? paths = configData["PathEntries"]?["Paths"]?.ToObject<PathEntry[]>();

            if (paths == null)
            {
                Logger.Warn("BizHawk settings file does not contain any paths");
                yield break;
            }

            string system = gameInstallation.GameDescriptor.Platform switch
            {
                GamePlatform.Ps1 => "PSX",
                GamePlatform.Gbc => "GB_GBC_SGB",
                GamePlatform.Gba => "GBA",
                _ => throw new InvalidOperationException("Unsupported BizHawk platform")
            };

            string? basePath = paths.FirstOrDefault(x => x.System == system && x.Type == "Base")?.Path;

            if (basePath == null)
            {
                Logger.Warn("BizHawk settings file does not contain a base path for the current system");
                yield break;
            }

            string? savePath = paths.FirstOrDefault(x => x.System == system && x.Type == "Save RAM")?.Path;

            if (savePath == null)
            {
                Logger.Warn("BizHawk settings file does not contain a save path for the current system");
                yield break;
            }

            FileSystemPath saveDir = gameClientInstallation.InstallLocation.Directory + basePath + savePath;
            saveFile = saveDir + gameInstallation.InstallLocation.GetRequiredFileName();
            saveFile = saveFile.ChangeFileExtension(new FileExtension(""));
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Reading BizHawk config");
            yield break;
        }

        yield return gameInstallation.GameDescriptor.Platform switch
        {
            GamePlatform.Ps1 => new EmulatedPs1SaveFile(saveFile),
            GamePlatform.Gbc => new EmulatedGbcSaveFile(saveFile),
            GamePlatform.Gba => new EmulatedGbaSaveFile(saveFile),
            _ => throw new InvalidOperationException("Unsupported BizHawk platform")
        };
    }

    private record PathEntry(string Type, string Path, string System);
}