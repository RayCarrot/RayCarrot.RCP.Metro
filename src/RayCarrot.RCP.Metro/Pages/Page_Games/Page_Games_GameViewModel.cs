using System;
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
    /// <param name="isDemo">Indicates if the game is a demo</param>
    /// <param name="mainAction">The main action</param>
    /// <param name="secondaryAction">The secondary action</param>
    /// <param name="launchActions">The launch actions</param>
    public Page_Games_GameViewModel(Games game, string displayName, string iconSource, bool isDemo, ActionItemViewModel mainAction, ActionItemViewModel? secondaryAction, IEnumerable<OverflowButtonItemViewModel>? launchActions)
    {
        Game = game;
        DisplayName = displayName;
        IconSource = iconSource;
        IsDemo = isDemo;
        MainAction = mainAction;
        SecondaryAction = secondaryAction;
        LaunchActions = launchActions ?? Array.Empty<OverflowButtonItemViewModel>();
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
    public ActionItemViewModel? SecondaryAction { get; }

    /// <summary>
    /// The display name
    /// </summary>
    public string DisplayName { get; }

    /// <summary>
    /// The icons source
    /// </summary>
    public string IconSource { get; }

    /// <summary>
    /// Indicates if the game is a demo
    /// </summary>
    public bool IsDemo { get; }

    /// <summary>
    /// The launch actions
    /// </summary>
    public IEnumerable<OverflowButtonItemViewModel> LaunchActions { get; }
}