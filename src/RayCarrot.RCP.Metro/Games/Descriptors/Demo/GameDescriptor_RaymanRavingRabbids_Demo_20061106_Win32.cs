﻿using RayCarrot.RCP.Metro.Games.Components;
using RayCarrot.RCP.Metro.Games.Finder;
using RayCarrot.RCP.Metro.Games.OptionsDialog;
using RayCarrot.RCP.Metro.Games.Structure;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// The Rayman Raving Rabbids Demo 2006/11/06 (Win32) game descriptor
/// </summary>
public sealed class GameDescriptor_RaymanRavingRabbids_Demo_20061106_Win32 : Win32GameDescriptor
{
    #region Public Properties

    public override string GameId => "RaymanRavingRabbids_Demo_20061106_Win32";
    public override string LegacyGameId => "Demo_RaymanRavingRabbids";
    public override Game Game => Game.RaymanRavingRabbids;
    public override GameCategory Category => GameCategory.Rabbids;
    public override bool IsDemo => true;

    public override LocalizedString DisplayName => "Rayman Raving Rabbids Demo (2006/11/06)";
    public override DateTime ReleaseDate => new(2006, 11, 06);

    public override GameIconAsset Icon => GameIconAsset.RaymanRavingRabbids;
    public override GameBannerAsset Banner => GameBannerAsset.RaymanRavingRabbids;

    #endregion

    #region Private Methods

    private static string GetLaunchArgs(GameInstallation gameInstallation) => "/B Rayman4.bf";

    private static IEnumerable<GameLinksComponent.GameUriLink> GetLocalGameLinks(GameInstallation gameInstallation) => new[]
    {
        new GameLinksComponent.GameUriLink(
            Header: new ResourceLocString(nameof(Resources.GameLink_Setup)),
            Uri: gameInstallation.InstallLocation.Directory + "SettingsApplication.exe")
    };

    #endregion

    #region Protected Methods

    protected override void RegisterComponents(IGameComponentBuilder builder)
    {
        base.RegisterComponents(builder);

        builder.Register(new GameConfigComponent(x => new RaymanRavingRabbidsDemoConfigViewModel(x)));
        builder.Register(new LaunchArgumentsComponent(GetLaunchArgs));
        builder.Register(new LocalGameLinksComponent(GetLocalGameLinks));
        builder.Register<BinaryGameModeComponent>(new JadeGameModeComponent(JadeGameMode.RaymanRavingRabbids_PC));
    }

    protected override ProgramInstallationStructure GetStructure() => new(new GameInstallationPath[]
    {
        // Files
        new GameInstallationFilePath("Jade_enr.exe", GameInstallationPathType.PrimaryExe, required: true),
    });

    #endregion

    #region Public Methods

    public override IEnumerable<GameAddAction> GetAddActions() => base.GetAddActions().Concat(new GameAddAction[]
    {
        new DownloadGameAddAction(this, new Uri[]
        {
            new(AppURLs.Games_RRRDemo_Url),
        })
    });

    public override FinderQuery[] GetFinderQueries() => new FinderQuery[]
    {
        new PreviouslyDownloadedGameFinderQuery(GameId, LegacyGameId),
    };

    #endregion
}