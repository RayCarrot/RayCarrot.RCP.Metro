using RayCarrot.CarrotFramework.Abstractions;
using System;
using System.Diagnostics;
using System.Windows.Input;
using RayCarrot.UI;

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

            CanEditShowUnderInstalledPrograms = App.IsRunningAsAdmin;
        }

        #endregion

        #region Commands

        public ICommand ContributeLocalizationCommand { get; }

        #endregion

        #region Public Properties

        /// <summary>
        /// Indicates if the property <see cref="AppUserData.ShowUnderInstalledPrograms"/> can be edited
        /// </summary>
        public bool CanEditShowUnderInstalledPrograms { get; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Opens the URL for contributing to localizing the program
        /// </summary>
        public void ContributeLocalization()
        {
            try
            {
                Process.Start("https://steamcommunity.com/groups/RaymanControlPanel/discussions/0/1812044473314212117/")?.Dispose();
            }
            catch (Exception ex)
            {
                ex.HandleError($"Opening localization contribute url");
            }
        }

        #endregion
    }
}