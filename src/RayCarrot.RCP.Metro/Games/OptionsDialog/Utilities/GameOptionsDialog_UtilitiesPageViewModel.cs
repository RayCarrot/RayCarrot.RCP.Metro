namespace RayCarrot.RCP.Metro
{
    public class GameOptionsDialog_UtilitiesPageViewModel : GameOptionsDialog_BasePageViewModel
    {
        public GameOptionsDialog_UtilitiesPageViewModel(UtilityViewModel[] utilities) : base(new ResourceLocString(nameof(Resources.GameOptions_Utilities)), GenericIconKind.GameOptions_Utilities)
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

        public override void Dispose()
        {
            base.Dispose();

            Utilities?.DisposeAll();
        }
    }
}