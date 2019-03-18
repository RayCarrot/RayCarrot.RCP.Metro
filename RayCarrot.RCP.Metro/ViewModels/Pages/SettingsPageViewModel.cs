using System.Threading.Tasks;
using System.Windows.Input;
using RayCarrot.CarrotFramework;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// View model for the settings page
    /// </summary>
    public class SettingsPageViewModel : BaseRCPViewModel
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public SettingsPageViewModel()
        {
            ContributeLocalizationCommand = new AsyncRelayCommand(ContributeLocalizationAsync);
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// The index of the current language
        /// </summary>
        public int CurrentLanguage
        {
            get => AppLanguages.AvailableLanguages.FindIndex(x => x.LanguageTag == AppLanguages.CurrentLanguage.LanguageTag);
            set => AppLanguages.CurrentLanguage = AppLanguages.AvailableLanguages[value];
        }

        #endregion

        #region Commands

        public ICommand ContributeLocalizationCommand { get; }

        #endregion

        #region Public Methods

        public async Task ContributeLocalizationAsync()
        {

        }

        #endregion
    }
}