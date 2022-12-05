using System;
using System.Collections.Generic;
using System.Linq;
using RayCarrot.RCP.Metro.Games.Options;

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
    public override LegacyGame? LegacyGame => Metro.LegacyGame.PrintStudio;

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

    public override IEnumerable<GameOptionsViewModel> GetOptionsViewModels(GameInstallation gameInstallation) =>
        base.GetOptionsViewModels(gameInstallation).Concat(new GameOptionsViewModel[]
        {
            new Rayman3PrintStudioGameOptionsViewModel(gameInstallation)
        });

    #endregion
}