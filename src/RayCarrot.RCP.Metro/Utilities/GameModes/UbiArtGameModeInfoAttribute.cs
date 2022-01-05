using System;
using BinarySerializer.UbiArt;

namespace RayCarrot.RCP.Metro;

[AttributeUsage(AttributeTargets.Field)]
public sealed class UbiArtGameModeInfoAttribute : GameModeBaseAttribute
{
    public UbiArtGameModeInfoAttribute(string displayName, Game ubiArtGame, Platform platform) : base(displayName, null)
    {
        UbiArtGame = ubiArtGame;
        Platform = platform;
    }

    public UbiArtGameModeInfoAttribute(string displayName, Game ubiArtGame, Platform platform, Games game) : base(displayName, game)
    {
        UbiArtGame = ubiArtGame;
        Platform = platform;
    }

    public Game UbiArtGame { get; }
    public Platform Platform { get; }

    public UbiArtSettings GetSettings() => new UbiArtSettings(UbiArtGame, Platform);
}