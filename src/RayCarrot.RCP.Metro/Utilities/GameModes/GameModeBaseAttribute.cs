using System;
using System.Linq;

namespace RayCarrot.RCP.Metro;

[AttributeUsage(AttributeTargets.Field)]
public abstract class GameModeBaseAttribute : Attribute
{
    /// <summary>
    /// Default constructor
    /// </summary>
    /// <param name="displayName">The game mode display name</param>
    /// <param name="descriptorTypes">The types of the associated game descriptors</param>
    protected GameModeBaseAttribute(string displayName, params Type[] descriptorTypes)
    {
        DisplayName = displayName;
        DescriptorTypes = descriptorTypes;

        if (descriptorTypes.Any(x => !x.IsSubclassOf(typeof(GameDescriptor))))
            throw new ArgumentException($"The provided types have to derive from {nameof(GameDescriptor)}", nameof(descriptorTypes));
    }

    /// <summary>
    /// The game mode display name
    /// </summary>
    public string DisplayName { get; }

    /// <summary>
    /// The types of the associated game descriptors
    /// </summary>
    public Type[] DescriptorTypes { get; }

    public abstract object? GetSettingsObject();

    public GameInstallation? FindGameInstallation(GamesManager gamesManager) =>
        gamesManager.FindGameInstallation(x => DescriptorTypes.Contains(x.GetType()));
}