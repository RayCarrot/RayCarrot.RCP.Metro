using System.Collections.Generic;
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
        /// The current language value pair
        /// </summary>
        public KeyValuePair<string, string> CurrentLanguage
        {
            get => AppLanguages.Languages.FindItem(x => x.Key == RCFRCP.Data.CurrentCulture);
            set => RCFRCP.Data.CurrentCulture = value.Key;
        }

        #endregion

        #region Commands

        public ICommand ContributeLocalizationCommand { get; }

        #endregion

        #region Public Methods

        public async Task ContributeLocalizationAsync()
        {
            // TODO: Implement
        }

        #endregion
    }
}