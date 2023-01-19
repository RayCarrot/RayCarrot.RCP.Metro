using RayCarrot.RCP.Metro.Games.Components;
using RayCarrot.RCP.Metro.Games.Options;
using RayCarrot.RCP.Metro.Games.Structure;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// The Rayman 1 minigames (Win32) game descriptor
/// </summary>
public sealed class GameDescriptor_Rayman1Minigames_Win32 : Win32GameDescriptor
{
    #region Public Override Properties

    public override string GameId => "Rayman1Minigames_Win32";
    public override string LegacyGameId => "Ray1Minigames";
    public override Game Game => Game.Rayman1Minigames;
    public override GameCategory Category => GameCategory.Other;

    public override LocalizedString DisplayName => "Rayman Minigames";
    public override DateTime ReleaseDate => new(1999, 07, 20);

    public override GameIconAsset Icon => GameIconAsset.Rayman1Minigames;
    public override GameBannerAsset Banner => GameBannerAsset.Rayman1;

    #endregion

    #region Protected Methods

    protected override void RegisterComponents(IGameComponentBuilder builder)
    {
        base.RegisterComponents(builder);

        builder.Register(new GameOptionsComponent(x => new Rayman1MinigamesGameOptionsViewModel(x)));
        builder.Register<OnGameAddedComponent, DefaultToRunAsAdminOnGameAddedComponent>();
    }

    protected override ProgramInstallationStructure GetStructure() => new(new GameInstallationPath[]
    {
        // Files
        new GameInstallationFilePath("RayGames.exe", GameInstallationPathType.PrimaryExe, required: true),
    });

    #endregion

    #region Public Methods

    // Can only be downloaded
    public override IEnumerable<GameAddAction> GetAddActions() => new GameAddAction[]
    {
        new DownloadGameAddAction(this, new Uri[]
        {
            new(AppURLs.Games_Ray1Minigames_Url),
        })
    };

    #endregion
}