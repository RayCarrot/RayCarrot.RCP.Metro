namespace RayCarrot.RCP.Metro;

/// <summary>
/// Contains information for an item during an ongoing operation
/// </summary>
public class ItemsOperationProgress
{
    public ItemsOperationProgress(
       Progress totalProgress,
       Progress itemProgress,
       object currentItem,
       string operationName)
    {
        TotalProgress = totalProgress;
        ItemProgress = itemProgress;
        CurrentItem = currentItem;
        OperationName = operationName;
    }

    /// <summary>
    /// The total progress of the operation
    /// </summary>
    public Progress TotalProgress { get; }

    /// <summary>
    /// The item progress for the currently processed item in the operation
    /// </summary>
    public Progress ItemProgress { get; }

    /// <summary>
    /// The current item being processed in the operation
    /// </summary>
    public object CurrentItem { get; }

    /// <summary>
    /// The name of the current operation, 
    /// usually the name of the method
    /// </summary>
    public string OperationName { get; }
}