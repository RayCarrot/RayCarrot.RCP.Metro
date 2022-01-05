using System;

namespace RayCarrot.RCP.Metro;

[AttributeUsage(AttributeTargets.Field)]
public class GameModeBaseAttribute : Attribute
{
    /// <summary>
    /// Default constructor
    /// </summary>
    /// <param name="displayName">The game mode display name</param>
    /// <param name="game">The associated game</param>
    public GameModeBaseAttribute(string displayName, Games? game)
    {
        DisplayName = displayName;
        Game = game;
    }

    /// <summary>
    /// The game mode display name
    /// </summary>
    public string DisplayName { get; }

    /// <summary>
    /// The associated game
    /// </summary>
    public Games? Game { get; }
}