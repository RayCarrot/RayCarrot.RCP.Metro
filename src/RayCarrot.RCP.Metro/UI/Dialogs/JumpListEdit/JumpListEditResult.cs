namespace RayCarrot.RCP.Metro;

/// <summary>
/// Result for the jump list editor
/// </summary>
public class JumpListEditResult : UserInputResult
{
    /// <summary>
    /// Default constructor
    /// </summary>
    /// <param name="includedItems">The included items</param>
    public JumpListEditResult(JumpListItemViewModel[] includedItems)
    {
        IncludedItems = includedItems;
    }

    /// <summary>
    /// The included items
    /// </summary>
    public JumpListItemViewModel[] IncludedItems { get; }
}