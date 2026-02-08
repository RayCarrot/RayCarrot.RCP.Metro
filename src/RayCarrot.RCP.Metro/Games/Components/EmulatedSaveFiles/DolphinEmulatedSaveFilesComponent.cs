using System.IO;
using Microsoft.Win32;
using RayCarrot.RCP.Metro.Games.Clients;
using RayCarrot.RCP.Metro.Games.Structure;
using RayCarrot.RCP.Metro.Ini;

namespace RayCarrot.RCP.Metro.Games.Components;

// NOTE: Currently hard-codeded for GameCube and only using card slot A
public class DolphinEmulatedSaveFilesComponent : EmulatedSaveFilesComponent
{
    public DolphinEmulatedSaveFilesComponent() : base(GetEmulatedSaveFiles) { }

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    private static FileSystemPath GetUserDirectory(GameInstallation gameInstallation)
    {
        // https://github.com/dolphin-emu/dolphin/blob/a8fbe8f28f622851333538ee0a2899bc159161ba/Source/Core/UICommon/UICommon.cpp
        const string normalUserDirName = "Dolphin Emulator";
        const string portableUserDirName = "User";

        GameClientInstallation gameClientInstallation = Services.GameClients.GetRequiredAttachedGameClient(gameInstallation);
        FileSystemPath dolphinDir = gameClientInstallation.InstallLocation.Directory;

        // Check if it's local
        FileSystemPath portableCheckFilePath = dolphinDir + "portable.txt";
        if (portableCheckFilePath.FileExists)
            return dolphinDir + portableUserDirName;

        // Check registry key
        if (Registry.CurrentUser.OpenSubKey(@"Software\Dolphin Emulator") is { } key)
        {
            // Check if it's local
            if (key.GetValue("LocalUserConfig") != null)
                return dolphinDir + portableUserDirName;

            // Check for specific user config path
            if (key.GetValue("UserConfigPath") is string path)
                return path.Replace('/', '\\');
        }

        // Check for old user directory
        FileSystemPath oldUserDir = Environment.SpecialFolder.MyDocuments.GetFolderPath() + normalUserDirName;
        if (oldUserDir.DirectoryExists)
            return oldUserDir;

        // Check for new user directory
        FileSystemPath appDataDir = Environment.SpecialFolder.ApplicationData.GetFolderPath() + normalUserDirName;
        if (appDataDir.DirectoryExists)
            return appDataDir;

        // Default to local
        return dolphinDir + portableUserDirName;
    }

    private static string GetRegionFolderName(string gameCode)
    {
        return gameCode[3] switch
        {
            'P' => "EUR",
            'E' => "USA",
            _ => throw new Exception("Unsupported region")
        };
    }

    private static FileSystemPath GetMemoryCardDirectory(FileSystemPath userDir, FileSystemPath configFilePath, string gameCode)
    {
        string configuredFolder = IniNative.GetString(configFilePath, "Core", "GCIFolderAPath", String.Empty);

        if (configuredFolder.IsNullOrEmpty())
            return userDir + "GC" + GetRegionFolderName(gameCode) + "Card A";

        // Cut off the region folder name if it exists
        if (configuredFolder.EndsWith("EUR", StringComparison.OrdinalIgnoreCase))
            configuredFolder = configuredFolder[..^"EUR".Length];
        else if (configuredFolder.EndsWith("USA", StringComparison.OrdinalIgnoreCase))
            configuredFolder = configuredFolder[..^"USA".Length];

        return new FileSystemPath(configuredFolder) + GetRegionFolderName(gameCode);
    }

    public static IEnumerable<EmulatedSaveFile> GetEmulatedSaveFiles(GameInstallation gameInstallation)
    {
        // Get the Dolphin user directory
        FileSystemPath userDir = GetUserDirectory(gameInstallation);

        // Get the config file path
        FileSystemPath configFilePath = userDir + "Config" + "Dolphin.ini";

        int slotADeviceType = IniNative.GetInt(configFilePath, "Core", "SlotA", 255);

        // Raw memory card
        if (slotADeviceType == 1)
        {
            // TODO: Implement reading raw memory card file
        }
        // Memory card folder
        else if (slotADeviceType == 8)
        {
            if (gameInstallation.GameDescriptor.Structure.GetLayout(gameInstallation) is not GameCubeProgramLayout layout)
            {
                Logger.Warn("No matching layout found for game");
                yield break;
            }

            // Get the memory card directory
            FileSystemPath memoryCardDir = GetMemoryCardDirectory(userDir, configFilePath, layout.GameCode);

            // Return every file in the memory card directory
            if (memoryCardDir.DirectoryExists)
                foreach (FileSystemPath filePath in Directory.GetFiles(memoryCardDir, "*.gci", SearchOption.TopDirectoryOnly))
                    yield return new EmulatedGameCubeSaveFile(filePath);
        }
        // Other (not a memory card)
        else
        {
            Logger.Info("Couldn't read Dolphin memory card due to type being {0}", slotADeviceType);
        }
    }
}