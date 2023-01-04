using RayCarrot.RCP.Metro.Games.Components;
using RayCarrot.RCP.Metro.Games.OptionsDialog;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// The Rayman 1 Demo 1996/02/15 (MS-DOS) game descriptor
/// </summary>
public sealed class GameDescriptor_Rayman1_Demo_19960215_MSDOS : MsDosGameDescriptor
{
    #region Public Properties

    public override string GameId => "Rayman1_Demo_19960215_MSDOS";
    public override Game Game => Game.Rayman1;
    public override GameCategory Category => GameCategory.Rayman;
    public override bool IsDemo => true;
    public override LegacyGame? LegacyGame => Metro.LegacyGame.Demo_Rayman1_2;

    public override LocalizedString DisplayName => "Rayman Demo (1996/02/15)";
    public override string DefaultFileName => "RAYMAN.EXE";
    public override DateTime ReleaseDate => new(1996, 02, 15);

    public override GameIconAsset Icon => GameIconAsset.Rayman1_Demo;

    #endregion

    #region Private Methods

    private static Task TryFindMountPath(GameInstallation gameInstallation)
    {
        // Set the default mount path if available
        FileSystemPath mountPath = gameInstallation.InstallLocation + "Disc" + "RAY1DEMO.cue";

        if (mountPath.FileExists)
            gameInstallation.SetValue(GameDataKey.Client_DosBox_MountPath, mountPath);

        return Task.CompletedTask;
    }

    #endregion

    #region Protected Methods

    protected override void RegisterComponents(IGameComponentBuilder builder)
    {
        base.RegisterComponents(builder);

        builder.Register(new GameConfigComponent(x => new Rayman1ConfigViewModel(this, x)));
        builder.Register(new OnGameAddedComponent(TryFindMountPath));
        builder.Register<MsDosGameRequiresDiscComponent>();
    }

    #endregion

    #region Public Methods

    public override IEnumerable<GameAddAction> GetAddActions() => base.GetAddActions().Concat(new GameAddAction[]
    {
        new DownloadGameAddAction(this, new Uri[]
        {
            new(AppURLs.Games_R1Demo2_Url),
        })
    });

    #endregion
}