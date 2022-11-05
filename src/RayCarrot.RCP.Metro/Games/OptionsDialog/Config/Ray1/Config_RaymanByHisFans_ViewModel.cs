using BinarySerializer.Ray1;

namespace RayCarrot.RCP.Metro;

public class Config_RaymanByHisFans_ViewModel : Config_Ray1_BaseViewModel
{
    public Config_RaymanByHisFans_ViewModel(MSDOSGameDescriptor gameDescriptor, GameInstallation gameInstallation) : 
        base(gameDescriptor, gameInstallation, Ray1EngineVersion.PC_Fan, LanguageMode.Argument) { }

    public override string GetConfigFileName() => "RAYFAN.CFG";
}