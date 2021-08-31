namespace RayCarrot.RCP.Metro
{
    public class GameOptions_UtilitiesPageViewModel : GameOptions_BasePageViewModel
    {
        public GameOptions_UtilitiesPageViewModel(UtilityViewModel[] utilities) : base(new LocalizedString(() => Resources.GameOptions_Utilities), GenericIconKind.GameOptions_Utilities)
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