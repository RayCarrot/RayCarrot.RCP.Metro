using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// The Rayman 1 Demo 1996/02/15 (MS-DOS) game descriptor
/// </summary>
public sealed class GameDescriptor_Rayman1_Demo_19960215_MSDOS : MSDOSGameDescriptor
{
    #region Protected Properties

    protected override string IconName => "Rayman1Demo";

    #endregion

    #region Public Properties

    public override string Id => "Rayman1_Demo_19960215_MSDOS";
    public override Game Game => Game.Rayman1;
    public override GameCategory Category => GameCategory.Demo;
    public override bool IsDemo => true;
    public override Games LegacyGame => Games.Demo_Rayman1_2;

    public override string DisplayName => "Rayman 1 Demo (1996/02/15)";
    public override string DefaultFileName => "RAYMAN.EXE";

    public override bool CanBeDownloaded => true;
    public override IList<Uri> DownloadURLs => new Uri[]
    {
        new(AppURLs.Games_R1Demo2_Url),
    };

    public override string ExecutableName => "RAYMAN.EXE";

    #endregion

    #region Public Methods

    public override FrameworkElement GetOptionsUI(GameInstallation gameInstallation) =>
        new GameOptions_DOSBox_Control(gameInstallation);

    public override GameOptionsDialog_ConfigPageViewModel GetConfigPageViewModel(GameInstallation gameInstallation) =>
        new Config_Rayman1_ViewModel(gameInstallation);

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