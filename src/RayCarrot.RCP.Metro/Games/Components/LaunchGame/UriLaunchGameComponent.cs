﻿using System.Diagnostics;
using RayCarrot.RCP.Metro.Games.Structure;

namespace RayCarrot.RCP.Metro.Games.Components;

public abstract class UriLaunchGameComponent : LaunchGameComponent
{
    #region Logger

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    #endregion

    #region Private Methods

    private IEnumerable<DuoGridItemViewModel> GetGameInfoItems(GameInstallation gameInstallation)
    {
        string launchUri = GetLaunchUri();

        return new[]
        {
            new DuoGridItemViewModel(
                header: new ResourceLocString(nameof(Resources.GameInfo_LaunchUri)),
                text: launchUri,
                minUserLevel: UserLevel.Technical),
        };
    }

    #endregion

    #region Protected Methods

    protected abstract string GetLaunchUri();

    protected override async Task<bool> LaunchImplAsync()
    {
        // Get the uri
        string uri = GetLaunchUri();

        Logger.Trace("The game {0} is launching with the URI {1}", GameInstallation.FullId, uri);

        // Launch the game. The process this returns is the exe of the program associated with the uri.
        Process? process = await Services.File.LaunchURIAsync(uri);

        Logger.Info("The game {0} has been launched", GameInstallation.FullId);

        return process != null;
    }

    #endregion

    #region Public Methods

    public override void RegisterComponents(IGameComponentBuilder builder)
    {
        base.RegisterComponents(builder);

        // Register component for showing launch info
        builder.Register(new GameInfoComponent(GetGameInfoItems));
    }

    public override void CreateShortcut(FileSystemPath shortcutName, FileSystemPath destinationDirectory)
    {
        // Get the uri
        string uri = GetLaunchUri();

        Services.File.CreateURLShortcut(shortcutName, destinationDirectory, uri);

        Logger.Trace("An URL shortcut was created for {0} under {1}", GameInstallation.FullId, destinationDirectory);
    }

    public override IEnumerable<JumpListItemViewModel> GetJumpListItems() => new[]
    {
        new JumpListItemViewModel(
            gameInstallation: GameInstallation,
            name: GameInstallation.GetDisplayName(),
            iconSource: GameDescriptor.GetStructure<DirectoryProgramInstallationStructure>().FileSystem.GetAbsolutePath(GameInstallation, ProgramPathType.PrimaryExe),
            launchPath: GetLaunchUri(),
            workingDirectory: null,
            launchArguments: null,
            id: GameInstallation.InstallationId)
    };

    #endregion
}