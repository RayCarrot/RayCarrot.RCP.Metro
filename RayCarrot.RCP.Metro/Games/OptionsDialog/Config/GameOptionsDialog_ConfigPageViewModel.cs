namespace RayCarrot.RCP.Metro
{
    public abstract class GameOptionsDialog_ConfigPageViewModel : GameOptionsDialog_BasePageViewModel
    {
        protected GameOptionsDialog_ConfigPageViewModel() : base(new ResourceLocString(nameof(Resources.GameOptions_Config)), GenericIconKind.GameOptions_Config) { }

        /// <summary>
        /// Indicates if the page can be saved
        /// </summary>
        public override bool CanSave => true;
    }
}