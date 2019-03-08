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
                    return "Desktop";

                case GameType.Steam:
                    return "Steam";

                case GameType.WinStore:
                    return "Windows Store";

                case GameType.DosBox:
                    return "DosBox";

                default:
                    throw new ArgumentOutOfRangeException(nameof(gameType), gameType, null);
            }
        }
    }
}