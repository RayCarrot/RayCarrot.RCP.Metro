using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Nito.AsyncEx;
using RayCarrot.RCP.Metro.Patcher;

namespace RayCarrot.RCP.Metro;

public class AppDataManager
{
    #region Constructor

    public AppDataManager(
        AppUserData data, 
        LaunchArguments args, 
        GamesManager gamesManager, 
        IMessenger messenger, 
        IMessageUIManager messageUi, 
        FileManager fileManager)
    {
        Data = data ?? throw new ArgumentNullException(nameof(data));
        Args = args ?? throw new ArgumentNullException(nameof(args));
        GamesManager = gamesManager ?? throw new ArgumentNullException(nameof(gamesManager));
        Messenger = messenger ?? throw new ArgumentNullException(nameof(messenger));
        MessageUI = messageUi ?? throw new ArgumentNullException(nameof(messageUi));
        FileManager = fileManager ?? throw new ArgumentNullException(nameof(fileManager));
    }

    #endregion

    #region Logger

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    #endregion

    #region Private Fields

    private readonly object _lock = new();
    private readonly AsyncLock _dataChangedHandlerAsyncLock = new();

    #endregion

    #region Services

    private AppUserData Data { get; }
    private LaunchArguments Args { get; }
    private GamesManager GamesManager { get; }
    private IMessenger Messenger { get; }
    private IMessageUIManager MessageUI { get; }
    private FileManager FileManager { get; }

    #endregion

    #region Private Properties

    private FileSystemPath PreviousBackupLocation { get; set; }
    private UserData_LinkItemStyle PreviousLinkItemStyle { get; set; }

    #endregion

    #region Event Handlers

