#nullable disable
using IniParser.Model;
using NLog;
using System;
using System.Threading.Tasks;
using RayCarrot.RCP.Metro.Ini;

namespace RayCarrot.RCP.Metro.Games.OptionsDialog;

/// <summary>
/// Base class for ubi.ini game config view models
/// </summary>
/// <typeparam name="Handler">The config handler</typeparam>
public abstract class UbiIniBaseConfigViewModel<Handler> : ConfigPageViewModel
    where Handler : UbiIniData
{
    #region Constructor

    /// <summary>
    /// Default constructor
    /// </summary>
    /// <param name="gameInstallation">The game installation</param>
    protected UbiIniBaseConfigViewModel(GameInstallation gameInstallation)
    {
        // Set properties
        GameInstallation = gameInstallation;
        CanModifyGame = Services.File.CheckDirectoryWriteAccess(GameInstallation.InstallLocation);

        if (!CanModifyGame)
            Logger.Info("The game {0} can't be modified", GameInstallation.FullId);
    }

    #endregion

    #region Logger

    // ReSharper disable once StaticMemberInGenericType
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    #endregion

    #region Public Properties

    /// <summary>
    /// The game installation
    /// </summary>
    public GameInstallation GameInstallation { get; }

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

    protected override async Task LoadAsync()
    {
        Logger.Info("{0} config is being set up", GameInstallation.FullId);

        // Run setup code
        bool unsavedChanges = await OnSetupAsync();

        // Due to the ubi.ini file being located in the C:\Windows directory you normally don't have
        // access to it unless running a process as admin. To avoid having to always run RCP as admin
        // we have the user accept a one-time admin prompt which gives everyone full access to the
        // ubi.ini file. Previously this was done during the RCP app startup, but has now been moved
        // here so that it only prompts the user when they are to make a change to the file.
        await App.EnableUbiIniWriteAccessAsync();

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
            Logger.Info("The ubi.ini section for {0} was recreated", GameInstallation.FullId);
        }

        GraphicsMode.GetAvailableResolutions();

        // Import config data
        await ImportConfigAsync();

        // If the data was recreated we mark that there are unsaved changes available
        UnsavedChanges = recreated || unsavedChanges;

        Logger.Info("All section properties have been loaded");
    }

    /// <summary>
    /// Saves the changes
    /// </summary>
    /// <returns>The task</returns>
    protected override async Task<bool> SaveAsync()
    {
        Logger.Info("{0} configuration is saving...", GameInstallation.FullId);

        try
        {
            // Update the config data
            await UpdateConfigAsync();

            // Save the config data
            ConfigData.Save();

            Logger.Info("{0} configuration has been saved", GameInstallation.FullId);
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Saving ubi.ini data");
            await Services.MessageUI.DisplayExceptionMessageAsync(ex, String.Format(Resources.Config_SaveError, GameInstallation.GameDescriptor.DisplayName), Resources.Config_SaveErrorHeader);
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
    /// <returns>True if there are unsaved changes, otherwise false</returns>
    protected virtual Task<bool> OnSetupAsync() => Task.FromResult(false);

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
    /// Provides support to duplicate a section in a ubi ini file
    /// </summary>
    protected class DuplicateSectionUbiIniData : UbiIniData
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="path">The path of the ubi.ini file</param>
        /// <param name="sectionKey">The name of the section to retrieve, usually the name of the game</param>
        public DuplicateSectionUbiIniData(FileSystemPath path, string sectionKey) : base(path, sectionKey)
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