using RayCarrot.Logging;
using RayCarrot.WPF;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// View model for the mods page
    /// </summary>
    public class ModsPageViewModel : BaseRCPViewModel
    { 
        /// <summary>
        /// Default constructor
        /// </summary>
        public ModsPageViewModel()
        {
            Mods = new BaseModViewModel[]
            {

            };

            App.SelectedPageChanged += App_SelectedPageChangedAsync;
        }
        
        private bool _hasInitialized;

        private async void App_SelectedPageChangedAsync(object sender, PropertyChangedEventArgs<Pages> e)
        {
            if (e.NewValue != Pages.Mods)
                return;

            if (_hasInitialized)
                return;

            _hasInitialized = true;

            foreach (var mod in Mods)
            {
                RL.Logger?.LogInformationSource($"Initializing mod {mod.Header.Value}");
                await mod.InitializeAsync();
            }

            RL.Logger?.LogInformationSource("Initialized mods");
        }

        public BaseModViewModel[] Mods { get; }
    }
}