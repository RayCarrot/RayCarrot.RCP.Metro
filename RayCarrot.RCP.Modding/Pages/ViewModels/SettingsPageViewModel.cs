using RayCarrot.CarrotFramework.Abstractions;
using RayCarrot.RCP.Core;
using RayCarrot.UI;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Windows.Input;

namespace RayCarrot.RCP.Modding
{
    /// <summary>
    /// View model for the settings page
    /// </summary>
    public class SettingsPageViewModel : BaseViewModel
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public SettingsPageViewModel()
        {
            ContributeLocalizationCommand = new RelayCommand(ContributeLocalization);

            Data = RCFRCPM.Data;
            CanEditShowUnderInstalledPrograms = RCFRCPM.App.IsRunningAsAdmin;
            RCFRCPC.Localization.RefreshLanguages(Data.ShowIncompleteTranslations);
        }

        #endregion

        #region Commands

        public ICommand ContributeLocalizationCommand { get; }

        #endregion

        #region Public Properties

        /// <summary>
        /// The app user data
        /// </summary>
        public AppUserData Data { get; }

        /// <summary>
        /// The current culture info
        /// </summary>
        public CultureInfo CurrentCultureInfo
        {
            get => new CultureInfo(RCFRCPM.Data.CurrentCulture);
            set => Data.CurrentCulture = value?.Name ?? RCFRCPC.Localization.DefaultCulture.Name;
        }

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