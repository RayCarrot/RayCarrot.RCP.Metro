using System;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Extension methods for <see cref="GameType"/>
    /// </summary>
    public static class GameTypeExtensions
    {
        /// <summary>
        /// Gets the display name for the specified game type
        /// </summary>
        /// <param name="gameType">The type of game to get the display name for</param>
        /// <returns>The display name</returns>
        public static string GetDisplayName(this GameType gameType)
        {
            switch (gameType)
            {
                case GameType.Win32:
                    return Resources.GameType_Desktop;

                case GameType.Steam:
                    return Resources.GameType_Steam;

                case GameType.WinStore:
                    return Resources.GameType_WinStore;

                case GameType.DosBox:
                    return Resources.GameType_DosBox;

                default:
                    throw new ArgumentOutOfRangeException(nameof(gameType), gameType, null);
            }
        }
    }
}