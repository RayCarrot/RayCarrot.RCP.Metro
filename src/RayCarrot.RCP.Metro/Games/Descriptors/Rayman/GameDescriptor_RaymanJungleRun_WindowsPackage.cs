using RayCarrot.RCP.Metro.Games.Components;
using RayCarrot.RCP.Metro.Games.OptionsDialog;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// The Rayman Jungle Run (Windows Package) game descriptor
/// </summary>
public sealed class GameDescriptor_RaymanJungleRun_WindowsPackage : WindowsPackageGameDescriptor
{
    #region Private Constant Fields

    private const string MicrosoftStoreID = "9WZDNCRFJ13P";

    #endregion

    #region Public Properties

    public override string GameId => "RaymanJungleRun_WindowsPackage";
    public override Game Game => Game.RaymanJungleRun;
    public override GameCategory Category => GameCategory.Rayman;
    public override LegacyGame? LegacyGame => Metro.LegacyGame.RaymanJungleRun;

    public override LocalizedString DisplayName => "Rayman Jungle Run";
    public override string DefaultFileName => "RO1Mobile.exe";
    public override DateTime ReleaseDate => new(2013, 03, 07);

    public override GameIconAsset Icon => GameIconAsset.RaymanJungleRun;

    public override string PackageName => "UbisoftEntertainment.RaymanJungleRun";
    public override string FullPackageName => "UbisoftEntertainment.RaymanJungleRun_dbgk1hhpxymar";

    #endregion

    #region Protected Methods

    protected override void RegisterComponents(IGameComponentBuilder builder)
    {
        base.RegisterComponents(builder);

        builder.Register(new ProgressionManagersComponent(x => new GameProgressionManager_RaymanJungleRun(this, x, "Rayman Jungle Run")));
        builder.Register(new GameConfigComponent(x => new RaymanJungleRunConfigViewModel(this, x)));
        builder.Register<OnGameAddedComponent, AddToJumpListOnGameAddedComponent>();
        builder.Register<ExternalGameLinksComponent>(new MicrosoftStoreExternalGameLinksComponent(MicrosoftStoreID));
    }

    #endregion
}