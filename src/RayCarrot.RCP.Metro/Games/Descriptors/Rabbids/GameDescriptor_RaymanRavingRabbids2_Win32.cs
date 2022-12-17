using RayCarrot.RCP.Metro.Games.Components;
using RayCarrot.RCP.Metro.Games.Options;
using RayCarrot.RCP.Metro.Games.OptionsDialog;

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
    public override string DefaultFileName => "Jade.exe";
    public override DateTime ReleaseDate => new(2007, 01, 01); // Not exact

    public override GameIconAsset Icon => GameIconAsset.RaymanRavingRabbids2;

    #endregion

    #region Protected Methods

    protected override void RegisterComponents(DescriptorComponentBuilder builder)
    {
        base.RegisterComponents(builder);

        builder.Register(new GameOptionsComponent(x => new RaymanRavingRabbids2GameOptionsViewModel(x)));
        builder.Register(new ProgressionManagersComponent(x => new GameProgressionManager_RaymanRavingRabbids2(x, "Rayman Raving Rabbids 2")));
        builder.Register(new GameConfigComponent(x => new RaymanRavingRabbids2ConfigViewModel(x)));
    }

    protected override string GetLaunchArgs(GameInstallation gameInstallation)
    {
        UserData_RRR2LaunchMode launchMode = gameInstallation.GetValue(GameDataKey.RRR2_LaunchMode, UserData_RRR2LaunchMode.AllGames);
        return $"/{launchMode.ToString().ToLower()} /B Rrr2.bf";
    }

    #endregion

    #region Public Methods

    public override IEnumerable<GameUriLink> GetLocalUriLinks(GameInstallation gameInstallation)
    {
        UserData_RRR2LaunchMode launchMode = gameInstallation.GetValue(GameDataKey.RRR2_LaunchMode, UserData_RRR2LaunchMode.AllGames);

        return new GameUriLink[]
        {
            new(new ResourceLocString(nameof(Resources.GameLink_Setup)), gameInstallation.InstallLocation + "SettingsApplication.exe",
                Arguments: $"/{launchMode.ToString().ToLower()}")
        };
    }

    public override GameFinder_GameItem GetGameFinderItem() => new(null, "Rayman Raving Rabbids 2", new[]
    {
        "Rayman Raving Rabbids 2",
        "Rayman: Raving Rabbids 2",
        "Rayman Raving Rabbids 2 Orange",
        "Rayman: Raving Rabbids 2 Orange",
        "RRR2",
    });

    #endregion
}