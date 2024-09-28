using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Nito.AsyncEx;
using RayCarrot.RCP.Metro.Games.Clients;
using RayCarrot.RCP.Metro.Games.Clients.Data;
using RayCarrot.RCP.Metro.Games.Clients.DosBox;
using RayCarrot.RCP.Metro.Games.Components;
using RayCarrot.RCP.Metro.Games.Data;
using RayCarrot.RCP.Metro.Games.Tools.PrototypeRestoration;
using RayCarrot.RCP.Metro.Legacy.AppData;

namespace RayCarrot.RCP.Metro;

public class AppDataManager
{
    #region Constructor

    public AppDataManager(
        AppUserData data, 
        LaunchArguments args, 
        IMessenger messenger, 
        IMessageUIManager messageUi, 
        FileManager fileManager, 
        GameClientsManager gameClientsManager, 
        GamesManager gamesManager)
    {
        Data = data ?? throw new ArgumentNullException(nameof(data));
        Args = args ?? throw new ArgumentNullException(nameof(args));
        Messenger = messenger ?? throw new ArgumentNullException(nameof(messenger));
        MessageUI = messageUi ?? throw new ArgumentNullException(nameof(messageUi));
        FileManager = fileManager ?? throw new ArgumentNullException(nameof(fileManager));
        GameClientsManager = gameClientsManager ?? throw new ArgumentNullException(nameof(gameClientsManager));
        GamesManager = gamesManager ?? throw new ArgumentNullException(nameof(gamesManager));
    }

    #endregion

    #region Logger

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    #endregion

    #region Private Fields

    private readonly object _lock = new();
    private readonly AsyncLock _dataChangedHandlerAsyncLock = new();
    private readonly JsonSerializerSettings JsonSettings = new();
    private JObject? _dataJObject;

    #endregion

    #region Services

    private AppUserData Data { get; }
    private LaunchArguments Args { get; }
    private IMessenger Messenger { get; }
    private IMessageUIManager MessageUI { get; }
    private FileManager FileManager { get; }
    private GameClientsManager GameClientsManager { get; }
    private GamesManager GamesManager { get; }

    #endregion

    #region Private Properties

    private FileSystemPath PreviousBackupLocation { get; set; }

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

