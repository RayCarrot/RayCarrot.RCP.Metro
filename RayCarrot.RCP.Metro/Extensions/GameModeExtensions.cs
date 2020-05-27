using RayCarrot.Rayman;
using System;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Extension methods for <see cref="GameMode"/>
    /// </summary>
    public static class GameModeExtensions
    {
        /// <summary>
        /// Gets the RCP supported game from a game mode
        /// </summary>
        /// <param name="gameMode">The game mode to get the game from</param>
        /// <returns>The game or null if not available</returns>
        public static Games? GetGame(this Enum gameMode)
        {
            return gameMode switch
            {
                GameMode.Rayman1PC => Games.Rayman1,
                GameMode.RayKitPC => Games.RaymanDesigner,

                GameMode.Rayman2PCDemo1 => Games.Demo_Rayman2_1,
                GameMode.Rayman2PCDemo2 => Games.Demo_Rayman2_2,
                GameMode.Rayman2PC => Games.Rayman2,
                GameMode.RaymanMPC => Games.RaymanM,
                GameMode.RaymanArenaPC => Games.RaymanArena,
                GameMode.Rayman3PC => Games.Rayman3,

                GameMode.RaymanOriginsPC => Games.RaymanOrigins,
                GameMode.RaymanLegendsPC => Games.RaymanLegends,

                _ => (null as Games?)
            };
        }
    }
}