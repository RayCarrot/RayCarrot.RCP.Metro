namespace RayCarrot.RCP.Metro.Legacy
{
    /// <summary>
    /// Extension methods for <see cref="LegacyGameType"/>
    /// </summary>
    public static class LegacyGameTypeExtensions
    {
        /// <summary>
        /// Get the current game type from the specified <see cref="LegacyGameType"/>
        /// </summary>
        /// <param name="game">The legacy game type to get the current game type for</param>
        /// <returns>The current game type or null if not found</returns>
        public static GameType? GetCurrent(this LegacyGameType game)
        {
            switch (game)
            {
                case LegacyGameType.Win32:
                    return GameType.Win32;

                case LegacyGameType.Steam:
                    return GameType.Steam;

                case LegacyGameType.WinStore:
                    return GameType.WinStore;

                case LegacyGameType.DosBox:
                    return GameType.DosBox;

                default:
                    return null;
            }
        }
    }
}