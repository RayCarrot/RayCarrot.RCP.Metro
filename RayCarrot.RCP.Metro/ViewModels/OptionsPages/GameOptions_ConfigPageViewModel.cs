namespace RayCarrot.RCP.Metro
{
    public abstract class GameOptions_ConfigPageViewModel : GameOptions_BasePageViewModel
    {
        protected GameOptions_ConfigPageViewModel() : base(new LocalizedString(() => Resources.GameOptions_Config), GenericIconKind.GameOptions_Config) { }

        /// <summary>
        /// Indicates if the page can be saved
        /// </summary>
        public override bool CanSave => true;
    }
}