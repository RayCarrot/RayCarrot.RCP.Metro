namespace RayCarrot.RCP.Metro;

[AttributeUsage(AttributeTargets.Field)]
public abstract class GameModeBaseAttribute : Attribute
{
    /// <summary>
    /// Default constructor
    /// </summary>
    /// <param name="displayName">The game mode display name</param>
    protected GameModeBaseAttribute(string displayName)
    {
        DisplayName = displayName;
    }

    /// <summary>
    /// The game mode display name
    /// </summary>
    public string DisplayName { get; }

    public abstract object? GetSettingsObject();
}