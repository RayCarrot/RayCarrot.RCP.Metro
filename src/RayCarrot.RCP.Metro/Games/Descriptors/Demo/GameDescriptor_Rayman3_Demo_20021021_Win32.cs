using BinarySerializer.OpenSpace;
using RayCarrot.RCP.Metro.Archive.CPA;
using RayCarrot.RCP.Metro.Archive;
using RayCarrot.RCP.Metro.Games.Components;
using RayCarrot.RCP.Metro.Games.OptionsDialog;
using RayCarrot.RCP.Metro.Games.Structure;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// The Rayman 3 Demo 2002/10/21 (Win32) game descriptor
/// </summary>
public sealed class GameDescriptor_Rayman3_Demo_20021021_Win32 : Win32GameDescriptor
{
    #region Public Properties

    public override string GameId => "Rayman3_Demo_20021021_Win32";
    public override Game Game => Game.Rayman3;
    public override GameCategory Category => GameCategory.Rayman;
    public override bool IsDemo => true;
    public override LegacyGame? LegacyGame => Metro.LegacyGame.Demo_Rayman3_2;

    public override LocalizedString DisplayName => "Rayman 3 Demo (2002/10/21)";
    public override DateTime ReleaseDate => new(2002, 10, 21);

    public override GameIconAsset Icon => GameIconAsset.Rayman3_Demo;

    public override IEnumerable<string> DialogGroupNames => new[] { UbiIniFileGroupName };

    public override bool HasArchives => true;

    #endregion

    #region Protected Methods

    protected override void RegisterComponents(IGameComponentBuilder builder)
    {
        base.RegisterComponents(builder);

        builder.Register(new GameConfigComponent(x => new Rayman3ConfigViewModel(x)));
        builder.Register<LocalGameLinksComponent>(new Rayman3SetupLocalGameLinksComponent(true));
    }

    protected override GameInstallationStructure GetStructure() => new(new GameInstallationPath[]
    {
        // Files
        new GameInstallationFilePath("MainP5Pvf.exe", GameInstallationPathType.PrimaryExe, required: true),
    });

    #endregion

    #region Public Methods

    public override IEnumerable<GameAddAction> GetAddActions() => base.GetAddActions().Concat(new GameAddAction[]
    {
        new DownloadGameAddAction(this, new Uri[]
        {
            new(AppURLs.Games_R3Demo2_Url),
        })
    });

    public override IArchiveDataManager GetArchiveDataManager(GameInstallation? gameInstallation) =>
        new CPACntArchiveDataManager(
            settings: new OpenSpaceSettings(EngineVersion.Rayman3, BinarySerializer.OpenSpace.Platform.PC), 
            gameInstallation: gameInstallation,
            cpaTextureSyncData: null);

    public override IEnumerable<string> GetArchiveFilePaths(GameInstallation? gameInstallation) => new[]
    {
        //@"Gamedatabin\tex16.cnt", // TODO-14: Why is this commented out?
        @"Gamedatabin\tex32.cnt",
        @"Gamedatabin\vignette.cnt",
    };

    #endregion
}