using RayCarrot.IO;
using RayCarrot.Rayman.Ray1;

namespace RayCarrot.RCP.Metro;

public class Config_RaymanByHisFans_ViewModel : Config_Ray1_BaseViewModel
{
    public Config_RaymanByHisFans_ViewModel(Games game) : base(game, Ray1Game.RayKit, LanguageMode.Argument) { }

    public override FileSystemPath GetConfigPath() => Game.GetInstallDir() + $"RAYFAN.CFG";
}