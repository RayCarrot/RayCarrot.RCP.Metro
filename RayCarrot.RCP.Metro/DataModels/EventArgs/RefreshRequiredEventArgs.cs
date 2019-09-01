using System;
using System.Collections.Generic;
using System.Linq;

namespace RayCarrot.RCP.Metro
{
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
        /// <param name="gameCollectionModified">Indicates if the collection of games has been modified, such as a game having been added or removed</param>
        /// <param name="launchInfoModified">Indicates if the launch info for a game has been modified, such as running as administrator and/or additional launch items</param>
        /// <param name="backupsModified">Indicates if the backups have been changed, such as the location having been changed and/or existing backups having been modified</param>
        /// <param name="gameInfoModified">Indicates if the game info for a game has been modified, such as the game install directory</param>
        /// <param name="jumpListModified">Indicates if the jump list ID collection has been modified</param>
        public RefreshRequiredEventArgs(Games modifiedGame, bool gameCollectionModified, bool launchInfoModified, bool backupsModified, bool gameInfoModified, bool jumpListModified = false)
        {
            ModifiedGames = new Games[]
            {
                modifiedGame
            };
            GameCollectionModified = gameCollectionModified;
            LaunchInfoModified = launchInfoModified;
            BackupsModified = backupsModified;
            GameInfoModified = gameInfoModified;
            JumpListModified = jumpListModified;
        }

        /// <summary>
        /// Default constructor for multiple modified games
        /// </summary>
        /// <param name="modifiedGames">The games which have been modified</param>
        /// <param name="gameCollectionModified">Indicates if the collection of games has been modified, such as a game having been added or removed</param>
        /// <param name="launchInfoModified">Indicates if the launch info for a game has been modified, such as running as administrator and/or additional launch items</param>
        /// <param name="backupsModified">Indicates if the backups have been changed, such as the location having been changed and/or existing backups having been modified</param>
        /// <param name="gameInfoModified">Indicates if the game info for a game has been modified, such as the game install directory</param>
        /// <param name="jumpListModified">Indicates if the jump list ID collection has been modified</param>
        public RefreshRequiredEventArgs(IEnumerable<Games> modifiedGames, bool gameCollectionModified, bool launchInfoModified, bool backupsModified, bool gameInfoModified, bool jumpListModified = false)
        {
            ModifiedGames = modifiedGames?.ToArray() ?? new Games[0];
            GameCollectionModified = gameCollectionModified;
            LaunchInfoModified = launchInfoModified;
            BackupsModified = backupsModified;
            GameInfoModified = gameInfoModified;
            JumpListModified = jumpListModified;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// The games which have been modified
        /// </summary>
        public Games[] ModifiedGames { get; }

        /// <summary>
        /// Indicates if the collection of games has been modified, such as a game having been added or removed
        /// </summary>
        public bool GameCollectionModified { get; }

        /// <summary>
        /// Indicates if the launch info for a game has been modified, such as running as administrator and/or additional launch items
        /// </summary>
        public bool LaunchInfoModified { get; }

        /// <summary>
        /// Indicates if the backups have been changed, such as the location having been changed and/or existing backups having been modified
        /// </summary>
        public bool BackupsModified { get; }

        /// <summary>
        /// Indicates if the game info for a game has been modified, such as the game install directory
        /// </summary>
        public bool GameInfoModified { get; }
        
        /// <summary>
        /// Indicates if the jump list ID collection has been modified
        /// </summary>
        public bool JumpListModified { get; }

        #endregion
    }
}