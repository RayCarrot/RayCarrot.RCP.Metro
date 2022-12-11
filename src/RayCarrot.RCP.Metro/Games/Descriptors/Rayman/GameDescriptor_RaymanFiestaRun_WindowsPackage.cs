using System;
using System.Collections.Generic;
using RayCarrot.RCP.Metro.Games.Components;
using RayCarrot.RCP.Metro.Games.OptionsDialog;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// The Rayman Fiesta Run (Windows Package) game descriptor
/// </summary>
public sealed class GameDescriptor_RaymanFiestaRun_WindowsPackage : WindowsPackageGameDescriptor
{
    #region Private Constant Fields

    private const string MicrosoftStoreID = "9wzdncrdds0c";

    #endregion

    #region Public Properties

    public override string GameId => "RaymanFiestaRun_WindowsPackage";
    public override Game Game => Game.RaymanFiestaRun;
    public override GameCategory Category => GameCategory.Rayman;
    public override LegacyGame? LegacyGame => Metro.LegacyGame.RaymanFiestaRun;

    public override string DisplayName => "Rayman Fiesta Run";
    public override string DefaultFileName => "RFR_WinRT.exe";
    public override DateTime ReleaseDate => new(2014, 02, 12);

    public override GameIconAsset Icon => GameIconAsset.RaymanFiestaRun;

    public override string PackageName => "Ubisoft.RaymanFiestaRun";
    public override string FullPackageName => "Ubisoft.RaymanFiestaRun_ngz4m417e0mpw";

    #endregion

    #region Protected Methods

    protected override void RegisterComponents(DescriptorComponentBuilder builder)
    {
        base.RegisterComponents(builder);

        builder.Register(new ProgressionManagersComponent(x => new GameProgressionManager_RaymanFiestaRun(this, x, "Rayman Fiesta Run (Default)", 1)));
        builder.Register(new GameConfigComponent(x => new RaymanFiestaRunConfigViewModel(this)));

        builder.Register(new UtilityComponent(x => new Utility_RaymanFiestaRun_SaveFix(this, x, 1)));
    }

    #endregion

    #region Public Methods

    public override IEnumerable<GameUriLink> GetExternalUriLinks(GameInstallation gameInstallation) => new[]
    {
        new GameUriLink(
            Header: new ResourceLocString(nameof(Resources.GameDisplay_OpenInWinStore)),
            Uri: MicrosoftStoreHelpers.GetStorePageURI(MicrosoftStoreID),
            Icon: GenericIconKind.GameAction_Microsoft)
    };

    #endregion
}