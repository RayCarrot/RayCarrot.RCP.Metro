using BinarySerializer.Ray1;

namespace RayCarrot.RCP.Metro;

public class Config_Rayman1_ViewModel : Config_Ray1_BaseViewModel
{
    public Config_Rayman1_ViewModel(MsDosGameDescriptor gameDescriptor, GameInstallation gameInstallation) : 
        base(gameDescriptor, gameInstallation, Ray1EngineVersion.PC, LanguageMode.Config) { }

    public override string GetConfigFileName() => "RAYMAN.CFG";
}