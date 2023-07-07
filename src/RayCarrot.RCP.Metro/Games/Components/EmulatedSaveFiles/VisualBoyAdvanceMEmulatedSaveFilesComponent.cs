using IniParser;
using IniParser.Model;

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
            {
                IniData configData = new FileIniDataParser().ReadFile(configFilePath);
                saveDir = configData["General"]["BatteryDir"];
            }
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
            yield return new EmulatedGbaSaveFile(saveFilePath);
    }
}