using RayCarrot.Rayman;
using RayCarrot.Rayman.OpenSpace;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// The sync texture info utility for Rayman 3
    /// </summary>
    public class R3GameSyncTextureInfoUtility : BaseGameSyncTextureInfoUtility
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public R3GameSyncTextureInfoUtility() : base(new BaseGameSyncTextureInfoUtilityViewModel(Games.Rayman3, OpenSpaceGameMode.Rayman3PC, new string[]
        {
            "Gamedatabin"
        }))
        { }
    }
}