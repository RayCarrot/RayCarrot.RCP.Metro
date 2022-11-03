using System;
using BinarySerializer.OpenSpace;

namespace RayCarrot.RCP.Metro;

[AttributeUsage(AttributeTargets.Field)]
public sealed class CPAGameModeInfoAttribute : GameModeBaseAttribute
{
    public CPAGameModeInfoAttribute(string displayName, EngineVersion engineVersion, Platform platform) 
        : base(displayName)
    {
        EngineVersion = engineVersion;
        Platform = platform;
    }

    public CPAGameModeInfoAttribute(string displayName, EngineVersion engineVersion, Platform platform, params Type[] descriptorTypes) 
        : base(displayName, descriptorTypes)
    {
        EngineVersion = engineVersion;
        Platform = platform;
    }

    public EngineVersion EngineVersion { get; }
    public Platform Platform { get; }

    public override object GetSettingsObject() => GetSettings();
    public OpenSpaceSettings GetSettings() => new(EngineVersion, Platform);
}