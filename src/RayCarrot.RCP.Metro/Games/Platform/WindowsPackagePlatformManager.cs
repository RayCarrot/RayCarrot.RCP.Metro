namespace RayCarrot.RCP.Metro;

public class WindowsPackagePlatformManager : PlatformManager
{
    public WindowsPackagePlatformManager(WindowsPackageGameDescriptor gameDescriptor) : base(gameDescriptor)
    {
        GameDescriptor = gameDescriptor;
    }

    public override GamePlatform Platform => GamePlatform.WindowsPackage;
    public new WindowsPackageGameDescriptor GameDescriptor { get; }

    ///// <summary>
    ///// Gets the game package
    ///// </summary>
    ///// <returns>The package or null if not found</returns>
    //private Package? GetPackage()
    //{
    //    return new PackageManager().FindPackagesForUser(String.Empty).FirstOrDefault(x => x.Id.Name == GameDescriptor.PackageName);
    //}

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