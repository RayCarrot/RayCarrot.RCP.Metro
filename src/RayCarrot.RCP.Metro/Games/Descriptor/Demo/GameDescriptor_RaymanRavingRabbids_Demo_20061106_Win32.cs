using System;
using System.Collections.Generic;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// The Rayman Raving Rabbids Demo 2006/11/06 (Win32) game descriptor
/// </summary>
public sealed class GameDescriptor_RaymanRavingRabbids_Demo_20061106_Win32 : Win32GameDescriptor
{
    #region Protected Properties

    protected override string IconName => $"{Games.RaymanRavingRabbids}";

    #endregion

    #region Public Properties

    public override string Id => "RaymanRavingRabbids_Demo_20061106_Win32";
    public override Game Game => Game.RaymanRavingRabbids;
    public override GameCategory Category => GameCategory.Demo;
    public override bool IsDemo => true;
    public override Games LegacyGame => Games.Demo_RaymanRavingRabbids;

    public override string DisplayName => "Rayman Raving Rabbids Demo (2006/11/06)";
    public override string DefaultFileName => "Jade_enr.exe";

    public override bool CanBeDownloaded => true;
    public override IList<Uri> DownloadURLs => new Uri[]
    {
        new(AppURLs.Games_RRRDemo_Url),
    };

    #endregion

    #region Protected Methods

    protected override string GetLaunchArgs(GameInstallation gameInstallation) => "/B Rayman4.bf";

    #endregion

    #region Public Methods

    public override GameOptionsDialog_ConfigPageViewModel GetConfigPageViewModel(GameInstallation gameInstallation) =>
        new Config_RaymanRavingRabbidsDemo_ViewModel(gameInstallation);

    public override IEnumerable<GameFileLink> GetGameFileLinks(GameInstallation gameInstallation) => new GameFileLink[]
    {
        new(Resources.GameLink_Setup, gameInstallation.InstallLocation + "SettingsApplication.exe")
    };

    #endregion
}