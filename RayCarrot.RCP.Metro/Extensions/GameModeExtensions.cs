using System;
using RayCarrot.Rayman.OpenSpace;
using RayCarrot.Rayman.Rayman1;
using RayCarrot.Rayman.UbiArt;

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
                OpenSpaceGameMode.Rayman2PC => Games.Rayman2,
                OpenSpaceGameMode.RaymanMPC => Games.RaymanM,
                OpenSpaceGameMode.RaymanArenaPC => Games.RaymanArena,
                OpenSpaceGameMode.Rayman3PC => Games.Rayman3,

                UbiArtGameMode.RaymanOriginsPC => Games.RaymanOrigins,
                UbiArtGameMode.RaymanLegendsPC => Games.RaymanLegends,

                Rayman1GameMode.Rayman1PC => Games.Rayman1,
                Rayman1GameMode.RaymanDesignerPC => Games.RaymanDesigner,
                //Rayman1GameMode.RaymanEduPC => Games.EducationalDos,

                _ => (null as Games?)
            };
        }
    }
}