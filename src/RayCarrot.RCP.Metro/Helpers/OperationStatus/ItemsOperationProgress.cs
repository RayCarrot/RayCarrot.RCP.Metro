#nullable disable
namespace RayCarrot.RCP.Metro;

/// <summary>
/// Contains information for an item during an ongoing operation
/// </summary>
public class ItemsOperationProgress
{
    /// <summary>
    /// The total progress of the operation
    /// </summary>
    public Progress TotalProgress { get; set; }

    /// <summary>
    /// The item progress for the currently processed item in the operation
    /// </summary>
    public Progress ItemProgress { get; set; }

    /// <summary>
    /// The current item being processed in the operation
    /// </summary>
    public object CurrentItem { get; set; }

    /// <summary>
    /// The name of the current operation, 
    /// usually the name of the method
    /// </summary>
    public string OperationName { get; set; }
}