using IniParser.Model;
using RayCarrot.IO;
using NLog;
using RayCarrot.Rayman.UbiIni;
using System;
using System.Threading.Tasks;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// Base class for ubi.ini game config view models
/// </summary>
/// <typeparam name="Handler">The config handler</typeparam>
public abstract class Config_UbiIni_BaseViewModel<Handler> : GameOptionsDialog_ConfigPageViewModel
    where Handler : UbiIniHandler
{
    #region Constructor

    /// <summary>
    /// Default constructor
    /// </summary>
    /// <param name="game">The game</param>
    protected Config_UbiIni_BaseViewModel(Games game)
    {
        // Set properties
        Game = game;
        CanModifyGame = Services.File.CheckDirectoryWriteAccess(Game.GetInstallDir(false));

        if (!CanModifyGame)
            Logger.Info("The game {0} can't be modified", Game);
    }

    #endregion

    #region Logger

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    #endregion

    #region Public Properties

    /// <summary>
    /// The game
    /// </summary>
    public Games Game { get; }

    /// <summary>
    /// Indicates if the game can be modified
    /// </summary>
    public bool CanModifyGame { get; }

    #endregion

    #region Protected Properties

    /// <summary>
    /// The configuration data
    /// </summary>
    protected Handler ConfigData { get; set; }

    #endregion
        
    #region Protected Methods

    /// <summary>
    /// Loads and sets up the current configuration properties
    /// </summary>
    /// <returns>The task</returns>
    protected override async Task LoadAsync()
    {
        Logger.Info("{0} config is being set up", Game);

        // Run setup code
        await OnSetupAsync();

        // Load the configuration data
        ConfigData = await LoadConfigAsync();

        Logger.Info("The ubi.ini file has been loaded");

        // Keep track if the data had to be recreated
        bool recreated = false;

        // Re-create the section if it doesn't exist
        if (!ConfigData.Exists)
        {
            ConfigData.ReCreate();
            recreated = true;
            Logger.Info("The ubi.ini section for {0} was recreated", Game);
        }

        GraphicsMode.GetAvailableResolutions();

        // Import config data
        await ImportConfigAsync();

        // If the data was recreated we mark that there are unsaved changes available
        UnsavedChanges = recreated;

        Logger.Info("All section properties have been loaded");
    }

    /// <summary>
    /// Saves the changes
    /// </summary>
    /// <returns>The task</returns>
    protected override async Task<bool> SaveAsync()
    {
        Logger.Info("{0} configuration is saving...", Game);

        try
        {
            // Update the config data
            await UpdateConfigAsync();

            // Save the config data
            ConfigData.Save();

            Logger.Info("{0} configuration has been saved", Game);
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Saving ubi.ini data");
            await Services.MessageUI.DisplayExceptionMessageAsync(ex, String.Format(Resources.Config_SaveError, Game.GetGameInfo().DisplayName), Resources.Config_SaveErrorHeader);
            return false;
        }

        try
        {
            // Run save code
            await OnSaveAsync();
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "On save config");
            await Services.MessageUI.DisplayExceptionMessageAsync(ex, Resources.Config_SaveWarning, Resources.Config_SaveErrorHeader);

            return false;
        }

        return true;
    }

    /// <summary>
    /// Override to run on setup
    /// </summary>
    /// <returns>The task</returns>
    protected virtual Task OnSetupAsync() => Task.CompletedTask;

    /// <summary>
    /// Override to run on saving
    /// </summary>
    /// <returns>The task</returns>
    protected virtual Task OnSaveAsync() => Task.CompletedTask;

    #endregion

    #region Protected Abstract Methods

    /// <summary>
    /// Loads the <see cref="ConfigData"/>
    /// </summary>
    /// <returns>The config data</returns>
    protected abstract Task<Handler> LoadConfigAsync();

    /// <summary>
    /// Imports the <see cref="ConfigData"/>
    /// </summary>
    /// <returns>The task</returns>
    protected abstract Task ImportConfigAsync();

    /// <summary>
    /// Updates the <see cref="ConfigData"/>
    /// </summary>
    /// <returns>The task</returns>
    protected abstract Task UpdateConfigAsync();

    #endregion

    #region Protected Classes

    /// <summary>
    /// Provides support to duplicate a section
    /// in a ubi ini file
    /// </summary>
    protected class DuplicateSectionUbiIniHandler : UbiIniHandler
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="path">The path of the ubi.ini file</param>
        /// <param name="sectionKey">The name of the section to retrieve, usually the name of the game</param>
        public DuplicateSectionUbiIniHandler(FileSystemPath path, string sectionKey) : base(path, sectionKey)
        {

        }

        public void Duplicate(KeyDataCollection sectionData)
        {
            // Recreate the section
            ReCreate();

            // Add all new keys
            sectionData.ForEach(x => Section.AddKey(x));
        }
    }

    #endregion
}