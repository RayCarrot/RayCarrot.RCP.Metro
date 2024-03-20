namespace RayCarrot.RCP.Metro.Games.Structure;

public class ProgramLayout
{
    public ProgramLayout(string variantId)
    {
        VariantId = variantId;
    }

    /// <summary>
    /// The RCP-specific id for this layout. This is used to differentiate
    /// between different rom layouts of the same game. For example for
    /// regional releases and other variants.
    /// </summary>
    public string VariantId { get; }

    // TODO: Add common data like offset table etc. (can be used for runtime modifications)
}