using RayCarrot.IO;
using RayCarrot.Rayman.Ray1;

namespace RayCarrot.RCP.Metro
{
    public class Rayman1ConfigViewModel : Ray_1_KIT_EDU_BaseConfigViewModel
    {
        public Rayman1ConfigViewModel(Games game) : base(game, Ray1Game.Rayman1, LanguageMode.Config) { }

        public override FileSystemPath GetConfigPath() => Game.GetInstallDir() + $"RAYMAN.CFG";
    }
}