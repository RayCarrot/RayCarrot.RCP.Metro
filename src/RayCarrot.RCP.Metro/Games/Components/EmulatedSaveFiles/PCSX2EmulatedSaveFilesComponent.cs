using System.IO;
using RayCarrot.RCP.Metro.Games.Clients;
using RayCarrot.RCP.Metro.Games.Structure;
using RayCarrot.RCP.Metro.Ini;

namespace RayCarrot.RCP.Metro.Games.Components;

public class PCSX2EmulatedSaveFilesComponent : EmulatedSaveFilesComponent
{
    public PCSX2EmulatedSaveFilesComponent() : base(GetEmulatedSaveFiles) { }

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    public static IEnumerable<EmulatedSaveFile> GetEmulatedSaveFiles(GameInstallation gameInstallation)
    {
        GameClientInstallation gameClientInstallation = Services.GameClients.GetRequiredAttachedGameClient(gameInstallation);

        if (gameInstallation.GameDescriptor.Structure.GetLayout(gameInstallation) is not Ps2DiscProgramLayout layout)
        {
            Logger.Warn("No matching layout found for game");
            yield break;
        }

        // Check if the installation is portable
        FileSystemPath portableCheckFilePath = gameClientInstallation.InstallLocation.Directory + "portable.ini";
        bool isPortable = portableCheckFilePath.FileExists;

        // Get the user directory for the emulator
        FileSystemPath emuDir = isPortable
            ? gameClientInstallation.InstallLocation.Directory
            : Environment.SpecialFolder.MyDocuments.GetFolderPath() + "PCSX2";

        FileSystemPath mainConfigFilePath = emuDir + "inis" + "PCSX2.ini";

        if (!mainConfigFilePath.FileExists)
        {
            Logger.Warn("PCSX2 settings file not found");
            yield break;
        }

        // Games can override settings, so attempt to find a game-specific config
        FileSystemPath gameSettingsDir = emuDir + "gamesettings";
        FileSystemPath gameConfigFilePath = FileSystemPath.EmptyPath;
        if (gameSettingsDir.DirectoryExists)
            // Search for first matching file since we don't have the hash (the file name is set as {productcode}_hash.ini)
            gameConfigFilePath = Directory.EnumerateFiles(gameSettingsDir,
                $"{layout.ProductCode}_*.ini", SearchOption.TopDirectoryOnly).
                FirstOrDefault();

        FileSystemPath saveDir;
        string memcard1;
        string memcard2;

        try
        {
            saveDir = emuDir + readIniString("Folders", "MemoryCards");

            if (readIniString("MemoryCards", "Slot1_Enable").
                Equals("true", StringComparison.InvariantCultureIgnoreCase))
                memcard1 = readIniString("MemoryCards", "Slot1_Filename");
            else
                memcard1 = String.Empty;

            if (readIniString("MemoryCards", "Slot2_Enable").
                Equals("true", StringComparison.InvariantCultureIgnoreCase))
                memcard2 = readIniString("MemoryCards", "Slot2_Filename");
            else
                memcard2 = String.Empty;
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Reading PCXSX2 config");
            yield break;
        }

        if (!memcard1.IsNullOrWhiteSpace())
            yield return new EmulatedPs2SaveFile(saveDir + memcard1);
        if (!memcard2.IsNullOrWhiteSpace())
            yield return new EmulatedPs2SaveFile(saveDir + memcard2);

        string readIniString(string appName, string keyName)
        {
            // First check the game config if it exists
            if (gameConfigFilePath.FileExists)
            {
                string value = IniNative.GetString(gameConfigFilePath, appName, keyName, String.Empty);

                if (value != String.Empty)
                    return value;
            }

            // Fall-back to the main config
            return IniNative.GetString(mainConfigFilePath, appName, keyName, String.Empty);
        }
    }
}