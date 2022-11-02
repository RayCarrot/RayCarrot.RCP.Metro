using System;

namespace RayCarrot.RCP.Metro;

// TODO-14: How do we make this system work with the new game descriptor id system? We could make the id an enum, but that would
//          easily lead to a lot of the same issues we had before. Another option is to have the game descriptors themselves
//          define their mode somehow. Yet another options is "params Type[] descriptorTypes", but that also sucks.

[AttributeUsage(AttributeTargets.Field)]
public abstract class GameModeBaseAttribute : Attribute
{
    /// <summary>
    /// Default constructor
    /// </summary>
    /// <param name="displayName">The game mode display name</param>
    /// <param name="game">The associated game</param>
    protected GameModeBaseAttribute(string displayName, Games? game)
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

    public abstract object? GetSettingsObject();
}