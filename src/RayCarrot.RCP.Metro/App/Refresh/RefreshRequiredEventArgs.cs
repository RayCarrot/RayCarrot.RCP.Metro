#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// The event arguments for when a refresh is required
/// </summary>
public class RefreshRequiredEventArgs : EventArgs
{
    #region Constructor

    /// <summary>
    /// Default constructor for a single modified game
    /// </summary>
    /// <param name="modifiedGame">The game which has been modified</param>
    /// <param name="flags">The refresh flags indicating what has been modified</param>
    public RefreshRequiredEventArgs(Games modifiedGame, RefreshFlags flags)
    {
        ModifiedGames = new Games[]
        {
            modifiedGame
        };
        Flags = flags;
    }

    /// <summary>
    /// Default constructor for multiple modified games
    /// </summary>
    /// <param name="modifiedGames">The games which have been modified</param>
    /// <param name="flags">The refresh flags indicating what has been modified</param>
    public RefreshRequiredEventArgs(IEnumerable<Games> modifiedGames, RefreshFlags flags)
    {
        ModifiedGames = modifiedGames?.ToArray() ?? new Games[0];
        Flags = flags;
    }

    /// <summary>
    /// Default constructor for no modified games
    /// </summary>
    /// <param name="flags">The refresh flags indicating what has been modified</param>
    public RefreshRequiredEventArgs(RefreshFlags flags) : this(null, flags) { }

    #endregion

    #region Public Properties

    /// <summary>
    /// The games which have been modified
    /// </summary>
    public IList<Games> ModifiedGames { get; }

    /// <summary>
    /// The refresh flags indicating what has been modified
    /// </summary>
    public RefreshFlags Flags { get; }

    /// <summary>
    /// Indicates if the collection of games has been modified, such as a game having been added or removed
    /// </summary>
    public bool GameCollectionModified => Flags.HasFlag(RefreshFlags.GameCollection);

    /// <summary>
    /// Indicates if the launch info for a game has been modified, such as running as administrator and/or additional launch items
    /// </summary>
    public bool LaunchInfoModified => Flags.HasFlag(RefreshFlags.LaunchInfo);

    /// <summary>
    /// Indicates if the backups have been changed, such as the location having been changed and/or existing backups having been modified
    /// </summary>
    public bool BackupsModified => Flags.HasFlag(RefreshFlags.Backups);

    /// <summary>
    /// Indicates if the game data for a game has been modified, such as the game install directory
    /// </summary>
    public bool GameInfoModified => Flags.HasFlag(RefreshFlags.GameInfo);

    /// <summary>
    /// Indicates if the jump list ID collection has been modified
    /// </summary>
    public bool JumpListModified => Flags.HasFlag(RefreshFlags.JumpList);

    #endregion
}