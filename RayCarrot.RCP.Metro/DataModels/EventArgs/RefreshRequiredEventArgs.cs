using System;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// The event arguments for when a refresh is required
    /// </summary>
    public class RefreshRequiredEventArgs : EventArgs
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="modifiedGame">The game which has been modified, if a single game</param>
        /// <param name="gameCollectionModified">Indicates if the collection of games has been modified, such as a game having been added or removed</param>
        /// <param name="launchInfoModified">Indicates if the launch info for a game has been modified, such as running as administrator and/or additional launch items</param>
        /// <param name="backupsModified">Indicates if the backups have been changed, such as the location having been changed and/or existing backups having been modified</param>
        /// <param name="gameInfoModified">Indicates if the game info for a game has been modified, such as the game install directory</param>
        public RefreshRequiredEventArgs(Games? modifiedGame, bool gameCollectionModified, bool launchInfoModified, bool backupsModified, bool gameInfoModified)
        {
            ModifiedGame = modifiedGame;
            GameCollectionModified = gameCollectionModified;
            LaunchInfoModified = launchInfoModified;
            BackupsModified = backupsModified;
            GameInfoModified = gameInfoModified;
        }

        /// <summary>
        /// The game which has been modified, if a single game
        /// </summary>
        public Games? ModifiedGame { get; }

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
    }
}