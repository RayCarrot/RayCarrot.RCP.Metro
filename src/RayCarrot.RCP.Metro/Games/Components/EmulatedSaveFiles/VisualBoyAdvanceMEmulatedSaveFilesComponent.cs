using RayCarrot.RCP.Metro.Ini;

namespace RayCarrot.RCP.Metro.Games.Components;

public class VisualBoyAdvanceMEmulatedSaveFilesComponent : EmulatedSaveFilesComponent
{
    public VisualBoyAdvanceMEmulatedSaveFilesComponent() : base(GetEmulatedSaveFiles) { }

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    public static IEnumerable<EmulatedSaveFile> GetEmulatedSaveFiles(GameInstallation gameInstallation)
    {
        FileSystemPath saveDir = FileSystemPath.EmptyPath;

        try
        {
            FileSystemPath configFilePath = Environment.SpecialFolder.LocalApplicationData.GetFolderPath() + "visualboyadvance-m" + "vbam.ini";

            if (configFilePath.FileExists)
                saveDir = IniNative.GetString(configFilePath, "General", "BatteryDir", String.Empty);
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Reading Visual Boy Advance - M config");
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
                _ => throw new InvalidOperationException("Unsupported Visual Boy Advance - M platform")
            };
        }
    }
}