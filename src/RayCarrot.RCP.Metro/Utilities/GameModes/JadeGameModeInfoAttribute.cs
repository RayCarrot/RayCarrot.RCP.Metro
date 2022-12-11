namespace RayCarrot.RCP.Metro;

[AttributeUsage(AttributeTargets.Field)]
public sealed class JadeGameModeInfoAttribute : GameModeBaseAttribute
{
    public JadeGameModeInfoAttribute(string displayName)
        : base(displayName)
    {

    }

    public JadeGameModeInfoAttribute(string displayName, params Type[] descriptorTypes) 
        : base(displayName, descriptorTypes)
    {

    }

    public override object? GetSettingsObject() => null;
}