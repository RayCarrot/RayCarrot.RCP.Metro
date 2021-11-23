namespace RayCarrot.RCP.Metro;

/// <summary>
/// View model for a Rayman 1 cheat code item
/// </summary>
public class Page_Help_Rayman1CheatCodeItemViewModel : Page_Help_BaseCheatCodeItemViewModel
{
    /// <summary>
    /// Default constructor
    /// </summary>
    /// <param name="cheatDescription">The cheat code display description</param>
    /// <param name="inputLocation">The location in which to input the cheat code</param>
    /// <param name="input1">Inputs with gibberish letters</param>
    /// <param name="input2">Inputs with letters forming words</param>
    /// <param name="input3">Inputs using the tab key</param>
    public Page_Help_Rayman1CheatCodeItemViewModel(string cheatDescription, string inputLocation, string input1, string input2, string input3) : base(cheatDescription, inputLocation)
    {
        Input1 = input1;
        Input2 = input2;
        Input3 = input3;
    }

    /// <summary>
    /// Inputs with gibberish letters
    /// </summary>
    public string Input1 { get; }

    /// <summary>
    /// Inputs with letters forming words
    /// </summary>
    public string Input2 { get; }

    /// <summary>
    /// Inputs using the tab key
    /// </summary>
    public string Input3 { get; }
}