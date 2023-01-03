﻿using System.IO;
using RayCarrot.RCP.Metro.Games.Components;
using RayCarrot.RCP.Metro.Games.Finder;

namespace RayCarrot.RCP.Metro;

// TODO-14: We might need to disable inlining some places here so that this doesn't crash on Windows 7 where WinRT isn't supported
// TODO-14: Fix logs and comments due to rename from WinStore

/// <summary>
/// A game descriptor for a Windows package
/// </summary>
public abstract class WindowsPackageGameDescriptor : GameDescriptor
{
    #region Public Properties

    /// <summary>
    /// Indicates if th Windows runtime is supported on the current system
    /// </summary>
    public static bool SupportsWinRT => AppViewModel.WindowsVersion is >= WindowsVersion.Win8 or WindowsVersion.Unknown;

    public override GamePlatform Platform => GamePlatform.WindowsPackage;
    public override bool AllowPatching => false;

    public abstract string PackageName { get; }
    public abstract string FullPackageName { get; }

    #endregion

    #region Protected Methods

    protected override void RegisterComponents(IGameComponentBuilder builder)
    {
        base.RegisterComponents(builder);

        builder.Register<LaunchGameComponent, WindowsPackageLaunchGameComponent>();
        builder.Register(new WindowsPackageComponent(PackageName, FullPackageName));
    }

    protected override bool IsGameLocationValid(FileSystemPath installLocation)
    {
        // Make sure version is at least Windows 8
        if (!SupportsWinRT)
            return false;

        // Make sure the default game file is found
        return base.IsGameLocationValid(installLocation);
    }

    #endregion

    #region Public Methods

    public override FinderQuery[] GetFinderQueries()
    {
        // Make sure version is at least Windows 8
        if (!SupportsWinRT)
            return Array.Empty<FinderQuery>();

        return new FinderQuery[]
        {
            new WindowsPackageFinderQuery(PackageName),
        };
    }

    public GameBackups_Directory[] GetBackupDirectories() => new GameBackups_Directory[]
    {
        new(Environment.SpecialFolder.LocalApplicationData.GetFolderPath() + "Packages" + FullPackageName, SearchOption.AllDirectories, "*", "0", 0),
        new(GetLocalAppDataDirectory(), SearchOption.TopDirectoryOnly, "*", "0", 1)
    };

    public FileSystemPath GetLocalAppDataDirectory() => 
        Environment.SpecialFolder.LocalApplicationData.GetFolderPath() + "Packages" + FullPackageName + "LocalState";

    #endregion
}