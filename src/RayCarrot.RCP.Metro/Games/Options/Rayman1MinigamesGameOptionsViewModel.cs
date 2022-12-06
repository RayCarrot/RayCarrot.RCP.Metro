using Nito.AsyncEx;
using NLog;
using System;
using System.Threading.Tasks;

namespace RayCarrot.RCP.Metro.Games.Options;

// TODO: Rather than rename exe we could just change the exe RCP launches?

/// <summary>
/// View model for the Rayman 1 Minigames game options
/// </summary>
public class Rayman1MinigamesGameOptionsViewModel : GameOptionsViewModel
{
    #region Constructor

    public Rayman1MinigamesGameOptionsViewModel(GameInstallation gameInstallation) : base(gameInstallation)
    {
        // Create properties
        AsyncLock = new AsyncLock();

        InstallDir = gameInstallation.InstallLocation;

        _selectedLanguage = GetCurrentLanguage() ?? Ray1MinigamesLanguage.French;

        Logger.Info("The current Rayman 1 Minigames language has been detected as {0}", SelectedLanguage);
    }

    #endregion

    #region Logger

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

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
            Logger.Error(ex, "Getting Rayman 1 Minigames language");

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
                Logger.Info("The Rayman 1 Minigames language is being updated...");

                // Get languages
                var newLang = SelectedLanguage.ToString();
                var oldLang = (SelectedLanguage == Ray1MinigamesLanguage.German ? Ray1MinigamesLanguage.French : Ray1MinigamesLanguage.German).ToString();

                // Move exe files
                Services.File.MoveFile(InstallDir + "RayGames.exe", InstallDir + $"{oldLang}.exe", true);
                Services.File.MoveFile(InstallDir + $"{newLang}.exe", InstallDir + "RayGames.exe", true);

                Logger.Info("The Rayman 1 Minigames language has been updated");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Updating Rayman 1 Minigames language");
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