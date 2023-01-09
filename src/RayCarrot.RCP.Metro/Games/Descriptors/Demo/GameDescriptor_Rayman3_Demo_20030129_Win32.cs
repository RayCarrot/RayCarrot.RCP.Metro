﻿using RayCarrot.RCP.Metro.Games.Components;
using RayCarrot.RCP.Metro.Games.OptionsDialog;
using RayCarrot.RCP.Metro.Games.Structure;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// The Rayman 3 Demo 2003/01/29 (Win32) game descriptor
/// </summary>
public sealed class GameDescriptor_Rayman3_Demo_20030129_Win32 : Win32GameDescriptor
{
    #region Public Properties

    public override string GameId => "Rayman3_Demo_20030129_Win32";
    public override Game Game => Game.Rayman3;
    public override GameCategory Category => GameCategory.Rayman;
    public override bool IsDemo => true;
    public override LegacyGame? LegacyGame => Metro.LegacyGame.Demo_Rayman3_4;

    public override LocalizedString DisplayName => "Rayman 3 Demo (2003/01/29)";
    public override DateTime ReleaseDate => new(2003, 01, 29);

    public override GameIconAsset Icon => GameIconAsset.Rayman3_Demo;

    public override IEnumerable<string> DialogGroupNames => new[] { UbiIniFileGroupName };

    #endregion

    #region Protected Methods

    protected override void RegisterComponents(IGameComponentBuilder builder)
    {
        base.RegisterComponents(builder);

        builder.Register(new GameConfigComponent(x => new Rayman3ConfigViewModel(x)));
        builder.Register<LocalGameLinksComponent>(new Rayman3SetupLocalGameLinksComponent(true));
        builder.Register<BinaryGameModeComponent>(new CPAGameModeComponent(CPAGameMode.Rayman3_PC));
        builder.Register<ArchiveComponent>(new CPAArchiveComponent(_ => new[]
        {
            //@"Gamedatabin\tex16.cnt", // TODO-14: Why is this commented out?
            @"Gamedatabin\tex32.cnt",
            @"Gamedatabin\vignette.cnt",
        }));
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
            new(AppURLs.Games_R3Demo4_Url),
        })
    });

    #endregion
}