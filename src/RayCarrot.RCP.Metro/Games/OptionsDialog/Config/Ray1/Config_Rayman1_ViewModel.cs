using RayCarrot.IO;
using RayCarrot.Rayman.Ray1;

namespace RayCarrot.RCP.Metro;

public class Config_Rayman1_ViewModel : Config_Ray1_BaseViewModel
{
    public Config_Rayman1_ViewModel(Games game) : base(game, Ray1Game.Rayman1, LanguageMode.Config) { }

    public override FileSystemPath GetConfigPath() => Game.GetInstallDir() + $"RAYMAN.CFG";
}