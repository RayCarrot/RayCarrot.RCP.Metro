#nullable disable
using RayCarrot.IO;
using RayCarrot.Rayman.Ray1;

namespace RayCarrot.RCP.Metro;

public class Config_RaymanDesigner_ViewModel : Config_Ray1_BaseViewModel
{
    public Config_RaymanDesigner_ViewModel(Games game) : base(game, Ray1Game.RayKit, LanguageMode.Argument) { }

    public override FileSystemPath GetConfigPath() => Game.GetInstallDir() + $"RAYKIT.CFG";
}