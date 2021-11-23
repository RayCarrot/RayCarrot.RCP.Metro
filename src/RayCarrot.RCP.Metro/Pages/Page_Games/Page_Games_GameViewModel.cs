using System.Collections.Generic;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// View model for a game display
/// </summary>
public class Page_Games_GameViewModel : BaseRCPViewModel
{
    /// <summary>
    /// Default constructor
    /// </summary>
    /// <param name="game">The game</param>
    /// <param name="displayName">The display name</param>
    /// <param name="iconSource">The icon source</param>
    /// <param name="mainAction">The main action</param>
    /// <param name="secondaryAction">The secondary action</param>
    /// <param name="launchActions">The launch actions</param>
    public Page_Games_GameViewModel(Games game, string displayName, string iconSource, ActionItemViewModel mainAction, ActionItemViewModel secondaryAction, IEnumerable<OverflowButtonItemViewModel> launchActions)
    {
        Game = game;
        DisplayName = displayName;
        IconSource = iconSource;
        MainAction = mainAction;
        SecondaryAction = secondaryAction;
        LaunchActions = launchActions ?? new OverflowButtonItemViewModel[0];
    }

    /// <summary>
    /// The game
    /// </summary>
    public Games Game { get; }

    /// <summary>
    /// The main action
    /// </summary>
    public ActionItemViewModel MainAction { get; }

    /// <summary>
    /// The secondary action
    /// </summary>
    public ActionItemViewModel SecondaryAction { get; }

    /// <summary>
    /// The display name
    /// </summary>
    public string DisplayName { get; }

    /// <summary>
    /// The icons source
    /// </summary>
    public string IconSource { get; }

    /// <summary>
    /// The launch actions
    /// </summary>
    public IEnumerable<OverflowButtonItemViewModel> LaunchActions { get; }
}