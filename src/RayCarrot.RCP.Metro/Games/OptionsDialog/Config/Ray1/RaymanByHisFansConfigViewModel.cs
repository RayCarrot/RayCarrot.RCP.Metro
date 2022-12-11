using BinarySerializer.Ray1;

namespace RayCarrot.RCP.Metro.Games.OptionsDialog;

public class RaymanByHisFansConfigViewModel : Ray1BaseConfigViewModel
{
    public RaymanByHisFansConfigViewModel(MsDosGameDescriptor gameDescriptor, GameInstallation gameInstallation) : 
        base(gameDescriptor, gameInstallation, Ray1EngineVersion.PC_Fan, LanguageMode.Argument) { }

    public override string GetConfigFileName() => "RAYFAN.CFG";
}