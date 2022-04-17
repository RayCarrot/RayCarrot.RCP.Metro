using System;
using BinarySerializer.OpenSpace;

namespace RayCarrot.RCP.Metro;

[AttributeUsage(AttributeTargets.Field)]
public sealed class CPAGameModeInfoAttribute : GameModeBaseAttribute
{
    public CPAGameModeInfoAttribute(string displayName, EngineVersion engineVersion, Platform platform) : base(displayName, null)
    {
        EngineVersion = engineVersion;
        Platform = platform;
    }

    public CPAGameModeInfoAttribute(string displayName, EngineVersion engineVersion, Platform platform, Games game) : base(displayName, game)
    {
        EngineVersion = engineVersion;
        Platform = platform;
    }

    public EngineVersion EngineVersion { get; }
    public Platform Platform { get; }

    public override object GetSettingsObject() => GetSettings();
    public OpenSpaceSettings GetSettings() => new OpenSpaceSettings(EngineVersion, Platform);
}