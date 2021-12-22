using BinarySerializer.Ray1;

namespace RayCarrot.RCP.Metro;

public class Config_RaymanDesigner_ViewModel : Config_Ray1_BaseViewModel
{
    public Config_RaymanDesigner_ViewModel(Games game) : base(game, Ray1EngineVersion.PC_Kit, LanguageMode.Argument) { }

    public override string GetConfigFileName() => "RAYKIT.CFG";
}