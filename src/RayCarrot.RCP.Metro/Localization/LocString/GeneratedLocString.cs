using System;

namespace RayCarrot.RCP.Metro;

public class GeneratedLocString : LocalizedString
{
    /// <summary>
    /// Default constructor
    /// </summary>
    /// <param name="generator">The value generator</param>
    public GeneratedLocString(Func<string> generator)
    {
        // Set properties
        Generator = generator ?? throw new ArgumentNullException(nameof(generator));

        // Refresh the value
        RefreshValue();
    }

    /// <summary>
    /// The value generator
    /// </summary>
    private Func<string> Generator { get; }

    protected override string GetValue()
    {
        return Generator();
    }
}