namespace RayCarrot.RCP.Metro;

/// <summary>
/// View model for a generic cheat code item
/// </summary>
public class Page_Help_GenericCheatCodeItemViewModel : Page_Help_BaseCheatCodeItemViewModel
{
    /// <summary>
    /// Default constructor
    /// </summary>
    /// <param name="cheatDescription">The cheat code display description</param>
    /// <param name="inputLocation">The location in which to input the cheat code</param>
    /// <param name="input">The input</param>
    public Page_Help_GenericCheatCodeItemViewModel(string cheatDescription, string inputLocation, string input) : base(cheatDescription, inputLocation)
    {
        Input = input;
    }

    /// <summary>
    /// The input
    /// </summary>
    public string Input { get; }
}