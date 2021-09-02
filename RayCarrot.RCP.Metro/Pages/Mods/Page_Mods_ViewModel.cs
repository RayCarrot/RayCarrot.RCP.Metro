using RayCarrot.Logging;
using RayCarrot.WPF;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// View model for the mods page
    /// </summary>
    public class Page_Mods_ViewModel : BaseRCPViewModel
    { 
        /// <summary>
        /// Default constructor
        /// </summary>
        public Page_Mods_ViewModel()
        {
            Mods = new Mod_BaseViewModel[]
            {
                new Mod_RRR_ViewModel(),
            };

            App.SelectedPageChanged += App_SelectedPageChangedAsync;
        }
        
        private bool _hasInitialized;

        private async void App_SelectedPageChangedAsync(object sender, PropertyChangedEventArgs<AppPage> e)
        {
            if (e.NewValue != AppPage.Mods)
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

        public Mod_BaseViewModel[] Mods { get; }
    }
}