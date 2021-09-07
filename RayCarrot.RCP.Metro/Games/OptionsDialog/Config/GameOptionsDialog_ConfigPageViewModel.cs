using System;

namespace RayCarrot.RCP.Metro
{
    public abstract class GameOptionsDialog_ConfigPageViewModel : GameOptionsDialog_BasePageViewModel
    {
        protected GameOptionsDialog_ConfigPageViewModel() : base(new ResourceLocString(nameof(Resources.GameOptions_Config)), GenericIconKind.GameOptions_Config)
        {
            Resolution = new ResolutionSelectionViewModel();
            Resolution.ResolutionChanged += Resolution_ResolutionChanged;
        }

        /// <summary>
        /// Indicates if the page can be saved
        /// </summary>
        public override bool CanSave => true;

        /// <summary>
        /// The game screen resolution
        /// </summary>
        public ResolutionSelectionViewModel Resolution { get; }

        private void Resolution_ResolutionChanged(object sender, EventArgs e)
        {
            UnsavedChanges = true;
        }

        public override void Dispose()
        {
            base.Dispose();

            Resolution.ResolutionChanged -= Resolution_ResolutionChanged;
        }
    }
}