    private async void Data_PropertyChangedAsync(object sender, PropertyChangedEventArgs e)
    {
        // TODO: Eventually get rid of needing this
        using (await _dataChangedHandlerAsyncLock.LockAsync())
        {
            switch (e.PropertyName)
            {
                case nameof(AppUserData.Theme_DarkMode):
                case nameof(AppUserData.Theme_SyncTheme):
                    App.Current.SetTheme(Data.Theme_DarkMode, Data.Theme_SyncTheme);
                    break;

                case nameof(AppUserData.Backup_BackupLocation):
                    Messenger.Send<BackupLocationChangedMessage>();

                    if (!PreviousBackupLocation.DirectoryExists)
                    {
                        Logger.Info("The backup location has been changed, but the previous directory does not exist");
                        return;
                    }

                    Logger.Info("The backup location has been changed and old backups are being moved...");

                    await MoveBackupsAsync(PreviousBackupLocation, Data.Backup_BackupLocation);

                    PreviousBackupLocation = Data.Backup_BackupLocation;

                    break;

                case nameof(AppUserData.UI_LinkItemStyle):
                    static string GetStyleSource(UserData_LinkItemStyle linkItemStye) =>
                        $"{AppViewModel.WPFApplicationBasePath}/UI/Styles/LinkItem.{linkItemStye}.xaml";

                    // Get previous source
                    string oldSource = GetStyleSource(PreviousLinkItemStyle);

                    // Remove old source
                    foreach (ResourceDictionary resourceDictionary in App.Current.Resources.MergedDictionaries)
                    {
                        if (!String.Equals(resourceDictionary.Source?.ToString(), oldSource,
                                StringComparison.OrdinalIgnoreCase))
                            continue;

                        App.Current.Resources.MergedDictionaries.Remove(resourceDictionary);
                        break;
                    }

                    // Add new source
                    App.Current.Resources.MergedDictionaries.Add(new ResourceDictionary
                    {
                        Source = new Uri(GetStyleSource(Data.UI_LinkItemStyle))
                    });

                    PreviousLinkItemStyle = Data.UI_LinkItemStyle;

                    break;
            }
        }
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Attempts to move the backups from the old path to the new one
    /// </summary>
    /// <param name="oldPath">The old backup location</param>
    /// <param name="newPath">The new backup location</param>
    /// <returns>The task</returns>
    private async Task MoveBackupsAsync(FileSystemPath oldPath, FileSystemPath newPath)
    {
        if (!await MessageUI.DisplayMessageAsync(Resources.MoveBackups_Question, Resources.MoveBackups_QuestionHeader, MessageType.Question, true))
        {
            Logger.Info("Moving old backups has been canceled by the user");
            return;
        }

        try
        {
            // Get the complete paths
            FileSystemPath oldLocation = oldPath + GameBackups_Manager.BackupFamily;
            FileSystemPath newLocation = newPath + GameBackups_Manager.BackupFamily;

            // Make sure the old location has backups
            if (!oldLocation.DirectoryExists || !Directory.GetFileSystemEntries(oldLocation).Any())
            {
                Logger.Info("Old backups could not be moved due to not being found");

                await MessageUI.DisplayMessageAsync(String.Format(Resources.MoveBackups_NoBackupsFound, oldLocation.FullPath), Resources.MoveBackups_ErrorHeader, MessageType.Error);

                return;
            }

            // Make sure the new location doesn't already exist
            if (newLocation.DirectoryExists)
            {
                Logger.Info("Old backups could not be moved due to the new location already existing");

                await MessageUI.DisplayMessageAsync(String.Format(Resources.MoveBackups_BackupAlreadyExists, newLocation.FullPath), Resources.MoveBackups_ErrorHeader, MessageType.Error);
                return;
            }

            // Move the directory
            FileManager.MoveDirectory(oldLocation, newLocation, false, false);

            Logger.Info("Old backups have been moved");

            // Refresh backups
            Messenger.Send<BackupLocationChangedMessage>();

            await MessageUI.DisplaySuccessfulActionMessageAsync(Resources.MoveBackups_Success);
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Moving backups");
            await MessageUI.DisplayExceptionMessageAsync(ex, Resources.MoveBackups_Error, Resources.MoveBackups_ErrorHeader);
        }
    }

    #endregion

    #region Public Methods

    public void Load()
    {
        try
        {
            // Read the data from the file if it exists
            if (!Args.HasArg("-reset") && AppFilePaths.AppUserDataPath.FileExists)
            {
                // Always reset the data first so any missing properties use the correct defaults
                Data.Reset();

                Data.App_LastVersion = null; // Need to set to null before calling JsonConvert.PopulateObject or else it's ignored

                // Populate the data from the file
                JsonConvert.PopulateObject(File.ReadAllText(AppFilePaths.AppUserDataPath), Data);

                Logger.Info("The app user data has been loaded");

                // Verify the data
                Data.Verify();
            }
            else
            {
                // Reset the user data
                Data.Reset();

                Logger.Info("The app user data has been reset");
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Loading app user data");

            // NOTE: This is not localized due to the current culture not having been set at this point
            MessageBox.Show("An error occurred reading saved app data. The settings have been reset to their default values.", "Error",
                MessageBoxButton.OK, MessageBoxImage.Error);

            // Reset the user data
            Data.Reset();
        }

        string? assemblyPath = Assembly.GetEntryAssembly()?.Location;

        // Log some debug information
        Logger.Debug("Entry assembly path: {0}", assemblyPath);

        // Update the application path
        if (assemblyPath != Data.App_ApplicationPath)
        {
            Data.App_ApplicationPath = assemblyPath;
            Logger.Info("The application path has been updated");

            if (File.Exists(assemblyPath))
            {
                // If the file type association is set for patch files we need to update them
                if (PatchFile.IsAssociatedWithFileType() == true)
                {
                    try
                    {
                        PatchFile.AssociateWithFileType(assemblyPath, true);
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex, "Setting patch file type association");
                    }
                }

                if (PatchFile.IsAssociatedWithURIProtocol() == true)
                {
                    try
                    {
                        PatchFile.AssociateWithURIProtocol(assemblyPath, true);
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex, "Setting patch URI protocol association");
                    }
                }
            }
        }

        // Track changes to the user data
        Data.PropertyChanged += Data_PropertyChangedAsync;
        PreviousLinkItemStyle = Data.UI_LinkItemStyle;
        PreviousBackupLocation = Data.Backup_BackupLocation;
    }

