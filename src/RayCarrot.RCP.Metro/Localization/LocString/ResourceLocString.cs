#nullable disable
using System;

namespace RayCarrot.RCP.Metro;

public class ResourceLocString : LocalizedString
{
    /// <summary>
    /// Default constructor
    /// </summary>
    /// <param name="resourcekey">The resource key</param>
    /// <param name="formatArgs">Optional format arguments</param>
    public ResourceLocString(string resourcekey, params object[] formatArgs)
    {
        // Set properties
        ResourceKey = resourcekey ?? throw new ArgumentNullException(nameof(resourcekey));
        FormatArgs = formatArgs;

        // Refresh the value
        RefreshValue();
    }

    /// <summary>
    /// The resource key
    /// </summary>
    private string ResourceKey { get; }

    /// <summary>
    /// Optional format arguments if the <see cref="ResourceKey"/> is specified
    /// </summary>
    private object[] FormatArgs { get; }

    protected override string GetValue()
    {
        var v = Resources.ResourceManager.GetString(ResourceKey, Services.InstanceData.CurrentCulture);

        if (FormatArgs.Any() && v != null)
            v = String.Format(v, FormatArgs);

        return v;
    }
}