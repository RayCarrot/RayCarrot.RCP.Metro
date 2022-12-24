using BinarySerializer.UbiArt;
using RayCarrot.RCP.Metro.Archive;
using RayCarrot.RCP.Metro.Archive.UbiArt;
using RayCarrot.RCP.Metro.Games.Components;
using RayCarrot.RCP.Metro.Games.OptionsDialog;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// The Rayman Legends (Steam) game descriptor
/// </summary>
public sealed class GameDescriptor_RaymanLegends_Steam : SteamGameDescriptor
{
    #region Public Properties

    public override string GameId => "RaymanLegends_Steam";
    public override Game Game => Game.RaymanLegends;
    public override GameCategory Category => GameCategory.Rayman;
    public override LegacyGame? LegacyGame => Metro.LegacyGame.RaymanLegends;

    public override LocalizedString DisplayName => "Rayman Legends";
    public override string DefaultFileName => "Rayman Legends.exe";
    public override System.DateTime ReleaseDate => new(2013, 08, 29);

    public override GameIconAsset Icon => GameIconAsset.RaymanLegends;
    public override GameBannerAsset Banner => GameBannerAsset.RaymanLegends;

    public override bool HasArchives => true;

    public override string SteamID => "242550";

    #endregion

    #region Protected Methods

    protected override void RegisterComponents(GameComponentBuilder builder)
    {
        base.RegisterComponents(builder);

        builder.Register(new ProgressionManagersComponent(x => new GameProgressionManager_RaymanLegends(x, "Rayman Legends")));
        builder.Register(new GameConfigComponent(x => new UbiArtConfigViewModel(x, AppFilePaths.RaymanLegendsRegistryKey)));
        builder.Register<OnGameAddedComponent, AddToJumpListOnGameAddedComponent>();

        builder.Register(new UtilityComponent(x => new Utility_RaymanLegends_UbiRay(x)));
        builder.Register(new UtilityComponent(x => new Utility_RaymanLegends_DebugCommands(x)));
    }

    #endregion

    #region Public Methods

    public override IArchiveDataManager GetArchiveDataManager(GameInstallation? gameInstallation) => 
        new UbiArtIPKArchiveDataManager(new UbiArtSettings(BinarySerializer.UbiArt.Game.RaymanLegends, BinarySerializer.UbiArt.Platform.PC), UbiArtIPKArchiveConfigViewModel.FileCompressionMode.WasCompressed);

    public override IEnumerable<string> GetArchiveFilePaths(GameInstallation? gameInstallation) => new[]
    {
        "Bundle_PC.ipk",
        "persistentLoading_PC.ipk",
    };

    #endregion
}