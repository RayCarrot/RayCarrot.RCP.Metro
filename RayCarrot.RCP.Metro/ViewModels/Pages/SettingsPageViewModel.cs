using RayCarrot.CarrotFramework;
using System.Windows.Input;

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
            ContributeLocalizationCommand = new RelayCommand(ContributeLocalization);
        }

        #endregion

        #region Commands

        public ICommand ContributeLocalizationCommand { get; }

        #endregion

        #region Public Methods

        public void ContributeLocalization()
        {
            // TODO: Open Steam discussion page?
        }

        #endregion
    }
}