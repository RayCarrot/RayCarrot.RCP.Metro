using BinarySerializer.Ray1;

namespace RayCarrot.RCP.Metro.Games.OptionsDialog;

public class RaymanDesignerConfigViewModel : BaseRay1ConfigViewModel
{
    public RaymanDesignerConfigViewModel(MsDosGameDescriptor gameDescriptor, GameInstallation gameInstallation) 
        : base(gameDescriptor, gameInstallation, Ray1EngineVersion.PC_Kit) { }

    public override string GetConfigFileName() => "RAYKIT.CFG";
}