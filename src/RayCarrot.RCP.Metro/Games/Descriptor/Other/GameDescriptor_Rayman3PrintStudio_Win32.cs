using System;
using System.Collections.Generic;
using System.Windows;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// The Rayman 3 Print Studio (Win32) game descriptor
/// </summary>
public sealed class GameDescriptor_Rayman3PrintStudio_Win32 : Win32GameDescriptor
{
    #region Public Properties

    public override string GameId => "Rayman3PrintStudio_Win32";
    public override Game Game => Game.Rayman3PrintStudio;
    public override GameCategory Category => GameCategory.Other;
    public override Games? LegacyGame => Games.PrintStudio;

    public override string DisplayName => "Rayman 3 Print Studio";
    public override string DefaultFileName => "Autorun.exe";

    public override GameIconAsset Icon => GameIconAsset.Rayman3PrintStudio;

    #endregion

    #region Public Methods

    // Can only be downloaded
    public override IEnumerable<GameAddAction> GetAddActions() => new GameAddAction[]
    {
        new DownloadGameAddAction(this, new Uri[]
        {
            new(AppURLs.Games_PrintStudio1_Url),
            new(AppURLs.Games_PrintStudio2_Url),
        })
    };

    public override FrameworkElement GetOptionsUI(GameInstallation gameInstallation) => 
        new GameOptions_PrintStudio_Control(new GameOptions_PrintStudio_ViewModel(gameInstallation));

    #endregion
}