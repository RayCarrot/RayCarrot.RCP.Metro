﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using BinarySerializer;
using NLog;
using RayCarrot.RCP.Metro.Patcher;

namespace RayCarrot.RCP.Metro;

public class InstalledGameViewModel : BaseViewModel
{
    #region Constructor

    public InstalledGameViewModel(GameInstallation gameInstallation)
    {
        // Set properties
        GameInstallation = gameInstallation;
        DisplayName = gameInstallation.GameDescriptor.DisplayName;

        // TODO-14: Don't hard-code WPF paths like this as they're hard to find when reorganizing solution
        // Get and set platform info
        GamePlatformInfoAttribute platformInfo = gameInstallation.GameDescriptor.Platform.GetInfo();
        PlatformDisplayName = platformInfo.DisplayName;
        PlatformIconSource = $"{AppViewModel.WPFApplicationBasePath}Img/GamePlatformIcons/{platformInfo.Icon.GetAttribute<ImageFileAttribute>()!.FileName}";

        string bannerFileName = gameInstallation.GameDescriptor.Banner.GetAttribute<ImageFileAttribute>()?.FileName ?? "Default.png";
        GameBannerImageSource = $"{AppViewModel.WPFApplicationBasePath}Img/GameBanners/{bannerFileName}";

        // TODO-UPDATE: Reload all of these when the game info changes since paths etc. might be different
        GamePanels = new ObservableCollection<GamePanelViewModel>();
        AddGamePanels();

        AdditionalLaunchActions = new ObservableActionItemsCollection();
        AddAdditionalLaunchActions();

        CanUninstall = gameInstallation.GetObject<UserData_RCPGameInstallInfo>(GameDataKey.RCPGameInstallInfo) != null;

        // Create commands
        LaunchCommand = new AsyncRelayCommand(LaunchAsync);
        OpenOptionsCommand = new AsyncRelayCommand(OpenOptionsAsync);
        RemoveCommand = new AsyncRelayCommand(RemoveAsync);
        UninstallCommand = new AsyncRelayCommand(UninstallAsync);
        CreateShortcutCommand = new AsyncRelayCommand(CreateShortcutAsync);
    }

    #endregion

    #region Logger

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    #endregion

    #region Commands

    public ICommand LaunchCommand { get; }
    public ICommand OpenOptionsCommand { get; }
    public ICommand RemoveCommand { get; }
    public ICommand UninstallCommand { get; }
    public ICommand CreateShortcutCommand { get; }

    #endregion

    #region Public Properties

    public GameInstallation GameInstallation { get; }
    public GameDescriptor GameDescriptor => GameInstallation.GameDescriptor;
    public LocalizedString DisplayName { get; }

    public LocalizedString PlatformDisplayName { get; }
    public string PlatformIconSource { get; }

    public string IconSource => GameDescriptor.IconSource;
    public bool IsDemo => GameDescriptor.IsDemo;
    public string GameBannerImageSource { get; }

    public ObservableCollection<GamePanelViewModel> GamePanels { get; }
    public ObservableActionItemsCollection AdditionalLaunchActions { get; }

    /// <summary>
    /// Indicates if the game can be uninstalled through the Rayman Control Panel
    /// </summary>
    public bool CanUninstall { get; }

    #endregion

    #region Private Methods

    private void AddGamePanels()
    {
        // Patcher
        if (GameDescriptor.AllowPatching)
            GamePanels.Add(new PatcherGamePanelViewModel(GameInstallation));
        
        // Archive Explorer
        if (GameDescriptor.HasArchives)
            GamePanels.Add(new ArchiveGamePanelViewModel(GameInstallation));

        // Progression
        foreach (GameProgressionManager progressionManager in GameDescriptor.GetGameProgressionManagers(GameInstallation))
            GamePanels.Add(new ProgressionGamePanelViewModel(GameInstallation, progressionManager));
    }

