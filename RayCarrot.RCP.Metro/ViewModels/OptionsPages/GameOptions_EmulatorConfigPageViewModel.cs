namespace RayCarrot.RCP.Metro
{
    public abstract class GameOptions_EmulatorConfigPageViewModel : GameOptions_BasePageViewModel
    {
        protected GameOptions_EmulatorConfigPageViewModel(LocalizedString pageName) : base(pageName, GenericIconKind.GameOptions_Emulator) { }

        /// <summary>
        /// Indicates if the page can be saved
        /// </summary>
        public override bool CanSave => true;
    }
}