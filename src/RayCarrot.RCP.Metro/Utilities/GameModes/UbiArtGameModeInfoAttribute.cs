using BinarySerializer.UbiArt;

namespace RayCarrot.RCP.Metro;

[AttributeUsage(AttributeTargets.Field)]
public sealed class UbiArtGameModeInfoAttribute : GameModeBaseAttribute
{
    public UbiArtGameModeInfoAttribute(string displayName, BinarySerializer.UbiArt.Game ubiArtGame, Platform platform) 
        : base(displayName)
    {
        UbiArtGame = ubiArtGame;
        Platform = platform;
    }

    public UbiArtGameModeInfoAttribute(string displayName, BinarySerializer.UbiArt.Game ubiArtGame, Platform platform, params Type[] descriptorTypes) 
        : base(displayName, descriptorTypes)
    {
        UbiArtGame = ubiArtGame;
        Platform = platform;
    }

    public BinarySerializer.UbiArt.Game UbiArtGame { get; }
    public Platform Platform { get; }

    public override object GetSettingsObject() => GetSettings();
    public UbiArtSettings GetSettings() => new(UbiArtGame, Platform);
}