    private void AddAdditionalLaunchActions()
    {
        // TODO-UPDATE: Always show this option if available (Win32, but not for Steam, packaged etc.). Keep setting in options
        //              dialog for if should always run as admin by default.
        // Add run as admin option
        UserData_GameLaunchMode launchMode = GameInstallation.GetValue<UserData_GameLaunchMode>(GameDataKey.Win32LaunchMode);

        if (launchMode == UserData_GameLaunchMode.AsAdminOption)
            AdditionalLaunchActions.AddGroup(new IconCommandItemViewModel(
                header: Resources.GameDisplay_RunAsAdmin, 
                description: null,
                iconKind: GenericIconKind.GameDisplay_Admin, 
                command: new AsyncRelayCommand(async () => await GameDescriptor.LaunchGameAsync(GameInstallation, true))));

        // Add local uri links
        AdditionalLaunchActions.AddGroup(GameDescriptor.GetLocalUriLinks(GameInstallation).
            Where(x => File.Exists(x.Uri)).
            Select<GameDescriptor.GameUriLink, ActionItemViewModel>(x =>
            {
                // Get the path
                string path = x.Uri;

                // Create the command
                AsyncRelayCommand command = new(async () =>
                    (await Services.File.LaunchFileAsync(path, arguments: x.Arguments))?.Dispose());

                if (x.Icon != GenericIconKind.None)
                    return new IconCommandItemViewModel(
                        header: x.Header,
                        description: path,
                        iconKind: x.Icon,
                        command: command);

                try
                {
                    return new ImageCommandItemViewModel(
                        header: x.Header,
                        description: path,
                        imageSource: WindowsHelpers.GetIconOrThumbnail(path, ShellThumbnailSize.Small).ToImageSource(),
                        command: command);
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "Getting file icon for drop-down action item");

                    return new IconCommandItemViewModel(
                        header: x.Header,
                        description: path,
                        iconKind: GenericIconKind.None,
                        command: command);
                }

            }));

        // Add external uri links
        AdditionalLaunchActions.AddGroup(GameDescriptor.GetExternalUriLinks(GameInstallation).
            Select(x =>
            {
                // Get the path
                string path = x.Uri;

                // Create the command
                AsyncRelayCommand command = new(async () =>
                    (await Services.File.LaunchFileAsync(path, arguments: x.Arguments))?.Dispose());

                return new IconCommandItemViewModel(
                    header: x.Header,
                    description: path,
                    iconKind: x.Icon,
                    command: command);
            }));

        // Add RayMap link
        GameDescriptor.RayMapInfo? rayMapInfo = GameDescriptor.GetRayMapInfo();
        if (rayMapInfo != null)
        {
            string url = rayMapInfo.GetURL();
            AdditionalLaunchActions.AddGroup(new ImageCommandItemViewModel(
                header: Resources.GameDisplay_Raymap, 
                description: url,
                imageSource: (ImageSource)new ImageSourceConverter().ConvertFrom($"{AppViewModel.WPFApplicationBasePath}Img/RayMap/{rayMapInfo.Viewer}.png")!, 
                command: new AsyncRelayCommand(async () => (await Services.File.LaunchFileAsync(url))?.Dispose())));
        }

