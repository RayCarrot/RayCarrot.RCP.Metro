using RayCarrot.CarrotFramework;
using Newtonsoft.Json;

namespace RayCarrot.RCP.Metro.Legacy
{
    /// <summary>
    /// Contains information for a legacy Rayman game
    /// </summary>
    public class LegacyRaymanGame
    {
        #region Constructors

        /// <summary>
        /// Default Json serializer constructor
        /// </summary>
        /// <param name="game">The game</param>
        /// <param name="type">The type of the game</param>
        [JsonConstructor]
        public LegacyRaymanGame(LegacyGames game, LegacyGameType type)
        {
            Game = game;
            Type = type;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// The game
        /// </summary>
        [JsonProperty]
        public LegacyGames Game { get; }

        /// <summary>
        /// The type of game
        /// </summary>
        [JsonProperty]
        public LegacyGameType Type { get; set; }

        /// <summary>
        /// The game directory
        /// </summary>
        [JsonProperty]
        public FileSystemPath Dir { get; set; }

        /// <summary>
        /// The mount directory
        /// </summary>
        [JsonProperty]
        public FileSystemPath MountDir { get; set; }

        /// <summary>
        /// The DosBox configuration
        /// </summary>
        [JsonProperty]
        public string[] DosBoxConfig { get; set; }

        #endregion
    }
}