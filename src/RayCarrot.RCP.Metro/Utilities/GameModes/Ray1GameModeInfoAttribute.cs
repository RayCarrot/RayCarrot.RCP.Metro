using System;
using BinarySerializer.Ray1;

namespace RayCarrot.RCP.Metro;

[AttributeUsage(AttributeTargets.Field)]
public sealed class Ray1GameModeInfoAttribute : GameModeBaseAttribute
{
    public Ray1GameModeInfoAttribute(string displayName, Ray1EngineVersion engineVersion) : base(displayName, null)
    {
        EngineVersion = engineVersion;
    }

    public Ray1GameModeInfoAttribute(string displayName, Ray1EngineVersion engineVersion, Games game) : base(displayName, game)
    {
        EngineVersion = engineVersion;
    }

    public Ray1EngineVersion EngineVersion { get; }
}