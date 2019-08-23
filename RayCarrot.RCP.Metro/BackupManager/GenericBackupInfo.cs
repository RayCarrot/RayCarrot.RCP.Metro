using System.Collections.Generic;
using RayCarrot.IO;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Backup information for a generic game
    /// </summary>
    public class GenericBackupInfo : BaseBackupInfo
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="game">The game to get the information from</param>
        public GenericBackupInfo(Games game) : 
            base(RCFRCP.App.GetCompressedBackupFile(game.GetBackupName()),
                RCFRCP.App.GetBackupDir(game.GetBackupName()),
                game.GetBackupInfo(),
                game.GetDisplayName())
        {

        }
    }
}