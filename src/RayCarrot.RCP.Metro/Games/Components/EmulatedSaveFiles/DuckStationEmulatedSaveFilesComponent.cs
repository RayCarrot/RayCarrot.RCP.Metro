using System.IO;
using IniParser;
using IniParser.Model;
using RayCarrot.RCP.Metro.Games.Clients;

namespace RayCarrot.RCP.Metro.Games.Components;

public class DuckStationEmulatedSaveFilesComponent : EmulatedSaveFilesComponent
{
    public DuckStationEmulatedSaveFilesComponent() : base(GetEmulatedSaveFiles) { }

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    public static IEnumerable<EmulatedSaveFile> GetEmulatedSaveFiles(GameInstallation gameInstallation)
    {
        GameClientInstallation gameClientInstallation = Services.GameClients.GetRequiredAttachedGameClient(gameInstallation);
        
        // Check if the installation is portable
        FileSystemPath portableCheckFilePath = gameClientInstallation.InstallLocation.Directory + "portable.txt";
        bool isPortable = portableCheckFilePath.FileExists;

        // Get the user directory for the emulator (see https://github.com/stenzek/duckstation?tab=readme-ov-file#user-directories)
        FileSystemPath emuDir = isPortable
            ? gameClientInstallation.InstallLocation.Directory
            : Environment.SpecialFolder.MyDocuments.GetFolderPath() + "DuckStation";

        FileSystemPath configFilePath = emuDir + "settings.ini";

        if (!configFilePath.FileExists)
        {
            Logger.Warn("DuckStation settings file not found");
            yield break;
        }

        FileSystemPath saveDir;

        try
        {
            IniData configData = new FileIniDataParser().ReadFile(configFilePath);
            saveDir = emuDir + configData["MemoryCards"]["Directory"];
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Reading DuckStation config");
            yield break;
        }

        // Easiest to check all files for saves
        foreach (FileSystemPath memCardFile in Directory.GetFiles(saveDir, "*.mcd"))
            yield return new EmulatedPs1SaveFile(memCardFile);
    }
}