    public void Save()
    {
        // Lock the saving of user data
        lock (_lock)
        {
            try
            {
                // Save the user data
                JsonHelpers.SerializeToFile(Data, AppFilePaths.AppUserDataPath);

                Logger.Info("The application user data was saved");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Saving user data");
            }
        }
    }

    public async Task PostUpdateAsync(Version lastVersion)
    {
        if (lastVersion < new Version(4, 0, 0, 6))
            Data.UI_EnableAnimations = true;

        if (lastVersion < new Version(4, 1, 1, 0))
            Data.App_ShowIncompleteTranslations = false;

        if (lastVersion < new Version(4, 5, 0, 0))
        {
            Data.UI_LinkItemStyle = UserData_LinkItemStyle.List;
            Data.App_ApplicationPath = Assembly.GetEntryAssembly()?.Location;
            Data.Update_ForceUpdate = false;
            Data.Update_GetBetaUpdates = false;
        }

        if (lastVersion < new Version(4, 6, 0, 0))
            Data.UI_LinkListHorizontalAlignment = HorizontalAlignment.Left;

        if (lastVersion < new Version(5, 0, 0, 0))
        {
            Data.Backup_CompressBackups = true;

            // Due to the fiesta run version system being changed the game has to be removed and then re-added
            // TODO-14: Restore this once we implement the app data migration
            // Data.Game_Games.Remove(Games.RaymanFiestaRun);

            // If a Fiesta Run backup exists the name needs to change to the new standard
            FileSystemPath fiestaBackupDir = Data.Backup_BackupLocation + GameBackups_Manager.BackupFamily + "Rayman Fiesta Run";

            if (fiestaBackupDir.DirectoryExists)
            {
                try
                {
                    // Read the app data file
                    JObject appData = new StringReader(File.ReadAllText(AppFilePaths.AppUserDataPath)).RunAndDispose(x =>
                        new JsonTextReader(x).RunAndDispose(y => JsonSerializer.Create().Deserialize(y))).CastTo<JObject>();

                    // Get the previous Fiesta Run version
                    bool? isWin10 = appData["IsFiestaRunWin10Edition"]?.Value<bool>();

                    if (isWin10 != null)
                    {
                        // TODO-14: Fix this migration
                        // Set the current edition
                        //Data.Game_FiestaRunVersion = isWin10.Value
                        //    ? UserData_FiestaRunEdition.Win10
                        //    : UserData_FiestaRunEdition.Default;

                        //Services.File.MoveDirectory(fiestaBackupDir, Data.Backup_BackupLocation + AppViewModel.BackupFamily + Games.RaymanFiestaRun.GetGameDescriptor().BackupName, true, true);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "Moving Fiesta Run backups to 5.0.0 standard");

                    await Services.MessageUI.DisplayMessageAsync(Resources.PostUpdate_MigrateFiestaRunBackup5Error, Resources.PostUpdate_MigrateBackupErrorHeader, MessageType.Error);
                }
            }

            // Remove old temp dir
            try
            {
                Services.File.DeleteDirectory(Path.Combine(Path.GetTempPath(), "RCP_Metro"));
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Cleaning pre-5.0.0 temp");
            }

            Data.Update_DisableDowngradeWarning = false;
        }

        if (lastVersion < new Version(6, 0, 0, 0))
            Data.Game_RabbidsGoHomeLaunchData = null;

        if (lastVersion < new Version(6, 0, 0, 2))
        {
            // TODO-14: Remove this?
            // By default, add all games to the jump list collection
            Data.App_JumpListItemIDCollection = GamesManager.GetInstalledGames().
                Select(x => x.GameDescriptor.GetJumpListItems(x).Select(y => y.ID)).
                SelectMany(x => x).
                ToList();
        }

        if (lastVersion < new Version(7, 0, 0, 0))
        {
            Data.Update_IsUpdateAvailable = false;

            if (Data.App_UserLevel == UserLevel.Normal)
                Data.App_UserLevel = UserLevel.Advanced;
        }

        if (lastVersion < new Version(7, 2, 0, 0))
            Data.Game_ShownRabbidsActivityCenterLaunchMessage = false;

        if (lastVersion < new Version(9, 0, 0, 0))
        {
            const string regUninstallKeyName = "RCP_Metro";

            // Since support has been removed for showing the program under installed programs we now have to remove the key
            string keyPath = RegistryHelpers.CombinePaths(AppFilePaths.UninstallRegistryKey, regUninstallKeyName);

            // Check if the key exists
            if (RegistryHelpers.KeyExists(keyPath))
            {
                try
                {
                    // Open the parent key
                    using RegistryKey? parentKey = RegistryHelpers.GetKeyFromFullPath(AppFilePaths.UninstallRegistryKey, RegistryView.Default, true);

                    // Delete the sub-key
                    parentKey?.DeleteSubKey(regUninstallKeyName);

                    Logger.Info("The program Registry key has been deleted");
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "Removing uninstall Registry key");

                    await Services.MessageUI.DisplayMessageAsync($"The Registry key {keyPath} could not be removed", MessageType.Error);
                }
            }

            if (Data.Utility_TPLSData != null)
            {
                Data.Utility_TPLSData.IsEnabled = false;
                await Services.MessageUI.DisplayMessageAsync(Resources.PostUpdate_TPLSUpdatePrompt);
            }
        }

        if (lastVersion < new Version(9, 4, 0, 0))
        {
            Data.Archive_GF_GenerateMipmaps = true;
            Data.Archive_GF_UpdateTransparency = UserData_Archive_GF_TransparencyMode.PreserveFormat;
        }

        if (lastVersion < new Version(9, 5, 0, 0))
            Data.Binary_BinarySerializationFileLogPath = FileSystemPath.EmptyPath;

        if (lastVersion < new Version(10, 0, 0, 0))
        {
            Data.Theme_SyncTheme = false;
            Data.App_HandleDownloadsManually = false;
        }

        if (lastVersion < new Version(10, 2, 0, 0))
            Data.Archive_GF_ForceGF8888Import = false;

        if (lastVersion < new Version(11, 0, 0, 0))
            Data.Archive_ExplorerSortOption = UserData_Archive_Sort.Default;

        if (lastVersion < new Version(11, 1, 0, 0))
        {
            Data.Archive_BinaryEditorExe = FileSystemPath.EmptyPath;
            Data.Archive_AssociatedPrograms = new Dictionary<string, FileSystemPath>();
        }

        if (lastVersion < new Version(11, 3, 0, 0))
            Data.Mod_RRR_KeyboardButtonMapping = new Dictionary<int, Key>();

        if (lastVersion < new Version(12, 0, 0, 0))
        {
            Data.App_DisableGameValidation = false;
            Data.UI_UseChildWindows = true;
        }

        if (lastVersion < new Version(13, 0, 0, 0))
        {
            Data.Progression_SaveEditorExe = FileSystemPath.EmptyPath;
            Data.Progression_ShownEditSaveWarning = false;
            Data.Backup_GameDataSources = new Dictionary<string, ProgramDataSource>();
            Data.Binary_IsSerializationLogEnabled = false;
            Data.Mod_RRR_ToggleStates = new Dictionary<string, UserData_Mod_RRR_ToggleState>();
        }

        if (lastVersion < new Version(13, 1, 0, 0))
            Data.Archive_CNT_SyncOnRepack = false;

        if (lastVersion < new Version(13, 3, 0, 0))
        {
            Data.Archive_CNT_SyncOnRepackRequested = false;
            Data.Patcher_LoadExternalPatches = true;
        }

        if (lastVersion < new Version(13, 3, 0, 2))
        {
            try
            {
                // Default to the file type association being enabled
                PatchFile.AssociateWithFileType(Data.App_ApplicationPath, true);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Setting patch file type association");
            }

            try
            {
                // Default to the URI protocol association being enabled
                PatchFile.AssociateWithURIProtocol(Data.App_ApplicationPath, true);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Setting patch URI protocol association");
            }
        }

        if (lastVersion < new Version(13, 4, 0, 0))
        {
            try
            {
                // Delete old R2 DRM removal utility backup files since it's now a patch
                Services.File.DeleteDirectory(AppFilePaths.UtilitiesBaseDir + "RemoveDRM");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Deleting old R2 DRM removal utility backup files");
            }
        }
    }

    #endregion
}