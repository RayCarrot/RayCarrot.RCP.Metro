using RayCarrot.Rayman.OpenSpace;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// The sync texture info utility for Rayman M
    /// </summary>
    public class RMGameSyncTextureInfoUtility : BaseGameSyncTextureInfoUtility
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public RMGameSyncTextureInfoUtility() : base(new BaseGameSyncTextureInfoUtilityViewModel(Games.RaymanM, OpenSpaceGameMode.RaymanMPC, new string[]
        {
            "MenuBin",
            "TribeBin",
            "FishBin",
        }))
        { }
    }
}