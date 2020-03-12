using RayCarrot.Rayman.OpenSpace;
using RayCarrot.Rayman.UbiArt;
using System;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Extension methods for <see cref="OpenSpaceGameMode"/>, <see cref="UbiArtGameMode"/> and <see cref="Rayman1GameMode"/>
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
                OpenSpaceGameMode.Rayman2PCDemo1 => Games.Demo_Rayman2_1,
                OpenSpaceGameMode.Rayman2PCDemo2 => Games.Demo_Rayman2_2,
                OpenSpaceGameMode.Rayman2PC => Games.Rayman2,
                OpenSpaceGameMode.RaymanMPC => Games.RaymanM,
                OpenSpaceGameMode.RaymanArenaPC => Games.RaymanArena,
                OpenSpaceGameMode.Rayman3PC => Games.Rayman3,

                UbiArtGameMode.RaymanOriginsPC => Games.RaymanOrigins,
                UbiArtGameMode.RaymanLegendsPC => Games.RaymanLegends,

                _ => (null as Games?)
            };
        }
    }
}