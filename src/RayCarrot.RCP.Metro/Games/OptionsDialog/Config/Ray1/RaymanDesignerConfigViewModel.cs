using BinarySerializer.Ray1;

namespace RayCarrot.RCP.Metro.Games.OptionsDialog;

public class RaymanDesignerConfigViewModel : Ray1BaseConfigViewModel
{
    public RaymanDesignerConfigViewModel(MsDosGameDescriptor gameDescriptor, GameInstallation gameInstallation) : 
        base(gameDescriptor, gameInstallation, Ray1EngineVersion.PC_Kit, LanguageMode.Argument) { }

    public override string GetConfigFileName() => "RAYKIT.CFG";
}