using RayCarrot.IO;
using RayCarrot.Rayman.Ray1;

namespace RayCarrot.RCP.Metro
{
    public class RaymanEduDosConfigViewModel : Ray_1_KIT_EDU_BaseConfigViewModel
    {
        public RaymanEduDosConfigViewModel(Games game) : base(game, Ray1Game.RayEdu, LanguageMode.None) { }

        public override FileSystemPath GetConfigPath() => FileSystemPath.EmptyPath; // TODO-UPDATE: Implement. The config name is $"{Primary}{Secondary}.CFG", for example "QUISG1.CFG" or "EDUUSA.CFG"
    }
}