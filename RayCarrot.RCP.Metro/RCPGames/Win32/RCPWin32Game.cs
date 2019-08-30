using System;
using System.Threading.Tasks;
using RayCarrot.CarrotFramework.Abstractions;
using RayCarrot.IO;
using RayCarrot.UI;

namespace RayCarrot.RCP.Metro
{
    ///// <summary>
    ///// Base for a Win32 Rayman Control Panel game
    ///// </summary>
    //public abstract class RCPWin32Game : RCPGame
    //{
    //    #region Public Override Properties

    //    /// <summary>
    //    /// The game type
    //    /// </summary>
    //    public override GameType Type => GameType.Win32;

    //    /// <summary>
    //    /// The display name for the game type
    //    /// </summary>
    //    public override string GameTypeDisplayName => Resources.GameType_Desktop;

    //    /// <summary>
    //    /// Indicates if using <see cref="GameLaunchMode"/> is supported
    //    /// </summary>
    //    public override bool SupportsGameLaunchMode => true;

    //    #endregion

    //    #region Public Abstract Properties

    //    /// <summary>
    //    /// Gets the launch name for the game
    //    /// </summary>
    //    public abstract string GetLaunchName { get; }

    //    #endregion

    //    #region Public Virtual Properties

    //    /// <summary>
    //    /// Gets the launch arguments for the game
    //    /// </summary>
    //    public virtual string GetLaunchArgs => null;

    //    #endregion

    //    #region Public Override Methods

    //    /// <summary>
    //    /// Locates the game
    //    /// </summary>
    //    /// <returns>Null if the game was not found. Otherwise a valid or empty path for the install directory</returns>
    //    public override async Task<FileSystemPath?> LocateAsync()
    //    {
    //        var result = await RCFUI.BrowseUI.BrowseDirectoryAsync(new DirectoryBrowserViewModel()
    //        {
    //            Title = Resources.LocateGame_BrowserHeader,
    //            DefaultDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
    //            MultiSelection = false
    //        });

    //        if (result.CanceledByUser)
    //            return null;

    //        if (!result.SelectedDirectory.DirectoryExists)
    //            return null;

    //        // Make sure the game is valid
    //        if (await new Win32GameManager(Game).IsValidAsync(result.SelectedDirectory))
    //            return result.SelectedDirectory;

    //        RCFCore.Logger?.LogInformationSource($"The selected install directory for {Game} is not valid");

    //        await RCFUI.MessageUI.DisplayMessageAsync(Resources.LocateGame_InvalidLocation, Resources.LocateGame_InvalidLocationHeader, MessageType.Error);

    //        return null;
    //    }

    //    #endregion

    //    #region Public Virtual Methods

    //    /// <summary>
    //    /// Gets the launch info for the game
    //    /// </summary>
    //    /// <returns>The launch info</returns>
    //    public virtual GameLaunchInfo GetLaunchInfo()
    //    {
    //        //if (Game == Games.RaymanRavingRabbids2)
    //        //    args = $"/{RCFRCP.Data.RRR2LaunchMode.ToString().ToLower()} /B Rrr2.bf";
    //        //else if (Game == Games.RabbidsGoHome)
    //        //    args = RCFRCP.Data.RabbidsGoHomeLaunchData?.ToString();

    //        return new GameLaunchInfo(GameInfo.InstallDirectory + Game.GetLaunchName(), null);
    //    }

    //    #endregion

    //    // Put game manager logic here
    //}
}