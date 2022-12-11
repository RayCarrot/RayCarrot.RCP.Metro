using BinarySerializer.Ray1;

namespace RayCarrot.RCP.Metro.Games.OptionsDialog;

public class Rayman1ConfigViewModel : Ray1BaseConfigViewModel
{
    public Rayman1ConfigViewModel(MsDosGameDescriptor gameDescriptor, GameInstallation gameInstallation) : 
        base(gameDescriptor, gameInstallation, Ray1EngineVersion.PC, LanguageMode.Config) { }

    public override string GetConfigFileName() => "RAYMAN.CFG";
}