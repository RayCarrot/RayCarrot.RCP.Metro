#nullable disable
namespace RayCarrot.RCP.Metro;

/// <summary>
/// Base view model for cheat code items
/// </summary>
public abstract class Page_Help_BaseCheatCodeItemViewModel : BaseRCPViewModel
{
    /// <summary>
    /// Default constructor
    /// </summary>
    /// <param name="cheatDescription">The cheat code display description</param>
    /// <param name="inputLocation">The location in which to input the cheat code</param>
    protected Page_Help_BaseCheatCodeItemViewModel(string cheatDescription, string inputLocation)
    {
        CheatDescription = cheatDescription;
        InputLocation = inputLocation;
    }

    /// <summary>
    /// The cheat code display description
    /// </summary>
    public string CheatDescription { get; }

    /// <summary>
    /// The location in which to input the cheat code
    /// </summary>
    public string InputLocation { get; }
}