    private async Task MigrateToVersion14Async(JObject obj)
    {
        // Deserialize the legacy data to get removed/replaced app data properties
        LegacyPre14AppUserData? legacyData = obj.ToObject<LegacyPre14AppUserData>();

        if (legacyData == null)
        {
            Logger.Error("v14 data migration: Failed to deserialize legacy app data");
            await MessageUI.DisplayMessageAsync(Resources.AppData_MigrateError, MessageType.Error);
            return;
        }

        // Migrate DOSBox
        if (!legacyData.Emu_DOSBox_Path.IsNullOrWhiteSpace())
        {
            var dosBoxDescriptor = GameClientsManager.GetGameClientDescriptor<DosBoxGameClientDescriptor>();
            var location = InstallLocation.FromFilePath(legacyData.Emu_DOSBox_Path);
            await addGameClientAsync(dosBoxDescriptor, location, new ConfigureGameClientInstallation(x =>
            {
                // Set the config path if previously specified
                if (File.Exists(legacyData.Emu_DOSBox_ConfigPath))
                {
                    x.ModifyObject<DosBoxConfigFilePaths>(GameClientDataKey.DosBox_ConfigFilePaths,
                        y => y.FilePaths.Add(legacyData.Emu_DOSBox_Path));
                    Logger.Info("v14 data migration: Added config file path '{0}'", legacyData.Emu_DOSBox_Path);
                }
            }));
        }

        if (legacyData.Game_Games == null)
        {
            Logger.Error("v14 data migration: Deserialized games are null");
            return;
        }

        // Migrate games
        foreach (var game in legacyData.Game_Games)
        {
            string legacyId = game.Key;
            LegacyPre14AppUserData.GameData data = game.Value;

            var descriptors = GamesManager.GetGameDescriptorsFromLegacyId(legacyId);

            if (descriptors.Count == 1)
            {
                await addGameAsync(legacyId, descriptors.First(), data.InstallDirectory, data);
            }
            else if (descriptors.Count > 1)
            {
                // The only legacy games which map to more than one descriptor
                // should be Fiesta Run and the educational games
                if (legacyId == "RaymanFiestaRun")
                {
                    GameDescriptor? descriptor = legacyData.Game_FiestaRunVersion switch
                    {
                        LegacyPre14AppUserData.FiestaRunEdition.Default => descriptors.FirstOrDefault(x =>
                            x is GameDescriptor_RaymanFiestaRun_WindowsPackage),
                        LegacyPre14AppUserData.FiestaRunEdition.Preload => descriptors.FirstOrDefault(x =>
                            x is GameDescriptor_RaymanFiestaRun_PreloadEdition_WindowsPackage),
                        LegacyPre14AppUserData.FiestaRunEdition.Win10 => descriptors.FirstOrDefault(x =>
                            x is GameDescriptor_RaymanFiestaRun_Windows10Edition_WindowsPackage),
                        _ => null
                    };

                    if (descriptor == null)
                    {
                        Logger.Error("v14 data migration: No game descriptor found for Fiesta Run version {0}", legacyData.Game_FiestaRunVersion);
                        continue;
                    }

                    await addGameAsync(legacyId, descriptor, data.InstallDirectory, data);
                }
                else if (legacyId == "EducationalDos")
                {
                    if (legacyData.Game_EducationalDosBoxGames == null)
                    {
                        Logger.Error("v14 data migration: Educational games were added but the data is null");
                        continue;
                    }

                    Logger.Info("v14 data migration: Adding educational games");

                    foreach (var eduGame in legacyData.Game_EducationalDosBoxGames
                                 .GroupBy(x => x.InstallDir.FullPath.ToLowerInvariant())
                                 .Select(x => x.First()))
                    {
                        if (eduGame.LaunchName == null)
                        {
                            Logger.Error("v14 data migration: The launch name is null for educational game with id {0}", eduGame.ID);
                            continue;
                        }

                        GameDescriptor? descriptor = eduGame.LaunchName.IndexOf("qui", StringComparison.InvariantCultureIgnoreCase) != -1
                            ? descriptors.FirstOrDefault(x =>
                                x is GameDescriptor_RaymanEdutainmentQuiz_MsDos)
                            : descriptors.FirstOrDefault(
                                x => x is GameDescriptor_RaymanEdutainmentEdu_MsDos);

                        if (descriptor == null)
                        {
                            Logger.Error("v14 data migration: No game descriptor found for educational games with launch name {0} and if {1}", eduGame.LaunchName, eduGame.ID);
                            continue;
                        }

                        await addGameAsync(legacyId, descriptor, eduGame.InstallDir, data, x =>
                        {
                            if (!eduGame.Name.IsNullOrWhiteSpace())
                            {
                                x.SetValue(GameDataKey.RCP_CustomName, eduGame.Name);
                                Logger.Info("v14 data migration: Set the game name as {0}", eduGame.Name);
                            }

                            x.SetValue(GameDataKey.Client_DosBox_MountPath, eduGame.MountPath);
                            Logger.Info("v14 data migration: Set the mount path as {0}", eduGame.MountPath);
                        });
                    }
                }
                else
                {
                    Logger.Error("Unhandled game {0} mapped to multiple descriptors", legacyId);
                }
            }
            else
            {
                Logger.Error("Game {0} is not mapped to any descriptors", legacyId);
            }
        }

        // Originally the TPLS utility was migrated, but starting with 14.2.1 it has been replaced
        // with a new TPLS tool and the legacy TPLS utility should be uninstalled on update
        if (legacyData.Utility_TPLSData is { InstallDir.DirectoryExists: true })
        {
            Logger.Error("v14 data migration: Uninstalling TPLS");

            try
            {
                FileManager.DeleteDirectory(legacyData.Utility_TPLSData.InstallDir);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "v14 data migration: Uninstalling TPLS");
            }
        }

