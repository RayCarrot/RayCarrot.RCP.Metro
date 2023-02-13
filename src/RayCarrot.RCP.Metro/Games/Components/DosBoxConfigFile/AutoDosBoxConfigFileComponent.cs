namespace RayCarrot.RCP.Metro.Games.Components;

[BaseGameComponent]
public class AutoDosBoxConfigFileComponent : DosBoxConfigFileComponent
{
    public AutoDosBoxConfigFileComponent() : base(GetGameConfigFile) { }

    private static FileSystemPath GetGameConfigFile(GameInstallation gameInstallation) =>
        AppFilePaths.UserDataBaseDir + "Clients" + "DOSBox" + (gameInstallation.InstallationId + ".ini");
}