using BinarySerializer.UbiArt;

namespace RayCarrot.RCP.Metro.Games.Components;

public class UbiArtGameModeComponent : BinaryGameModeComponent
{
    public UbiArtGameModeComponent(UbiArtGameMode gameMode) : base(gameMode) { }

    public UbiArtSettings GetSettings() => GetRequiredSettings<UbiArtSettings>();
}