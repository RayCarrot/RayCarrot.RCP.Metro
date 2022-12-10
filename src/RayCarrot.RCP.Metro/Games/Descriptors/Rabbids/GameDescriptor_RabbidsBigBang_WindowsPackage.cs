using System;
using System.Collections.Generic;
using RayCarrot.RCP.Metro.Games.Components;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// The Rabbids Big Bang (Windows Package) game descriptor
/// </summary>
public sealed class GameDescriptor_RabbidsBigBang_WindowsPackage : WindowsPackageGameDescriptor
{
    #region Private Constant Fields

    private const string MicrosoftStoreID = "9WZDNCRFJCS3";

    #endregion

    #region Public Properties

    public override string GameId => "RabbidsBigBang_WindowsPackage";
    public override Game Game => Game.RabbidsBigBang;
    public override GameCategory Category => GameCategory.Rabbids;
    public override LegacyGame? LegacyGame => Metro.LegacyGame.RabbidsBigBang;

    public override string DisplayName => "Rabbids Big Bang";
    public override string DefaultFileName => "Template.exe";
    public override DateTime ReleaseDate => new(2014, 03, 05);

    public override GameIconAsset Icon => GameIconAsset.RabbidsBigBang;

    public override string PackageName => "UbisoftEntertainment.RabbidsBigBang";
    public override string FullPackageName => "UbisoftEntertainment.RabbidsBigBang_dbgk1hhpxymar";

    #endregion

    #region Protected Methods

    protected override void RegisterComponents(DescriptorComponentBuilder builder)
    {
        base.RegisterComponents(builder);

        builder.Register(new ProgressionManagersComponent(x => new GameProgressionManager_RabbidsBigBang(this, x, "Rabbids Big Bang")));
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