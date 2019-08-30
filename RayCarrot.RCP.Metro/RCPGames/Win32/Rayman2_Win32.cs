using System.Collections.Generic;
using System.IO;
using System.Windows;

namespace RayCarrot.RCP.Metro
{
    ///// <summary>
    ///// The Rayman 2 (Win32) game
    ///// </summary>
    //public sealed class Rayman2_Win32 : RCPWin32Game
    //{
    //    #region Public Overrides

    //    /// <summary>
    //    /// The game
    //    /// </summary>
    //    public override Games Game => Games.Rayman2;

    //    /// <summary>
    //    /// The game display name
    //    /// </summary>
    //    public override string DisplayName => "Rayman 2";

    //    /// <summary>
    //    /// The game backup name
    //    /// </summary>
    //    public override string BackupName => "Rayman 2";

    //    /// <summary>
    //    /// Gets the launch name for the game
    //    /// </summary>
    //    public override string GetLaunchName => "Rayman2.exe";

    //    /// <summary>
    //    /// The config UI, if any is available
    //    /// </summary>
    //    public override FrameworkElement ConfigUI => new Rayman2Config();

    //    /// <summary>
    //    /// Gets the purchase links for the game
    //    /// </summary>
    //    public override IList<GamePurchaseLink> GetGamePurchaseLinks => new List<GamePurchaseLink>()
    //    {
    //        new GamePurchaseLink(Resources.GameDisplay_PurchaseGOG, "https://www.gog.com/game/rayman_2_the_great_escape"),
    //        new GamePurchaseLink(Resources.GameDisplay_PurchaseUplay, "https://store.ubi.com/eu/rayman-2--the-great-escape/56c4947e88a7e300458b465c.html")
    //    };

    //    /// <summary>
    //    /// Gets the file links for the game
    //    /// </summary>
    //    public override IList<GameFileLink> GetGameFileLinks => new GameFileLink[]
    //    {
    //        new GameFileLink(Resources.GameLink_Setup, GameInfo.InstallDirectory + "GXSetup.exe"),
    //        new GameFileLink(Resources.GameLink_R2nGlide, GameInfo.InstallDirectory + "nglide_config.exe"),
    //        new GameFileLink(Resources.GameLink_R2dgVoodoo, GameInfo.InstallDirectory + "dgVoodooCpl.exe")
    //    };

    //    /// <summary>
    //    /// Gets the backup directories for the game
    //    /// </summary>
    //    public override IList<BackupDir> GetBackupDirectories => new List<BackupDir>()
    //    {
    //        new BackupDir()
    //        {
    //            DirPath = GameInfo.InstallDirectory + "Data" + "SaveGame",
    //            SearchOption = SearchOption.AllDirectories,
    //            ID = "0"
    //        },
    //        new BackupDir()
    //        {
    //            DirPath = GameInfo.InstallDirectory + "Data" + "Options",
    //            SearchOption = SearchOption.AllDirectories,
    //            ID = "1"
    //        },
    //    };

    //    #endregion
    //}
}