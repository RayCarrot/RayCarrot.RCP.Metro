using RayCarrot.IO;
using RayCarrot.Rayman.Ray1;

namespace RayCarrot.RCP.Metro
{
    public class RaymanByHisFansConfigViewModel : Ray_1_KIT_EDU_BaseConfigViewModel
    {
        public RaymanByHisFansConfigViewModel(Games game) : base(game, Ray1Game.RayKit, LanguageMode.Argument) { }

        public override FileSystemPath GetConfigPath() => Game.GetInstallDir() + $"RAYFAN.CFG";
    }
}