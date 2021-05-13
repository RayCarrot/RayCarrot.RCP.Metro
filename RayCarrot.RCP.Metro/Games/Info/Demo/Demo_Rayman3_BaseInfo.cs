using RayCarrot.IO;
using RayCarrot.Rayman;
using RayCarrot.Rayman.OpenSpace;
using System.Collections.Generic;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// The Rayman 3 Demo base game info
    /// </summary>
    public abstract class Demo_Rayman3_BaseInfo : RCPGameInfo
    {
        #region Public Override Properties

        /// <summary>
        /// The category for the game
        /// </summary>
        public override GameCategory Category => GameCategory.Demo;

        /// <summary>
        /// Gets the launch name for the game
        /// </summary>
        public override string DefaultFileName => "MainP5Pvf.exe";

        /// <summary>
        /// Indicates if the game can be downloaded
        /// </summary>
        public override bool CanBeDownloaded => true;

        /// <summary>
        /// The config page view model, if any is available
        /// </summary>
        public override GameOptions_ConfigPageViewModel ConfigPageViewModel => new Rayman3ConfigViewModel(Game);

        /// <summary>
        /// Gets the file links for the game
        /// </summary>
        public override IList<GameFileLink> GetGameFileLinks => new GameFileLink[]
        {
            new GameFileLink(Resources.GameLink_Setup, Game.GetInstallDir() + "R3_Setup_DX8D.exe")
        };

        /// <summary>
        /// The group names to use for the options, config and utility dialog
        /// </summary>
        public override IEnumerable<string> DialogGroupNames => new string[]
        {
            UbiIniFileGroupName
        };

        /// <summary>
        /// Indicates if the game has archives which can be opened
        /// </summary>
        public override bool HasArchives => true;

        /// <summary>
        /// Gets the archive data manager for the game
        /// </summary>
        public override IArchiveDataManager GetArchiveDataManager => new OpenSpaceCntArchiveDataManager(OpenSpaceSettings.GetDefaultSettings(OpenSpaceGame.Rayman3, Platform.PC));

        /// <summary>
        /// Gets the archive file paths for the game
        /// </summary>
        /// <param name="installDir">The game's install directory</param>
        public override FileSystemPath[] GetArchiveFilePaths(FileSystemPath installDir) => new FileSystemPath[]
        {
            //installDir + "Gamedatabin" + "tex16.cnt",
            installDir + "Gamedatabin" + "tex32.cnt",
            installDir + "Gamedatabin" + "vignette.cnt",
        };

        #endregion
    }
}