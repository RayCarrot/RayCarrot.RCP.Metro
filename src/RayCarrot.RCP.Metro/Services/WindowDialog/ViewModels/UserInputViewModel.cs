using RayCarrot.UI;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// The base view model to use for a view model used to get user input
/// </summary>
public class UserInputViewModel : BaseViewModel
{
    /// <summary>
    /// The title to use for the user input action
    /// </summary>
    public string Title { get; set; }
}