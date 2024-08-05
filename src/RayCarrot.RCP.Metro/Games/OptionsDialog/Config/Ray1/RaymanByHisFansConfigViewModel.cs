using BinarySerializer.Ray1;

namespace RayCarrot.RCP.Metro.Games.OptionsDialog;

public class RaymanByHisFansConfigViewModel : BaseRay1ConfigViewModel
{
    public RaymanByHisFansConfigViewModel(MsDosGameDescriptor gameDescriptor, GameInstallation gameInstallation) 
        : base(gameDescriptor, gameInstallation, Ray1EngineVersion.PC_Fan) { }

    public override string GetConfigFileName() => "RAYFAN.CFG";
}