using RayCarrot.CarrotFramework.Abstractions;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using RayCarrot.RCP.Core;
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
            EditJumpListCommand = new AsyncRelayCommand(EditJumpListAsync);

            CanEditShowUnderInstalledPrograms = App.IsRunningAsAdmin;
            RCFRCPC.Localization.RefreshLanguages(Data.ShowIncompleteTranslations);
        }

        #endregion

        #region Commands

        public ICommand ContributeLocalizationCommand { get; }

        public ICommand EditJumpListCommand { get; }

        #endregion

        #region Public Properties

        /// <summary>
        /// The current culture info
        /// </summary>
        public CultureInfo CurrentCultureInfo
        {
            get => new CultureInfo(Data.CurrentCulture);
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

        /// <summary>
        /// Edits the jump list items
        /// </summary>
        /// <returns>The task</returns>
        public async Task EditJumpListAsync()
        {
            // Get the result
            var result = await RCFRCP.UI.EditJumpListAsync(new JumpListEditViewModel());

            if (result.CanceledByUser)
                return;

            // Update the jump list items collection
            Data.JumpListItemIDCollection = result.IncludedItems.Select(x => x.ID).ToList();

            // Refresh
            await App.OnRefreshRequiredAsync(new RefreshRequiredEventArgs(null, false, false, false, false, true));
        }

        #endregion
    }
}