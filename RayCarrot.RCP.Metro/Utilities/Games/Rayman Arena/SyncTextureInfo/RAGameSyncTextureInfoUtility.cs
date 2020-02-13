using RayCarrot.Rayman;
using RayCarrot.Rayman.OpenSpace;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// The sync texture info utility for Rayman Arena
    /// </summary>
    public class RAGameSyncTextureInfoUtility : BaseGameSyncTextureInfoUtility
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public RAGameSyncTextureInfoUtility() : base(new BaseGameSyncTextureInfoUtilityViewModel(Games.RaymanArena, OpenSpaceGameMode.RaymanArenaPC, new string[]
        {
            "MenuBin",
            "TribeBin",
            "FishBin",
        }))
        { }
    }
}