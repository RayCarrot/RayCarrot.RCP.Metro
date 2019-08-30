using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using RayCarrot.IO;

namespace RayCarrot.RCP.Metro
{
    ///// <summary>
    ///// The base for a Rayman Control Panel game
    ///// </summary>
    //public abstract class RCPGame
    //{
    //    #region Public Abstract Properties

    //    /// <summary>
    //    /// The game
    //    /// </summary>
    //    public abstract Games Game { get; }

    //    /// <summary>
    //    /// The game type
    //    /// </summary>
    //    public abstract GameType Type { get; }

    //    /// <summary>
    //    /// The game display name
    //    /// </summary>
    //    public abstract string DisplayName { get; }

    //    /// <summary>
    //    /// The game backup name
    //    /// </summary>
    //    public abstract string BackupName { get; }

    //    /// <summary>
    //    /// The display name for the game type
    //    /// </summary>
    //    public abstract string GameTypeDisplayName { get; }

    //    /// <summary>
    //    /// Indicates if using <see cref="GameLaunchMode"/> is supported
    //    /// </summary>
    //    public abstract bool SupportsGameLaunchMode { get; }

    //    #endregion

    //    #region Public Virtual Properties

    //    /// <summary>
    //    /// The options UI, if any is available
    //    /// </summary>
    //    public virtual FrameworkElement OptionsUI => null;

    //    /// <summary>
    //    /// The config UI, if any is available
    //    /// </summary>
    //    public virtual FrameworkElement ConfigUI => null;

    //    /// <summary>
    //    /// Gets the purchase links for the game
    //    /// </summary>
    //    public virtual IList<GamePurchaseLink> GetGamePurchaseLinks => new GamePurchaseLink[0];

    //    /// <summary>
    //    /// Gets the file links for the game
    //    /// </summary>
    //    public virtual IList<GameFileLink> GetGameFileLinks => new GameFileLink[0];

    //    /// <summary>
    //    /// Gets the backup directories for the game
    //    /// </summary>
    //    public virtual IList<BackupDir> GetBackupDirectories => null;

    //    #endregion

    //    #region Public Properties

    //    /// <summary>
    //    /// Gets the game info
    //    /// </summary>
    //    public GameInfo GameInfo => Game.GetInfo();

    //    /// <summary>
    //    /// Indicates if the game has been added
    //    /// </summary>
    //    public bool IsAdded => Game.IsAdded();

    //    #endregion

    //    #region Public Abstract Methods

    //    /// <summary>
    //    /// Locates the game
    //    /// </summary>
    //    /// <returns>Null if the game was not found. Otherwise a valid or empty path for the install directory</returns>
    //    public abstract Task<FileSystemPath?> LocateAsync();

    //    #endregion

    //    // TODO: Get backup dirs
    //}
}