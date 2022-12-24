using RayCarrot.RCP.Metro.Games.Components;
using RayCarrot.RCP.Metro.Games.OptionsDialog;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// The Rayman Raving Rabbids Demo 2006/11/06 (Win32) game descriptor
/// </summary>
public sealed class GameDescriptor_RaymanRavingRabbids_Demo_20061106_Win32 : Win32GameDescriptor
{
    #region Public Properties

    public override string GameId => "RaymanRavingRabbids_Demo_20061106_Win32";
    public override Game Game => Game.RaymanRavingRabbids;
    public override GameCategory Category => GameCategory.Rabbids;
    public override bool IsDemo => true;
    public override LegacyGame? LegacyGame => Metro.LegacyGame.Demo_RaymanRavingRabbids;

    public override LocalizedString DisplayName => "Rayman Raving Rabbids Demo (2006/11/06)";
    public override string DefaultFileName => "Jade_enr.exe";
    public override DateTime ReleaseDate => new(2006, 11, 06);

    public override GameIconAsset Icon => GameIconAsset.RaymanRavingRabbids;

    #endregion

    #region Protected Methods

    protected override void RegisterComponents(GameComponentBuilder builder)
    {
        base.RegisterComponents(builder);

        builder.Register(new GameConfigComponent(x => new RaymanRavingRabbidsDemoConfigViewModel(x)));
    }

    protected override string GetLaunchArgs(GameInstallation gameInstallation) => "/B Rayman4.bf";

    #endregion

    #region Public Methods

    public override IEnumerable<GameAddAction> GetAddActions() => base.GetAddActions().Concat(new GameAddAction[]
    {
        new DownloadGameAddAction(this, new Uri[]
        {
            new(AppURLs.Games_RRRDemo_Url),
        })
    });

    public override IEnumerable<GameUriLink> GetLocalUriLinks(GameInstallation gameInstallation) => new GameUriLink[]
    {
        new(new ResourceLocString(nameof(Resources.GameLink_Setup)), gameInstallation.InstallLocation + "SettingsApplication.exe")
    };

    #endregion
}