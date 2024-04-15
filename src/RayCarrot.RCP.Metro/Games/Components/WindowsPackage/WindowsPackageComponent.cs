using System.Globalization;
using RayCarrot.RCP.Metro.Games.Components;
using Windows.ApplicationModel;
using Windows.Management.Deployment;

namespace RayCarrot.RCP.Metro;

[GameComponentBase(SingleInstance = true)]
public class WindowsPackageComponent : GameComponent
{
    public WindowsPackageComponent(string packageName, string fullPackageName)
    {
        PackageName = packageName;
        FullPackageName = fullPackageName;
    }

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    public string PackageName { get; }
    public string FullPackageName { get; }

    /// <summary>
    /// Gets the legacy launch path to use for launching the game. This method of launching should only be used when no
    /// other method is available. If the package is not found this method will launch a new Windows Explorer window
    /// instead. The entry point is defaulted to "!APP" and may not always be correct.
    /// </summary>
    public string LegacyLaunchPath => "shell:appsFolder\\" + $"{FullPackageName}!App";

    private IEnumerable<DuoGridItemViewModel> GetGameInfoItems(GameInstallation gameInstallation)
    {
        // Get the package
        Package? package = GetPackage();

        if (package == null)
        {
            Logger.Warn("Could not find the package for the game {0}", GameInstallation.FullId);
            return Enumerable.Empty<DuoGridItemViewModel>();
        }

        return new[]
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
        };
    }

    public override void RegisterComponents(IGameComponentBuilder builder)
    {
        base.RegisterComponents(builder);

        // Add a component for showing package info
        builder.Register(new GameInfoComponent(GetGameInfoItems));
    }

    /// <summary>
    /// Gets the game package
    /// </summary>
    /// <returns>The package or null if not found</returns>
    public Package? GetPackage() => new PackageManager().FindPackagesForUser(String.Empty).FirstOrDefault(x => x.Id.Name == PackageName);
}