        // Migrate progression data sources
        if (legacyData.Backup_GameDataSources != null)
        {
            foreach (GameInstallation gameInstallation in GamesManager.GetInstalledGames())
            {
                var progressionManagers = gameInstallation.GetComponent<ProgressionManagersComponent>()?.CreateObject();

                if (progressionManagers == null) 
                    continue;
                
                foreach (GameProgressionManager progressionManager in progressionManagers)
                {
                    if (legacyData.Backup_GameDataSources.TryGetValue(progressionManager.ProgressionId,
                            out LegacyPre14AppUserData.ProgramDataSource src) &&
                        src != LegacyPre14AppUserData.ProgramDataSource.Auto)
                    {
                        gameInstallation.ModifyObject<ProgressionDataSources>(GameDataKey.Progression_DataSources,
                            y => y.DataSources[progressionManager.ProgressionId] = src switch
                            {
                                LegacyPre14AppUserData.ProgramDataSource.Auto => ProgramDataSource.Auto,
                                LegacyPre14AppUserData.ProgramDataSource.Default => ProgramDataSource.Default,
                                LegacyPre14AppUserData.ProgramDataSource.VirtualStore => ProgramDataSource.VirtualStore,
                                _ => ProgramDataSource.Auto
                            });
                    }
                }
            }

            Logger.Info("v14 data migration: Set progression data sources");
        }

