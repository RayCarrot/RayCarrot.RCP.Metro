using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;

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

    public override string DisplayName => "Rayman Demo (1996/02/15)";
    public override string DefaultFileName => "RAYMAN.EXE";

    public override GameIconAsset Icon => GameIconAsset.Rayman1_Demo;

    public override string ExecutableName => "RAYMAN.EXE";

    #endregion

    #region Public Methods

    public override IEnumerable<GameAddAction> GetAddActions() => new GameAddAction[]
    {
        new LocateRayman1MSDOSGameAddAction(this),
        new DownloadGameAddAction(this, new Uri[]
        {
            new(AppURLs.Games_R1Demo2_Url),
        })
    };

    public override FrameworkElement GetOptionsUI(GameInstallation gameInstallation) =>
        new GameOptions_DOSBox_Control(gameInstallation);

    public override GameOptionsDialog_ConfigPageViewModel GetConfigPageViewModel(GameInstallation gameInstallation) =>
        new Config_Rayman1_ViewModel(this, gameInstallation);

    public override async Task PostGameAddAsync(GameInstallation gameInstallation)
    {
        // Run base
        await base.PostGameAddAsync(gameInstallation);

        // Set the default mount path if available
        FileSystemPath mountPath = gameInstallation.InstallLocation + "Disc" + "RAY1DEMO.cue";

        if (mountPath.FileExists)
            gameInstallation.SetValue(GameDataKey.DOSBoxMountPath, mountPath);
    }

    #endregion
}