        // Add open location (don't add as a group since it's the last item)
        AdditionalLaunchActions.Add(new IconCommandItemViewModel(
            header: Resources.GameDisplay_OpenLocation, 
            description: GameInstallation.InstallLocation.FullPath,
            iconKind: GenericIconKind.GameDisplay_Location, 
            command: new AsyncRelayCommand(async () =>
            {
                // Get the install directory
                FileSystemPath instDir = GameInstallation.InstallLocation;

                // Select the file in Explorer if it exists
                if ((instDir + GameDescriptor.DefaultFileName).FileExists)
                    instDir += GameDescriptor.DefaultFileName;

                // Open the location
                await Services.File.OpenExplorerLocationAsync(instDir);

                Logger.Trace("The Game {0} install location was opened", GameInstallation.Id);
            })));
    }

    #endregion

    #region Public Methods

    public Task LoadAsync()
    {
        return Task.WhenAll(GamePanels.Select(x => x.LoadAsync()));
    }

    public Task LaunchAsync() => GameDescriptor.LaunchGameAsync(GameInstallation, false);
    public Task OpenOptionsAsync() => Services.UI.ShowGameOptionsAsync(GameInstallation);

    /// <summary>
    /// Removes the game from the program
    /// </summary>
    /// <returns>The task</returns>
    public async Task RemoveAsync()
    {
        // Ask the user
        if (!await Services.MessageUI.DisplayMessageAsync(String.Format(CanUninstall ? Resources.RemoveInstalledGameQuestion : Resources.RemoveGameQuestion, DisplayName), Resources.RemoveGameQuestionHeader, MessageType.Question, true))
            return;

        // Get applied utilities
        IList<string> appliedUtilities = await GameInstallation.GameDescriptor.GetAppliedUtilitiesAsync(GameInstallation);

        // TODO-UPDATE: This crashes for packaged apps since it tries to create the patch folder
        // Warn about applied utilities, if any
        if (appliedUtilities.Any() && !await Services.MessageUI.DisplayMessageAsync(
                $"{Resources.RemoveGame_UtilityWarning}{Environment.NewLine}{Environment.NewLine}" +
                $"{appliedUtilities.JoinItems(Environment.NewLine)}",
                Resources.RemoveGame_UtilityWarningHeader, MessageType.Warning, true))
            return;

        // Get applied patches
        using Context context = new RCPContext(String.Empty);
        PatchLibrary library = new(GameInstallation.InstallLocation, Services.File);
        PatchLibraryFile? libraryFile = null;

        try
        {
            libraryFile = context.ReadFileData<PatchLibraryFile>(library.LibraryFilePath);
        }
        catch (Exception ex)
        {
            Logger.Warn(ex, "Reading patch library");
        }

        // Warn about applied patches, if any
        if (libraryFile?.Patches.Any(x => x.IsEnabled) == true && !await Services.MessageUI.DisplayMessageAsync(String.Format(Resources.RemoveGame_PatchWarning, libraryFile.Patches.Count(x => x.IsEnabled)), MessageType.Warning, true))
            return;

        // Remove the game
        await Services.Games.RemoveGameAsync(GameInstallation);
    }

    /// <summary>
    /// Uninstalls the game
    /// </summary>
    /// <returns>The task</returns>
    public async Task UninstallAsync()
    {
        Logger.Info("{0} is being uninstalled...", GameInstallation.Id);

        // Have user confirm
        if (!await Services.MessageUI.DisplayMessageAsync(String.Format(Resources.UninstallGameQuestion, DisplayName), Resources.UninstallGameQuestionHeader, MessageType.Question, true))
        {
            Logger.Info("The uninstallation was canceled");

            return;
        }

        try
        {
            // Delete the game directory
            Services.File.DeleteDirectory(GameInstallation.InstallLocation);

            Logger.Info("The game install directory was removed");

            // Delete additional directories
            foreach (FileSystemPath dir in GameInstallation.GameDescriptor.UninstallDirectories)
                Services.File.DeleteDirectory(dir);

            Logger.Info("The game additional directories were removed");

            // Delete additional files
            foreach (FileSystemPath file in GameInstallation.GameDescriptor.UninstallFiles)
                Services.File.DeleteFile(file);

            Logger.Info("The game additional files were removed");
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Uninstalling game {0}", GameInstallation.Id);
            await Services.MessageUI.DisplayExceptionMessageAsync(ex, String.Format(Resources.UninstallGameError, DisplayName), Resources.UninstallGameErrorHeader);

            return;
        }

        // Remove the game
        await Services.Games.RemoveGameAsync(GameInstallation);

        await Services.MessageUI.DisplaySuccessfulActionMessageAsync(String.Format(Resources.UninstallGameSuccess, DisplayName), Resources.UninstallGameSuccessHeader);
    }

    /// <summary>
    /// Creates a shortcut to launch the game
    /// </summary>
    /// <returns>The task</returns>
    public async Task CreateShortcutAsync()
    {
        try
        {
            var result = await Services.BrowseUI.BrowseDirectoryAsync(new DirectoryBrowserViewModel()
            {
                DefaultDirectory = Environment.SpecialFolder.Desktop.GetFolderPath(),
                Title = Resources.GameShortcut_BrowseHeader
            });

            if (result.CanceledByUser)
                return;

            var shortcutName = String.Format(Resources.GameShortcut_ShortcutName, GameInstallation.GameDescriptor.DisplayName);

            GameInstallation.GameDescriptor.CreateGameShortcut(GameInstallation, shortcutName, result.SelectedDirectory);

            await Services.MessageUI.DisplaySuccessfulActionMessageAsync(Resources.GameShortcut_Success);
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Creating game shortcut {0}", GameInstallation.Id);
            await Services.MessageUI.DisplayExceptionMessageAsync(ex, Resources.GameShortcut_Error, Resources.GameShortcut_ErrorHeader);
        }
    }

    #endregion
}