using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using RayCarrot.CarrotFramework.Abstractions;
using RayCarrot.IO;
using RayCarrot.UI;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// The Rayman 1 (DOSBox) game manager
    /// </summary>
    public sealed class Rayman1_DOSBox : RCPDOSBoxGame
    {
        #region Public Override Properties

        /// <summary>
        /// The game
        /// </summary>
        public override Games Game => Games.Rayman1;

        /// <summary>
        /// The executable name for the game. This is independent of the <see cref="RCPGameInfo.DefaultFileName"/> which is used to launch the game.
        /// </summary>
        public override string ExecutableName => "RAYMAN.EXE";

        /// <summary>
        /// The Rayman Forever folder name, if available
        /// </summary>
        public override string RaymanForeverFolderName => "Rayman";

        /// <summary>
        /// Gets the purchase links for the game
        /// </summary>
        public override IList<GamePurchaseLink> GetGamePurchaseLinks => new GamePurchaseLink[]
        {
            new GamePurchaseLink(Resources.GameDisplay_PurchaseGOG, "https://www.gog.com/game/rayman_forever"),
            new GamePurchaseLink(Resources.GameDisplay_PurchaseUplay, "https://store.ubi.com/eu/rayman--forever/5800d3fc4e016524248b4567.html")
        };

        /// <summary>
        /// The DOSBox file path
        /// </summary>
        public override FileSystemPath DOSBoxFilePath => RCFRCP.Data.TPLSData?.IsEnabled != true ? base.DOSBoxFilePath : RCFRCP.Data.TPLSData.DOSBoxFilePath;

        /// <summary>
        /// Optional additional config files
        /// </summary>
        public override IEnumerable<FileSystemPath> AdditionalConfigFiles => RCFRCP.Data.TPLSData?.IsEnabled != true ? base.AdditionalConfigFiles : new FileSystemPath[]
        {
            RCFRCP.Data.TPLSData.ConfigFilePath
        };
        #endregion
    }
}