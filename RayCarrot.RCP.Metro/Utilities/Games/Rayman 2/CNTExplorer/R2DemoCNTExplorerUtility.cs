using RayCarrot.Rayman;
using RayCarrot.Rayman.OpenSpace;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// The Rayman 2 demo 1 CNT explorer utility
    /// </summary>
    public class R2Demo1CNTExplorerUtility : R2BaseCNTExplorerUtility
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public R2Demo1CNTExplorerUtility() : base(Games.Demo_Rayman2_1, OpenSpaceGameMode.Rayman2PCDemo1)
        { }
    }

    /// <summary>
    /// The Rayman 2 demo 2 CNT explorer utility
    /// </summary>
    public class R2Demo2CNTExplorerUtility : R2BaseCNTExplorerUtility
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public R2Demo2CNTExplorerUtility() : base(Games.Demo_Rayman2_2, OpenSpaceGameMode.Rayman2PCDemo2)
        { }
    }
}