using BinarySerializer.Ray1;

namespace RayCarrot.RCP.Metro.Games.Components;

public class Ray1GameModeComponent : BinaryGameModeComponent
{
    public Ray1GameModeComponent(Ray1GameMode gameMode) : base(gameMode) { }

    public Ray1Settings GetSettings() => GetRequiredSettings<Ray1Settings>();
}