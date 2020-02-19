using RayCarrot.Rayman.OpenSpace;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// The sync texture info utility for Rayman 2
    /// </summary>
    public class R2GameSyncTextureInfoUtility : BaseGameSyncTextureInfoUtility
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public R2GameSyncTextureInfoUtility() : base(new BaseGameSyncTextureInfoUtilityViewModel(Games.Rayman2, OpenSpaceGameMode.Rayman2PC, new string[]
        {
            "Data"
        }))
        { }
    }
}