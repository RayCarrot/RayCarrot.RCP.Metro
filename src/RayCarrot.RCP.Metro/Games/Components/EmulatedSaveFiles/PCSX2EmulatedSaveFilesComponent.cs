using RayCarrot.RCP.Metro.Games.Clients;
using RayCarrot.RCP.Metro.Ini;

namespace RayCarrot.RCP.Metro.Games.Components;

public class PCSX2EmulatedSaveFilesComponent : EmulatedSaveFilesComponent
{
    public PCSX2EmulatedSaveFilesComponent() : base(GetEmulatedSaveFiles) { }

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    public static IEnumerable<EmulatedSaveFile> GetEmulatedSaveFiles(GameInstallation gameInstallation)
    {
        GameClientInstallation gameClientInstallation = Services.GameClients.GetRequiredAttachedGameClient(gameInstallation);
        
        // Check if the installation is portable
        FileSystemPath portableCheckFilePath = gameClientInstallation.InstallLocation.Directory + "portable.ini";
        bool isPortable = portableCheckFilePath.FileExists;

        // Get the user directory for the emulator
        FileSystemPath emuDir = isPortable
            ? gameClientInstallation.InstallLocation.Directory
            : Environment.SpecialFolder.MyDocuments.GetFolderPath() + "PCSX2";

        FileSystemPath configFilePath = emuDir + "inis" + "PCSX2.ini";

        if (!configFilePath.FileExists)
        {
            Logger.Warn("PCSX2 settings file not found");
            yield break;
        }

        FileSystemPath saveDir;
        string memcard1;
        string memcard2;

        try
        {
            saveDir = emuDir + IniNative.GetString(configFilePath, "Folders", "MemoryCards", String.Empty);

            if (IniNative.GetString(configFilePath, "MemoryCards", "Slot1_Enable", String.Empty).
                Equals("true", StringComparison.InvariantCultureIgnoreCase))
                memcard1 = IniNative.GetString(configFilePath, "MemoryCards", "Slot1_Filename", String.Empty);
            else
                memcard1 = String.Empty;

            if (IniNative.GetString(configFilePath, "MemoryCards", "Slot2_Enable", String.Empty).
                Equals("true", StringComparison.InvariantCultureIgnoreCase))
                memcard2 = IniNative.GetString(configFilePath, "MemoryCards", "Slot2_Filename", String.Empty);
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
    }
}