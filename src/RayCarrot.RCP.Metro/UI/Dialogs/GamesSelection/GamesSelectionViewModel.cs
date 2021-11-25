#nullable disable
using System.Linq;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// View model for a games selection
/// </summary>
public class GamesSelectionViewModel : UserInputViewModel
{
    /// <summary>
    /// Default constructor
    /// </summary>
    public GamesSelectionViewModel()
    {
        Title = "Select games";
        Games = Services.App.GetGames.Select(x => new GamesItem(x)).ToArray();
    }

    /// <summary>
    /// The selected games
    /// </summary>
    public GamesItem[] Games { get; }

    /// <summary>
    /// A game item
    /// </summary>
    public class GamesItem : BaseRCPViewModel
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="game">The game</param>
        public GamesItem(Games game)
        {
            Game = game;
            DisplayName = game.GetGameInfo().DisplayName;
        }

        /// <summary>
        /// Indicates if the item is selected
        /// </summary>
        public bool IsSelected { get; set; }

        /// <summary>
        /// The game
        /// </summary>
        public Games Game { get; }

        /// <summary>
        /// The game display name
        /// </summary>
        public string DisplayName { get; }
    }
}