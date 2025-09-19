#nullable disable
namespace RayCarrot.RCP.Metro;

/// <summary>
/// The base model to use for a view model used as
/// a result when getting user input
/// </summary>
public class UserInputResult
{
    /// <summary>
    /// True if the operation was canceled
    /// </summary>
    public bool CanceledByUser { get; set; } = true;
}