        async Task addGameClientAsync(
            GameClientDescriptor descriptor,
            InstallLocation location,
            ConfigureGameClientInstallation? configureInstallation = null)
        {
            Logger.Info("v14 data migration: Adding legacy game client as {0} with path '{1}'", descriptor.GameClientId, location.FilePath);

            try
            {
                bool isValid = descriptor.IsValid(location);

                if (!isValid)
                {
                    Logger.Warn("v14 data migration: Could not add game client due to the location not being valid");
                    return;
                }

                GameClientInstallation gameClientInstallation =
                    await GameClientsManager.AddGameClientAsync(descriptor, location, configureInstallation);

                Logger.Info("v14 data migration: Added game client installation with id {0}", gameClientInstallation.InstallationId);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "v14 data migration: Adding game client");
            }
        }

        async Task addGameAsync(
            string legacyId,
            GameDescriptor descriptor,
            FileSystemPath installDir,
            LegacyPre14AppUserData.GameData data,
            Action<GameInstallation>? configureInstallation = null)
        {
            Logger.Info("v14 data migration: Adding legacy game {0} as {1} with path '{2}'", legacyId, descriptor.GameId, installDir);

            GameInstallation gameInstallation;
            try
            {
                InstallLocation location;

                // Rayman Garden PLUS was changed to a single-file game, so we need the file name
                if (legacyId == "RaymanGardenPLUS")
                    location = new InstallLocation(installDir, "rayman-garden-plus.exe");
                else
                    location = new InstallLocation(installDir);

                GameLocationValidationResult validationResult = descriptor.ValidateLocation(location, GameValidationFlags.All);

                if (!validationResult.IsValid)
                {
                    Logger.Warn("v14 data migration: Could not add game due to the location not being valid");
                    return;
                }

                gameInstallation = await GamesManager.AddGameAsync(descriptor, location, new ConfigureGameInstallation(x =>
                {
                    // Maintain option to run as admin for Win32 games
                    if (descriptor.Platform == GamePlatform.Win32 &&
                        data.LaunchMode == LegacyPre14AppUserData.GameLaunchMode.AsAdmin)
                    {
                        x.SetValue(GameDataKey.Win32_RunAsAdmin, true);
                        Logger.Info("v14 data migration: Set to run as admin");
                    }

                    // Set the install info if it was installed through RCP
                    if (legacyData.Game_InstalledGames?.Contains(legacyId) == true)
                    {
                        RCPGameInstallData.RCPInstallMode installMode = legacyId switch
                        {
                            "Rayman2" or "RaymanM" or "RaymanArena" => RCPGameInstallData.RCPInstallMode
                                .DiscInstall,
                            _ => RCPGameInstallData.RCPInstallMode.Download
                        };
                        RCPGameInstallData installData = new(installDir, installMode, DateTime.Now);
                        x.SetObject(GameDataKey.RCP_GameInstallData, installData);
                        Logger.Info("v14 data migration: Set the install data with mode {0}", installMode);
                    }

                    // Set the DOSBox mount path
                    if (legacyData.Game_DosBoxGames?.TryGetValue(legacyId, out LegacyPre14AppUserData.DosBoxOptions options) == true)
                    {
                        x.SetValue(GameDataKey.Client_DosBox_MountPath, options.MountPath);
                        Logger.Info("v14 data migration: Set the mount path as {0}", options.MountPath);
                    }

                    // Set the RRR2 launch mode
                    if (legacyId == "RaymanRavingRabbids2")
                    {
                        RaymanRavingRabbids2LaunchMode launchMode = legacyData.Game_RRR2LaunchMode switch
                        {
                            LegacyPre14AppUserData.RaymanRavingRabbids2LaunchMode.AllGames =>
                                RaymanRavingRabbids2LaunchMode.AllGames,
                            LegacyPre14AppUserData.RaymanRavingRabbids2LaunchMode.Orange =>
                                RaymanRavingRabbids2LaunchMode.Orange,
                            LegacyPre14AppUserData.RaymanRavingRabbids2LaunchMode.Red =>
                                RaymanRavingRabbids2LaunchMode.Red,
                            LegacyPre14AppUserData.RaymanRavingRabbids2LaunchMode.Green =>
                                RaymanRavingRabbids2LaunchMode.Green,
                            LegacyPre14AppUserData.RaymanRavingRabbids2LaunchMode.Blue =>
                                RaymanRavingRabbids2LaunchMode.Blue,
                            _ => RaymanRavingRabbids2LaunchMode.AllGames
                        };
                        x.SetValue(GameDataKey.RRR2_LaunchMode, launchMode);
                        Logger.Info("v14 data migration: Set the RRR3 launch mode as {0}", launchMode);
                    }

                    // Set if RRR Activity Center launch message has been shown
                    if (legacyId == "RaymanRavingRabbidsActivityCenter" &&
                        legacyData.Game_ShownRabbidsActivityCenterLaunchMessage)
                    {
                        x.SetValue(GameDataKey.RRRAC_ShownLaunchMessage, true);
                        Logger.Info("v14 data migration: Set that the RRR Activity Center launch message has been shown");
                    }

                    configureInstallation?.Invoke(x);
                }));

                Logger.Info("v14 data migration: Added game installation with id {gameInstallation.InstallationId}");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "v14 data migration: Adding game");
                return;
            }

            if (data.GameType is LegacyPre14AppUserData.GameType.DosBox or LegacyPre14AppUserData.GameType.EducationalDosBox)
            {
                try
                {
                    FileSystemPath oldConfigPath = AppFilePaths.UserDataBaseDir + "DosBox" + (legacyId + ".ini");
                    FileSystemPath newConfigPath = gameInstallation.GetRequiredComponent<DosBoxConfigFileComponent, AutoDosBoxConfigFileComponent>().CreateObject();

                    if (oldConfigPath.FileExists)
                        FileManager.MoveFile(oldConfigPath, newConfigPath, true);
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "v14 data migration: Moving DosBox auto config file");
                }
            }
        }
    }

    #endregion

    #region Public Methods

    public void Load()
    {
        try
        {
            _dataJObject = null;

            FileSystemPath filePath = AppFilePaths.AppUserDataPath;

            // Read the data from the file if it exists
            if (!Args.HasArg("-reset") && filePath.FileExists)
            {
                // Always reset the data first so any missing properties use the correct defaults
                Data.Reset();

                Data.App_LastVersion = null; // Need to set to null before calling JsonConvert.PopulateObject or else it's ignored

                _dataJObject = JsonConvert.DeserializeObject<JObject>(File.ReadAllText(filePath), JsonSettings);

                if (_dataJObject != null)
                {
                    using JsonReader reader = _dataJObject.CreateReader();
                    JsonSerializer.CreateDefault(JsonSettings).Populate(reader, Data);

                    Logger.Info("The app user data has been loaded");

                    // Verify the data
                    Data.Verify();
                }
                else
                {
                    // Since we set this to null we need to make sure it's not null when leaving this method
                    Data.App_LastVersion = new Version(0, 0, 0, 0);
                }
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

        // Rebuild game components
        foreach (GameInstallation gameInstallation in Data.Game_GameInstallations)
            gameInstallation.RebuildComponents();

        // Track changes to the user data
        Data.PropertyChanged += Data_PropertyChangedAsync;
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
                string json = JsonConvert.SerializeObject(Data, Formatting.Indented, JsonSettings);
                File.WriteAllText(AppFilePaths.AppUserDataPath, json);

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
        if (lastVersion < new Version(5, 0, 0, 0))
        {
            // If a Fiesta Run backup exists then it has to be renamed to use the new id based on version
            FileSystemPath oldBackupDir = Data.Backup_BackupLocation + GameBackups_Manager.BackupFamily + "Rayman Fiesta Run";

            if (oldBackupDir.DirectoryExists)
            {
                try
                {
                    // Get the previous Fiesta Run version
                    bool? isWin10 = _dataJObject?["IsFiestaRunWin10Edition"]?.Value<bool>();

                    if (isWin10 != null)
                    {
                        string backupId = isWin10.Value ? "Rayman Fiesta Run (Win10)" : "Rayman Fiesta Run (Default)";
                        FileSystemPath newBackupDir = Data.Backup_BackupLocation + GameBackups_Manager.BackupFamily + backupId;

                        FileManager.MoveDirectory(oldBackupDir, newBackupDir, true, true);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "Moving Fiesta Run backups to 5.0.0 standard");

                    await Services.MessageUI.DisplayMessageAsync(Resources.PostUpdate_MigrateFiestaRunBackup5Error, Resources.PostUpdate_MigrateBackupErrorHeader, MessageType.Error);
                }
            }
        }

        if (lastVersion < new Version(7, 0, 0, 0))
        {
            // Change the default user level from Normal to Advanced
            if (Data.App_UserLevel == UserLevel.Normal)
                Data.App_UserLevel = UserLevel.Advanced;
        }

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
        }

        if (lastVersion < new Version(13, 3, 0, 2))
        {
            try
            {
                // Default to the file type association being enabled
                new LegacyGamePatchFileLaunchHandler().AssociateWithFileType(Data.App_ApplicationPath, true);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Setting mod file type association");
            }

            try
            {
                // Default to the uri protocol association being enabled
                new GameBananaModFileUriLaunchHandler().AssociateWithUriProtocol(Data.App_ApplicationPath, true);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Setting mod uri protocol association");
            }
        }

        if (lastVersion < new Version(13, 4, 0, 0))
        {
            try
            {
                // Delete old R2 DRM removal utility backup files since it's now a mod
                Services.File.DeleteDirectory(AppFilePaths.UtilitiesBaseDir + "RemoveDRM");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Deleting old R2 DRM removal utility backup files");
            }
        }

        if (lastVersion < new Version(14, 0, 0, 0))
        {
            try
            {
                if (_dataJObject != null)
                    await MigrateToVersion14Async(_dataJObject);
                else
                    Logger.Error("Failed to migrate old data to 14.0 due to the serialized data not being available");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Migrating old data to 14.0");

                await MessageUI.DisplayExceptionMessageAsync(ex, Resources.AppData_MigrateError);
            }

            try
            {
                // Update the legacy game patch file association now that we have a custom icon for it
                LegacyGamePatchFileLaunchHandler handler = new();
                if (handler.IsAssociatedWithFileType() == true)
                    handler.AssociateWithFileType(Data.App_ApplicationPath, true);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Setting legacy game patch file type association");
            }
        }

        if (lastVersion < new Version(14, 0, 0, 5))
        {
            // The binary file editor path is no longer stored in its own value
            if (_dataJObject != null)
            {
                FileSystemPath binaryFileEditor = _dataJObject.Value<string>("Archive_BinaryEditorExe");

                if (binaryFileEditor != FileSystemPath.EmptyPath)
                    Data.FileEditors_AssociatedEditors[String.Empty] = binaryFileEditor;
            }
        }

        if (lastVersion < new Version(14, 1, 0, 0))
        {
            // The RRR Prototype Restoration data is now stored per game installation
            if (_dataJObject != null)
            {
                Dictionary<string, ToggleState>? toggleStates = _dataJObject["Mod_RRR_ToggleStates"]?.ToObject<Dictionary<string, ToggleState>>();
                Dictionary<int, Key>? keyboardButtonMapping = _dataJObject["Mod_RRR_KeyboardButtonMapping"]?.ToObject<Dictionary<int, Key>>();

                if (toggleStates != null && keyboardButtonMapping != null)
                {
                    foreach (GameInstallation gameInstallation in GamesManager.GetInstalledGames())
                    {
                        if (gameInstallation.GameDescriptor is { Game: Game.RaymanRavingRabbids, Platform: GamePlatform.Win32 })
                        {
                            gameInstallation.SetObject(GameDataKey.RRR_PrototypeRestorationData, new PrototypeRestorationData(toggleStates, keyboardButtonMapping));
                        }
                    }
                }
            }
        }

        if (lastVersion < new Version(14, 2, 0, 0))
        {
            try
            {
                // The admin worker has been removed, so delete the file if it's been deployed
                FileSystemPath adminWorkerFilePath = Environment.SpecialFolder.LocalApplicationData.GetFolderPath() +
                                                     "Rayman Control Panel\\RCP_Metro\\Temp\\AdditionalFiles\\Rayman Control Panel - Admin Worker.exe";
                FileManager.DeleteFile(adminWorkerFilePath);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Deleting admin worker file");
            }
        }

        if (lastVersion < new Version(14, 2, 1, 0))
        {
            // Uninstall legacy TPLS data
            foreach (GameInstallation gameInstallation in GamesManager.GetInstalledGames())
            {
                LegacyRayman1TplsData? legacyTplsData = gameInstallation.GetObject<LegacyRayman1TplsData>(GameDataKey.R1_LegacyTplsData);

                if (legacyTplsData == null) 
                    continue;
                
                try
                {
                    // Delete directory which contains the custom DOSBox build etc.
                    Services.File.DeleteDirectory(legacyTplsData.InstallDir);
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "Uninstalling TPLS");
                }

                // Remove data
                gameInstallation.SetObject<LegacyRayman1TplsData>(GameDataKey.R1_LegacyTplsData, null);

                // Remove the associated game client (the custom DOSBox build)
                if (legacyTplsData.GameClientInstallationId != null)
                    await Services.GameClients.RemoveGameClientAsync(legacyTplsData.GameClientInstallationId);

                Logger.Info("Legacy TPLS has been uninstalled for {0}", gameInstallation.FullId);
            }
        }
    }

    #endregion
}