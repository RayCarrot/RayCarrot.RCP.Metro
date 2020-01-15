using RayCarrot.Rayman;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Extension methods for <see cref="OpenSpaceGameMode"/>, <see cref="UbiArtGameMode"/> and <see cref="Rayman1GameMode"/>
    /// </summary>
    public static class GameModeExtensions
    {
        /// <summary>
        /// Gets the RCP supported game from an OpenSpace game mode
        /// </summary>
        /// <param name="gameMode">The game mode to get the game from</param>
        /// <returns>The game or null if not available</returns>
        public static Games? GetGame(this OpenSpaceGameMode gameMode)
        {
            return gameMode switch
            {
                OpenSpaceGameMode.Rayman2PC => Games.Rayman2,
                OpenSpaceGameMode.RaymanMPC => Games.RaymanM,
                OpenSpaceGameMode.RaymanArenaPC => Games.RaymanArena,
                OpenSpaceGameMode.Rayman3PC => Games.Rayman3,
                _ => (null as Games?)
            };
        }

        /// <summary>
        /// Gets the RCP supported game from an UbiArt game mode
        /// </summary>
        /// <param name="gameMode">The game mode to get the game from</param>
        /// <returns>The game or null if not available</returns>
        public static Games? GetGame(this UbiArtGameMode gameMode)
        {
            return gameMode switch
            {
                UbiArtGameMode.RaymanOriginsPC => Games.RaymanOrigins,
                UbiArtGameMode.RaymanLegendsPC => Games.RaymanLegends,
                _ => (null as Games?)
            };
        }

        /// <summary>
        /// Gets the RCP supported game from a Rayman 1 game mode
        /// </summary>
        /// <param name="gameMode">The game mode to get the game from</param>
        /// <returns>The game or null if not available</returns>
        public static Games? GetGame(this Rayman1GameMode gameMode)
        {
            return gameMode switch
            {
                Rayman1GameMode.Rayman1PC => Games.Rayman1,
                Rayman1GameMode.RaymanDesignerPC => Games.RaymanDesigner,
                Rayman1GameMode.RaymanEduPC => Games.EducationalDos,
                _ => (null as Games?)
            };
        }
    }
}