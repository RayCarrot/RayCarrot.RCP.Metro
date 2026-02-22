using RayCarrot.RCP.Metro.Games.Clients;
using RayCarrot.RCP.Metro.Ini;

namespace RayCarrot.RCP.Metro.Games.Components;

public class MGbaEmulatedSaveFilesComponent : EmulatedSaveFilesComponent
{
    public MGbaEmulatedSaveFilesComponent() : base(GetEmulatedSaveFiles) { }

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    public static IEnumerable<EmulatedSaveFile> GetEmulatedSaveFiles(GameInstallation gameInstallation)
    {
        FileSystemPath saveDir = FileSystemPath.EmptyPath;

        try
        {
            GameClientInstallation gameClientInstallation = Services.GameClients.GetRequiredAttachedGameClient(gameInstallation);

            // First check the install directory (if the installation is portable), then roaming app data (if not portable)
            FileSystemPath[] configFilePaths = 
            {
                gameClientInstallation.InstallLocation.Directory + "config.ini",
                Environment.SpecialFolder.ApplicationData.GetFolderPath() + "mGBA" + "config.ini",
            };

            foreach (FileSystemPath configFile in configFilePaths)
            {
                if (configFile.FileExists)
                {
                    saveDir = IniNative.GetString(configFile, "ports.qt", "savegamePath", String.Empty);
                    break;
                }
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Reading mGBA config");
        }

        if (saveDir == FileSystemPath.EmptyPath)
            saveDir = gameInstallation.InstallLocation.Directory;

        FileSystemPath saveFilePath = saveDir + gameInstallation.InstallLocation.GetRequiredFileName();
        saveFilePath = saveFilePath.ChangeFileExtension(new FileExtension(".sav"));

        if (saveFilePath.FileExists)
        {
            yield return gameInstallation.GameDescriptor.Platform switch
            {
                GamePlatform.Gbc => new EmulatedGbcSaveFile(saveFilePath),
                GamePlatform.Gba => new EmulatedGbaSaveFile(saveFilePath),
                _ => throw new InvalidOperationException("Unsupported mGBA platform")
            };
        }
    }
}