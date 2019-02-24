namespace RayCarrot.RCP.Metro.Legacy
{
    /// <summary>
    /// Extension methods for <see cref="LegacyGames"/>
    /// </summary>
    public static class LegacyGamesExtensions
    {
        /// <summary>
        /// Get the current game from the specified <see cref="LegacyGames"/>
        /// </summary>
        /// <param name="game">The legacy game to get the current game for</param>
        /// <returns>The current game or null if not found</returns>
        public static Games? GetCurrent(this LegacyGames game)
        {
            switch (game)
            {
                case LegacyGames.Rayman1:
                    return Games.Rayman1;

                case LegacyGames.RaymanDesigner:
                    return Games.RaymanDesigner;

                case LegacyGames.RaymanByHisFans:
                    return Games.RaymanByHisFans;

                case LegacyGames.Rayman60Levels:
                    return Games.Rayman60Levels;

                case LegacyGames.Rayman2:
                    return Games.Rayman2;

                case LegacyGames.RaymanM:
                    return Games.RaymanM;

                case LegacyGames.RaymanArena:
                    return Games.RaymanArena;

                case LegacyGames.Rayman3:
                    return Games.Rayman3;

                case LegacyGames.RaymanRavingRabbids:
                    return Games.RaymanRavingRabbids;

                case LegacyGames.RaymanOrigins:
                    return Games.RaymanOrigins;

                case LegacyGames.RaymanLegends:
                    return Games.RaymanLegends;

                case LegacyGames.RaymanJungleRun:
                    return Games.RaymanJungleRun;

                case LegacyGames.RaymanFiestaRun:
                    return Games.RaymanFiestaRun;

                default:
                    return null;
            }
        }
    }
}