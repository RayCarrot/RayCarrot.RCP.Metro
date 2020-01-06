namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// View model for a generic cheat code item
    /// </summary>
    public class GenericCheatCodeItemViewModel : BaseCheatCodeItemViewModel
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="cheatDescription">The cheat code display description</param>
        /// <param name="inputLocation">The location in which to input the cheat code</param>
        /// <param name="input">The input</param>
        public GenericCheatCodeItemViewModel(string cheatDescription, string inputLocation, string input) : base(cheatDescription, inputLocation)
        {
            Input = input;
        }

        /// <summary>
        /// The input
        /// </summary>
        public string Input { get; }
    }
}