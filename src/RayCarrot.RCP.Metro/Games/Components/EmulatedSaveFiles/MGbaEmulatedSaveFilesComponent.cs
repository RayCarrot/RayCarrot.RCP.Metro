using IniParser;
using IniParser.Model;
using RayCarrot.RCP.Metro.Games.Clients;

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
                    IniData configData = new FileIniDataParser().ReadFile(configFile);
                    saveDir = configData["ports.qt"]["savegamePath"];
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
            yield return new EmulatedGbaSaveFile(saveFilePath);
    }
}