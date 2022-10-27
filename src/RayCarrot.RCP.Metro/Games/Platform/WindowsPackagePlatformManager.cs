using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Windows.ApplicationModel;
using Windows.Management.Deployment;

namespace RayCarrot.RCP.Metro;

public class WindowsPackagePlatformManager : PlatformManager
{
    public WindowsPackagePlatformManager(WindowsPackageGameDescriptor gameDescriptor) : base(gameDescriptor)
    {
        GameDescriptor = gameDescriptor;
    }

    public override GamePlatform Platform => GamePlatform.WindowsPackage;
    public new WindowsPackageGameDescriptor GameDescriptor { get; }

    public override IEnumerable<DuoGridItemViewModel> GetGameInfoItems(GameInstallation gameInstallation)
    {
        // Get the package
        Package? package = GetPackage();

        if (package == null)
            return base.GetGameInfoItems(gameInstallation);

        return base.GetGameInfoItems(gameInstallation).Concat(new[]
        {
            new DuoGridItemViewModel(
                header: new ResourceLocString(nameof(Resources.GameInfo_WinStoreDependencies)),
                text: package.Dependencies.Select(x => x.Id.Name).JoinItems(", "),
                minUserLevel: UserLevel.Technical),
            new DuoGridItemViewModel(
                header: new ResourceLocString(nameof(Resources.GameInfo_WinStoreFullName)),
                text: package.Id.FullName,
                minUserLevel: UserLevel.Advanced),
            new DuoGridItemViewModel(
                header: new ResourceLocString(nameof(Resources.GameInfo_WinStoreArchitecture)),
                text: package.Id.Architecture.ToString(),
                minUserLevel: UserLevel.Technical),
            new DuoGridItemViewModel(
                header: new ResourceLocString(nameof(Resources.GameInfo_WinStoreVersion)),
                text: $"{package.Id.Version.Major}.{package.Id.Version.Minor}.{package.Id.Version.Build}.{package.Id.Version.Revision}",
                minUserLevel: UserLevel.Technical),
            new DuoGridItemViewModel(
                header: new ResourceLocString(nameof(Resources.GameInfo_WinStoreInstallDate)),
                text: new GeneratedLocString(() => package.InstalledDate.DateTime.ToString(CultureInfo.CurrentCulture)),
                minUserLevel: UserLevel.Advanced),
        });
    }

    /// <summary>
    /// Gets the game package
    /// </summary>
    /// <returns>The package or null if not found</returns>
    private Package? GetPackage()
    {
        return new PackageManager().FindPackagesForUser(String.Empty).FirstOrDefault(x => x.Id.Name == GameDescriptor.PackageName);
    }

    //public async Task LaunchAsync()
    //{
    //    // Launch the first app entry for the package
    //    Package? package = GetPackage();

    //    if (package == null)
    //    {
    //        return;
    //    }

    //    AppListEntry mainEntry = package.GetAppListEntries().First();
    //    await mainEntry.LaunchAsync();
    //}
}