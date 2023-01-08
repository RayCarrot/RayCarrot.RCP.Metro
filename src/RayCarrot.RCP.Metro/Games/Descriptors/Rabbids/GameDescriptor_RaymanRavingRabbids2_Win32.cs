using RayCarrot.RCP.Metro.Games.Components;
using RayCarrot.RCP.Metro.Games.Data;
using RayCarrot.RCP.Metro.Games.Finder;
using RayCarrot.RCP.Metro.Games.Options;
using RayCarrot.RCP.Metro.Games.OptionsDialog;
using RayCarrot.RCP.Metro.Games.Structure;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// The Rayman Raving Rabbids 2 (Win32) game descriptor
/// </summary>
public sealed class GameDescriptor_RaymanRavingRabbids2_Win32 : Win32GameDescriptor
{
    #region Public Overrides

    public override string GameId => "RaymanRavingRabbids2_Win32";
    public override Game Game => Game.RaymanRavingRabbids2;
    public override GameCategory Category => GameCategory.Rabbids;
    public override LegacyGame? LegacyGame => Metro.LegacyGame.RaymanRavingRabbids2;

    public override LocalizedString DisplayName => "Rayman Raving Rabbids 2";
    public override DateTime ReleaseDate => new(2007, 01, 01); // Not exact

    public override GameIconAsset Icon => GameIconAsset.RaymanRavingRabbids2;

    #endregion

    #region Private Methods

    private static string GetLaunchArgs(GameInstallation gameInstallation)
    {
        RaymanRavingRabbids2LaunchMode launchMode = gameInstallation.GetValue(GameDataKey.RRR2_LaunchMode, RaymanRavingRabbids2LaunchMode.AllGames);
        return $"/{launchMode.ToString().ToLower()} /B Rrr2.bf";
    }

    private static IEnumerable<GameLinksComponent.GameUriLink> GetLocalGameLinks(GameInstallation gameInstallation)
    {
        RaymanRavingRabbids2LaunchMode launchMode = gameInstallation.GetValue(GameDataKey.RRR2_LaunchMode, RaymanRavingRabbids2LaunchMode.AllGames);

        return new[]
        {
            new GameLinksComponent.GameUriLink(
                Header: new ResourceLocString(nameof(Resources.GameLink_Setup)), 
                Uri: gameInstallation.InstallLocation + "SettingsApplication.exe",
                Arguments: $"/{launchMode.ToString().ToLower()}")
        };
    }

    #endregion

    #region Protected Methods

    protected override void RegisterComponents(IGameComponentBuilder builder)
    {
        base.RegisterComponents(builder);

        builder.Register(new GameOptionsComponent(x => new RaymanRavingRabbids2GameOptionsViewModel(x)));
        builder.Register(new ProgressionManagersComponent(x => new GameProgressionManager_RaymanRavingRabbids2(x, "Rayman Raving Rabbids 2")));
        builder.Register(new GameConfigComponent(x => new RaymanRavingRabbids2ConfigViewModel(x)));
        builder.Register<OnGameAddedComponent, AddToJumpListOnGameAddedComponent>();
        builder.Register(new LaunchArgumentsComponent(GetLaunchArgs));
        builder.Register(new LocalGameLinksComponent(GetLocalGameLinks));
    }

    protected override GameInstallationStructure GetStructure() => new(new GameInstallationPath[]
    {
        // Files
        new GameInstallationFilePath("Jade.exe", GameInstallationPathType.PrimaryExe, required: true),
    });

    #endregion

    #region Public Methods

    public override FinderQuery[] GetFinderQueries() => new FinderQuery[]
    {
        new UninstallProgramFinderQuery("Rayman Raving Rabbids 2"),
        new UninstallProgramFinderQuery("Rayman: Raving Rabbids 2"),
        new UninstallProgramFinderQuery("Rayman Raving Rabbids 2 Orange"),
        new UninstallProgramFinderQuery("Rayman: Raving Rabbids 2 Orange"),
        new UninstallProgramFinderQuery("RRR2"),

        new Win32ShortcutFinderQuery("Rayman Raving Rabbids 2"),
    };

    #endregion
}