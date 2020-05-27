using RayCarrot.Rayman;

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
        public RAGameSyncTextureInfoUtility() : base(new BaseGameSyncTextureInfoUtilityViewModel(Games.RaymanArena, GameMode.RaymanArenaPC, new string[]
        {
            "MenuBin",
            "TribeBin",
            "FishBin",
        }))
        { }
    }
}