using RayCarrot.RCP.Metro.Games.Components;
using RayCarrot.RCP.Metro.Games.Finder;
using RayCarrot.RCP.Metro.Games.OptionsDialog;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// The Rabbids Go Home (Win32) game descriptor
/// </summary>
public sealed class GameDescriptor_RabbidsGoHome_Win32 : Win32GameDescriptor
{
    #region Public Override Properties

    public override string GameId => "RabbidsGoHome_Win32";
    public override Game Game => Game.RabbidsGoHome;
    public override GameCategory Category => GameCategory.Rabbids;
    public override LegacyGame? LegacyGame => Metro.LegacyGame.RabbidsGoHome;

    public override LocalizedString DisplayName => "Rabbids Go Home";
    public override string DefaultFileName => "Launcher.exe"; // TODO-14: This should be the same as GetLaunchFilePath()
    public override DateTime ReleaseDate => new(2009, 01, 01); // Not exact

    public override GameIconAsset Icon => GameIconAsset.RabbidsGoHome;

    #endregion

    #region Private Methods

    private static FileSystemPath GetLaunchFilePath(GameInstallation gameInstallation) =>
        gameInstallation.GetObject<UserData_RabbidsGoHomeLaunchData>(GameDataKey.RGH_LaunchData) == null
            ? "Launcher.exe"
            : "LyN_f.exe";

    private static string? GetLaunchArgs(GameInstallation gameInstallation) =>
        gameInstallation.GetObject<UserData_RabbidsGoHomeLaunchData>(GameDataKey.RGH_LaunchData)?.ToString();

    #endregion

    #region Protected Methods

    protected override void RegisterComponents(IGameComponentBuilder builder)
    {
        base.RegisterComponents(builder);

        builder.Register(new GameConfigComponent(x => new RabbidsGoHomeConfigViewModel(x)));
        builder.Register<OnGameAddedComponent, AddToJumpListOnGameAddedComponent>();
        builder.Register(new Win32LaunchPathComponent(GetLaunchFilePath));
        builder.Register(new LaunchArgumentsComponent(GetLaunchArgs));
    }

    #endregion

    #region Public Methods

    public override FinderQuery[] GetFinderQueries() => new FinderQuery[]
    {
        new UninstallProgramFinderQuery("Rabbids Go Home"),
        new UninstallProgramFinderQuery("Rabbids Go Home - DVD"),
        new UninstallProgramFinderQuery("Rabbids: Go Home"),

        new Win32ShortcutFinderQuery("Rabbids Go Home"),
    };

    #endregion
}