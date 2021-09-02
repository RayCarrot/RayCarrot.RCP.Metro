namespace RayCarrot.RCP.Metro
{
    public class GameOptionsDialog_UtilitiesPageViewModel : GameOptionsDialog_BasePageViewModel
    {
        public GameOptionsDialog_UtilitiesPageViewModel(UtilityViewModel[] utilities) : base(new LocalizedString(() => Resources.GameOptions_Utilities), GenericIconKind.GameOptions_Utilities)
        {
            Utilities = utilities;
        }

        /// <summary>
        /// The utilities for the game
        /// </summary>
        public UtilityViewModel[] Utilities { get; }

        protected override object GetPageUI() => new UtilitiesContainer()
        {
            Utilities = Utilities
        };
    }
}