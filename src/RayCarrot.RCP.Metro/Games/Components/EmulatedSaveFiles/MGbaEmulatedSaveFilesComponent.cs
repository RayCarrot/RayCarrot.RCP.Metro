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
            FileSystemPath configFile = gameClientInstallation.InstallLocation.Directory + "config.ini";

            if (configFile.FileExists)
            {
                IniData configData = new FileIniDataParser().ReadFile(configFile);
                saveDir = configData["ports.qt"]["savegamePath"];
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