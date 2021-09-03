using Nito.AsyncEx;
using RayCarrot.IO;
using RayCarrot.Logging;
using System;
using System.Threading.Tasks;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// View model for the Rayman 1 Minigames options
    /// </summary>
    public class GameOptions_Ray1Minigames_ViewModel : BaseRCPViewModel
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public GameOptions_Ray1Minigames_ViewModel()
        {
            // Create properties
            AsyncLock = new AsyncLock();

            // Get the .mms file path
            InstallDir = Games.Ray1Minigames.GetInstallDir();

            _selectedLanguage = GetCurrentLanguage() ?? Ray1MinigamesLanguage.French;

            RL.Logger?.LogInformationSource($"The current Rayman 1 Minigames language has been detected as {SelectedLanguage}");
        }

        #endregion

        #region Private Fields

        private Ray1MinigamesLanguage _selectedLanguage;

        #endregion

        #region Private Properties

        /// <summary>
        /// The game install directory
        /// </summary>
        private FileSystemPath InstallDir { get; }

        /// <summary>
        /// The async lock for <see cref="UpdateLanguageAsync"/>
        /// </summary>
        private AsyncLock AsyncLock { get; }

        #endregion

        #region Public Properties

        /// <summary>
        /// The selected Print Studio version
        /// </summary>
        public Ray1MinigamesLanguage SelectedLanguage
        {
            get => _selectedLanguage;
            set
            {
                // Update value
                _selectedLanguage = value;

                // Update data
                Task.Run(UpdateLanguageAsync);
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Gets the current language
        /// </summary>
        /// <returns>The current language or null if none was found</returns>
        public Ray1MinigamesLanguage? GetCurrentLanguage()
        {
            try
            {
                return (InstallDir + "German.exe").FileExists ? Ray1MinigamesLanguage.French : Ray1MinigamesLanguage.German;
            }
            catch (Exception ex)
            {
                ex.HandleError("Getting Rayman 1 Minigames language");

                return null;
            }
        }

        /// <summary>
        /// Updates the current language based on the selected one
        /// </summary>
        /// <returns>The task</returns>
        public async Task UpdateLanguageAsync()
        {
            using (await AsyncLock.LockAsync())
            {
                try
                {
                    RL.Logger?.LogInformationSource("The Rayman 1 Minigames language is being updated...");

                    // Get languages
                    var newLang = SelectedLanguage.ToString();
                    var oldLang = (SelectedLanguage == Ray1MinigamesLanguage.German ? Ray1MinigamesLanguage.French : Ray1MinigamesLanguage.German).ToString();

                    // Move exe files
                    RCPServices.File.MoveFile(InstallDir + "RayGames.exe", InstallDir + $"{oldLang}.exe", true);
                    RCPServices.File.MoveFile(InstallDir + $"{newLang}.exe", InstallDir + "RayGames.exe", true);

                    RL.Logger?.LogInformationSource($"The Rayman 1 Minigames language has been updated");
                }
                catch (Exception ex)
                {
                    ex.HandleError("Updating Rayman 1 Minigames language");
                    await Services.MessageUI.DisplayExceptionMessageAsync(ex, Resources.Ray1MinigamesOptions_LanguageUpdateError);
                }
            }
        }

        #endregion

        #region Enums

        /// <summary>
        /// The available languages
        /// </summary>
        public enum Ray1MinigamesLanguage
        {
            German,
            French
        }

        #endregion
    }
}