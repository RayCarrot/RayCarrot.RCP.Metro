using BinarySerializer.Ray1;

namespace RayCarrot.RCP.Metro;

[AttributeUsage(AttributeTargets.Field)]
public sealed class Ray1GameModeInfoAttribute : GameModeBaseAttribute
{
    public Ray1GameModeInfoAttribute(string displayName, Ray1EngineVersion engineVersion) 
        : base(displayName)
    {
        EngineVersion = engineVersion;
    }

    public Ray1GameModeInfoAttribute(string displayName, Ray1EngineVersion engineVersion, params Type[] descriptorTypes) 
        : base(displayName, descriptorTypes)
    {
        EngineVersion = engineVersion;
    }

    public Ray1EngineVersion EngineVersion { get; }

    public override object GetSettingsObject() => GetSettings();
    public Ray1Settings GetSettings() => new(EngineVersion);
}