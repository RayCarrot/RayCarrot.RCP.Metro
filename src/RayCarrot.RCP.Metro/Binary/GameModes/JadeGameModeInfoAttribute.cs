namespace RayCarrot.RCP.Metro;

[AttributeUsage(AttributeTargets.Field)]
public sealed class JadeGameModeInfoAttribute : GameModeBaseAttribute
{
    public JadeGameModeInfoAttribute(string displayName) : base(displayName) { }

    public override object? GetSettingsObject() => null;
}