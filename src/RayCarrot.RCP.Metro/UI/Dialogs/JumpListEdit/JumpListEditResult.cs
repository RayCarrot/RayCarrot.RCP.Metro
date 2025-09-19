#nullable disable
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
    /// <param name="autoSort">Indicates if the items should be automatically sorted</param>
    public JumpListEditResult(JumpListItemViewModel[] includedItems, bool autoSort)
    {
        IncludedItems = includedItems;
        AutoSort = autoSort;
    }

    /// <summary>
    /// The included items
    /// </summary>
    public JumpListItemViewModel[] IncludedItems { get; }

    /// <summary>
    /// Indicates if the items should be automatically sorted
    /// </summary>
    public bool AutoSort { get; }
}