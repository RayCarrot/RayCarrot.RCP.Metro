using System.Collections.Generic;
using System.Windows;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// The Rabbids Go Home game info
    /// </summary>
    public sealed class RabbidsGoHome_Info : RCPGameInfo
    {
        #region Public Override Properties

        /// <summary>
        /// The game
        /// </summary>
        public override Games Game => Games.RabbidsGoHome;

        /// <summary>
        /// The game display name
        /// </summary>
        public override string DisplayName => "Rabbids Go Home";

        /// <summary>
        /// The game backup name
        /// </summary>
        public override string BackupName => "Rabbids Go Home";

        /// <summary>
        /// Gets the launch name for the game
        /// </summary>
        public override string DefaultFileName => RCFRCP.Data.RabbidsGoHomeLaunchData == null ? "Launcher.exe" : "LyN_f.exe";

        /// <summary>
        /// The config UI, if any is available
        /// </summary>
        public override FrameworkElement ConfigUI => new RabbidsGoHomeConfig();

        /// <summary>
        /// Gets the file links for the game
        /// </summary>
        public override IList<GameFileLink> GetGameFileLinks => new GameFileLink[0];

        #endregion
    }
}